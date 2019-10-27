using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetflixNext.ProcessingEngine.Extensions;
using NetflixNext.xConnect.Extensions;
using Sitecore.Framework.Messaging;
using Sitecore.Framework.Messaging.Rebus.Configuration;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Engine.Abstractions.Messages;
using Sitecore.Processing.Tasks.Messaging;
using Sitecore.Processing.Tasks.Messaging.Buses;
using Sitecore.Processing.Tasks.Messaging.Handlers;
using Sitecore.Processing.Tasks.Options.DataSources.Search;
using Sitecore.Processing.Tasks.Options.Workers.ML;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Schema;
using Sitecore.XConnect.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetflixNext.Client
{
    class Program
    {

        private readonly XdbModel _xDbModel;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _services;

        public Program(IConfiguration config)
        {
            _xDbModel = MovieModel.Model;
            _config = config;
            _services = CreateServices();
        }
        
        public async Task RunAsyncProgram()
        {
            System.Console.WriteLine("Netflix Next - Console");

            char option = (char)0;
            while (option != 'q')
            {
                PrintOptions();
                option = Console.ReadKey().KeyChar;
                Console.WriteLine();

                try
                {
                    switch (option)
                    {
                        case '1':
                            SerializeXConnectModel();
                            break;

                        case '2':
                            await RegisterRecommendationTaskAsync();
                            break;

                        case 'q':
                            return;

                        default:
                            Console.WriteLine("Unknown option");
                            break;
                    }
                }
                catch (Exception e)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    Console.ForegroundColor = color;
                }
            }
        }

        private void PrintOptions()
        {
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("1 - Serialize Model");
            Console.WriteLine("2 - Run Recommendation Handler");
            Console.WriteLine();
            Console.WriteLine("q - Quit");
            Console.WriteLine();
        }

        /// <summary>
        /// This the the core method that runs the whole nine yards for Projection, Merge and Store Recommendations
        /// </summary>
        /// <returns></returns>
        private async Task RegisterRecommendationTaskAsync()
        {

            var taskManager = GetTaskManager();
            var xConnectClient = GetXConnectClient();
            var taskTimeout = TimeSpan.FromMinutes(20);
            var storageTimeout = TimeSpan.FromMinutes(30);
            //This ID should be swapped with your custom GOAL ID, should be ideally read from a constants file
            var goalId = new Guid("{56367C18-B211-431B-A2C7-975F9C59372F}");
            
            //Below will latest interactions on all contacts that belong to custom goal type.  The time here depends on frequency of run of your processing service
            var query = xConnectClient.Contacts.Where(contact =>
                contact.Interactions.Any(interaction =>
                    interaction.Events.OfType<Goal>().Any(x => x.DefinitionId == goalId) &&
                    interaction.EndDateTime > DateTime.UtcNow.AddMinutes(-120)
                )
            );
            
            var expandOptions = new ContactExpandOptions
            {
                Interactions = new RelatedInteractionsExpandOptions()
            };

            query = query.WithExpandOptions(expandOptions);

            var searchRequest = query.GetSearchRequest();

            // projection starts here 
            var dataSourceOptions = new ContactSearchDataSourceOptionsDictionary(
                searchRequest, 
                30, 
                50 
            );

            var projectionOptions = new ContactProjectionWorkerOptionsDictionary(
                typeof(MovieRecommendationModel).AssemblyQualifiedName, 
                storageTimeout, 
                "recommendation", 
                new Dictionary<string, string>
                {
                    { MovieRecommendationModel.OptionTableName, "contactMovies" }
                }
            );

            
            var projectionTaskId = await taskManager.RegisterDistributedTaskAsync(
                dataSourceOptions, 
                projectionOptions, 
                null, 
                taskTimeout 
            );
            
            //merge starts here 
            var mergeOptions = new MergeWorkerOptionsDictionary(
                "contactMoviesFinal", // tableName
                "contactMovies", // prefix
                storageTimeout, // timeToLive
                "recommendation" // schemaName
            );

            
            var mergeTaskId = await taskManager.RegisterDeferredTaskAsync(
                mergeOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    projectionTaskId
                },
                taskTimeout // expiresAfter
            );

            var workerOptions = new DeferredWorkerOptionsDictionary(
                typeof(MovieRecommendationWorker).AssemblyQualifiedName, // workerType
                new Dictionary<string, string> // options
                {
                    { MovieRecommendationWorker.OptionSourceTableName, "contactMoviesFinal" },
                    { MovieRecommendationWorker.OptionTargetTableName, "contactRecommendations" },
                    { MovieRecommendationWorker.OptionSchemaName, "recommendation" },
                    { MovieRecommendationWorker.OptionLimit, "20" }
                });


            //recommendation task
            var recommendationTaskId = await taskManager.RegisterDeferredTaskAsync(
                workerOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    mergeTaskId
                },
                taskTimeout // expiresAfter
            );

            //Facet storage
            var storageOptions = new DeferredWorkerOptionsDictionary(
                typeof(RecommendationFacetStorageWorker).AssemblyQualifiedName,
                new Dictionary<string, string>
                {
                    { RecommendationFacetStorageWorker.OptionTableName, "contactRecommendations" },
                    { RecommendationFacetStorageWorker.OptionSchemaName, "recommendation" }
                });

           var storageTask = await taskManager.RegisterDeferredTaskAsync(
                storageOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    recommendationTaskId
                },
                taskTimeout // expiresAfter
            );
        }
        /// <summary>
        /// Helper method that would serialize the model
        /// Model file will be generated in your Debug folder and should be copied to all locations applicable
        /// Custom Model should be copied to Processing Root, Indexing Root and XConnect Root under /App_Data/Models location
        /// </summary>
        private void SerializeXConnectModel()
        {
            var json = XdbModelWriter.Serialize(_xDbModel);

            var filename = _xDbModel + ".json";
            File.WriteAllText(filename, json);

            Console.WriteLine($"Serialized model name: {filename}");
        }
        /// <summary>
        /// Helper method to get XConnect Client object
        /// </summary>
        /// <returns></returns>
        private XConnectClient GetXConnectClient()
        {
            var xconnectConfig = new XConnectClientConfiguration(_xDbModel, new Uri("https://netflixnext.xconnect"));
            xconnectConfig.Initialize();

            return new XConnectClient(xconnectConfig);
        }
        /// <summary>
        /// Below method is used to get the task manager
        /// </summary>
        /// <returns></returns>
        private TaskManager GetTaskManager()
        {
            // Task registration bus
            var taskRegistrationSyncBus =
                _services.GetRequiredService<ISynchronizedMessageBusContext<IMessageBus<TaskRegistrationProducer>>>();

            // Task progress bus
            var taskProgressSyncBus =
                _services.GetRequiredService<ISynchronizedMessageBusContext<IMessageBus<TaskProgressProducer>>>();

            var options = new TaskManagerOptions(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
            var taskManager = new TaskManager(options, taskRegistrationSyncBus, taskProgressSyncBus);
            return taskManager;
        }
        /// <summary>
        /// Method is used to create service collection needed for Communication and Cortex core tasks
        /// </summary>
        /// <returns></returns>
        private IServiceProvider CreateServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();
            serviceCollection.AddLogging();
            
            var rebusConfigSection = _config.GetSection("rebus");
            serviceCollection.AddMessaging(config => config.AddBuses(rebusConfigSection, _ => { }));
            new RebusConfigurationServices(serviceCollection).AddSqlServerConfigurators();

            serviceCollection
                .AddSingleton<ISynchronizedMessageBusContext<IMessageBus<TaskRegistrationProducer>>,
                    SynchronizedMessageBusContext<IMessageBus<TaskRegistrationProducer>>>();

            serviceCollection
                .AddSingleton<ISynchronizedMessageBusContext<IMessageBus<TaskProgressProducer>>,
                    SynchronizedMessageBusContext<IMessageBus<TaskProgressProducer>>>();

            // message handlers
            serviceCollection.AddTransient(typeof(IMessageHandler<TaskRegistrationStatus>), typeof(TaskRegistrationStatusHandler));
            serviceCollection.AddTransient(typeof(IMessageHandler<TaskProgressResponse>), typeof(TaskProgressResponseHandler));

            return serviceCollection.BuildServiceProvider();
        }
    }
}
