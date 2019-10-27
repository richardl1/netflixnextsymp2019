using Sitecore.Processing.Engine.Abstractions;
using System.Collections.Generic;

namespace NetflixNext.ProcessingEngine.Extensions
{
    public class MovieRecommendationWorkerOptionsDictionary : DeferredWorkerOptionsDictionary
    {
        public MovieRecommendationWorkerOptionsDictionary() : base(
            typeof(MovieRecommendationWorker).AssemblyQualifiedName, // workerType
            new Dictionary<string, string> // options
            {
                { MovieRecommendationWorker.OptionSourceTableName, "contactMoviesFinal" },
                { MovieRecommendationWorker.OptionTargetTableName, "contactRecommendations" },
                { MovieRecommendationWorker.OptionSchemaName, "recommendation" },
                { MovieRecommendationWorker.OptionLimit, "3" }
            })
        {
            
        }
    }
}
