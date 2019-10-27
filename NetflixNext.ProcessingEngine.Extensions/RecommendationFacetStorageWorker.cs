using NetflixNext.Common.Models;
using NetflixNext.xConnect.Extensions;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Engine.Storage.Abstractions;
using Sitecore.XConnect;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetflixNext.ProcessingEngine.Extensions
{
    public class RecommendationFacetStorageWorker : IDeferredWorker
    {
        public const string OptionTableName = "tableName";
        public const string OptionSchemaName = "schemaName";

        private readonly string _tableName;
        private readonly ITableStore _tableStore;
        private readonly IXdbContext _xdbContext;

        public RecommendationFacetStorageWorker(ITableStoreFactory tableStoreFactory, IXdbContext xdbContext, IReadOnlyDictionary<string, string> options)
        {
            _tableName = options[OptionTableName];
            var schemaName = options[OptionSchemaName];
            _tableStore = tableStoreFactory.Create(schemaName);
            _xdbContext = xdbContext;
        }

        public void Dispose()
        {
            _tableStore.Dispose();
        }

        public async Task RunAsync(CancellationToken token)
        {
            var rows = await _tableStore.GetRowsAsync(_tableName, CancellationToken.None);

            while (await rows.MoveNext())
            {
                foreach (var row in rows.Current)
                {
                    var contactId = row.GetGuid(0);
                    var movieId = row.GetString(1);
                    var title = row.GetString(2);
                    var overview = row.GetString(3);
                    var image = row.GetString(4);
                    var rating = row.GetString(5);

                    var contact = await _xdbContext.GetContactAsync(contactId,
                        new ContactExpandOptions(MovieRecommendationFacet.DefaultFacetKey));

                    var facet = contact.GetFacet<MovieRecommendationFacet>(MovieRecommendationFacet.DefaultFacetKey) ??
                                new MovieRecommendationFacet();

                    if (facet.MovieRecommendations.All(x => x.netflixid != movieId))
                    {
                        facet.MovieRecommendations.Add(new Movie
                        {
                            netflixid = movieId,
                            title = title,
                            synopsis = overview,
                            image = image,
                            rating = rating
                        });
                    }
                        

                    _xdbContext.SetFacet(contact, MovieRecommendationFacet.DefaultFacetKey, facet);
                    await _xdbContext.SubmitAsync(CancellationToken.None);

                }
            }

            await _tableStore.RemoveAsync(_tableName, CancellationToken.None);
            System.Console.WriteLine("Finished Storing Recommendations for Contacts");
        }
    }
}
