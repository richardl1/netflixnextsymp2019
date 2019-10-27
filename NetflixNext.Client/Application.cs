using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
/// <summary>
/// Main Entry point of Client application
/// </summary>
namespace NetflixNext.Client
{
    class Application
    {
        static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddXmlFile("rebusSettings.xml", false);
            var config = configBuilder.Build();

            var app = new Program(config);
            Task.Run(() => app.RunAsyncProgram()).Wait();
        }
    }
}
