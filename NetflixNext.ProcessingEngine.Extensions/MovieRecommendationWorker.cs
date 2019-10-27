using NetflixNext.ProcessingEngine.Extensions.Providers;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Engine.Projection;
using Sitecore.Processing.Engine.Storage.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetflixNext.ProcessingEngine.Extensions
{
    public class MovieRecommendationWorker : IDeferredWorker
    {
        public const string OptionSourceTableName = "sourceTableName";
        public const string OptionTargetTableName = "targetTableName";
        public const string OptionSchemaName = "schemaName";
        public const string OptionLimit = "limit";

        private readonly ITableStore _tableStore;
        private readonly IMovieRecommendationsProvider _movieRecommendationsProvider;
        private readonly string _sourceTableName;
        private readonly string _targetTableName;
        private readonly int _limit = 1;

        public MovieRecommendationWorker(ITableStoreFactory tableStoreFactory, IMovieRecommendationsProvider movieRecommendationsProvider, IReadOnlyDictionary<string, string> options)
        {
            _sourceTableName = options[OptionSourceTableName];
            _targetTableName = options[OptionTargetTableName];
            _limit = int.Parse(options[OptionLimit]);

            _movieRecommendationsProvider = movieRecommendationsProvider;

            var schemaName = options[OptionSchemaName];
            _tableStore = tableStoreFactory.Create(schemaName);
        }

        public void Dispose()
        {
            _tableStore.Dispose();
        }

        public async Task RunAsync(CancellationToken token)
        {
            var sourceRows = await _tableStore.GetRowsAsync(_sourceTableName, CancellationToken.None);
            var targetRows = new List<DataRow>();
            var targetSchema = new RowSchema(
                new FieldDefinition("ContactId", FieldKind.Key, FieldDataType.Guid),
                new FieldDefinition("MovieId", FieldKind.Key, FieldDataType.String),
                new FieldDefinition("Title", FieldKind.Attribute, FieldDataType.String),
                new FieldDefinition("Overview", FieldKind.Attribute, FieldDataType.String),
                new FieldDefinition("Image", FieldKind.Attribute, FieldDataType.String),
                new FieldDefinition("ImdbRating", FieldKind.Attribute, FieldDataType.String)
            );

            while (await sourceRows.MoveNext())
            {
                foreach (var row in sourceRows.Current)
                {
                    var movieIds = row["LastMoviesId"].ToString().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    //grabbing only unique movie ID's
                    movieIds = movieIds.Distinct().ToArray();

                    if (!movieIds.Any()) { continue;}
                    var recommendations = _movieRecommendationsProvider.GetRecommendations(movieIds);
                    
                    //clean recommendations to not have any dups
                    recommendations = recommendations.GroupBy(m => m.netflixid).Select(g => g.FirstOrDefault()).ToList();
                    if (recommendations.Any())
                    {
                        for (var i = 0; i < _limit; i++)
                        {
                            if (i < recommendations.Count)
                            {
                                var targetRow = new DataRow(targetSchema);
                                targetRow.SetGuid(0, row.GetGuid(0));
                                targetRow.SetString(1, recommendations[i].netflixid);
                                targetRow.SetString(2, recommendations[i].title);
                                targetRow.SetString(3, recommendations[i].synopsis);
                                targetRow.SetString(4, recommendations[i].image);
                                targetRow.SetString(5, recommendations[i].rating);
                                targetRows.Add(targetRow);
                            }
                        }
                    }
                }
            }

            var tableDefinition = new TableDefinition(_targetTableName, targetSchema);
            var targetTable = new InMemoryTableData(tableDefinition, targetRows);
            await _tableStore.PutTableAsync(targetTable, TimeSpan.FromMinutes(30), CancellationToken.None);
        }
    }
}
