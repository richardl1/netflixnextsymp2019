using Sitecore.Processing.Engine.ML.Abstractions;
using Sitecore.Processing.Engine.Projection;
using Sitecore.XConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetflixNext.ProcessingEngine.Extensions
{
    public class MovieRecommendationModel : IModel<Contact>
    {
        public const string OptionTableName = "tableName";
        //Replace the below Goal ID with your custom Goal ID
        public Guid MovieWatchedGoalId = new Guid("{56367C18-B211-431B-A2C7-975F9C59372F}");
        private readonly string _tableName;

        public MovieRecommendationModel(IReadOnlyDictionary<string, string> options)
        {
            _tableName = options[OptionTableName];
        }

        public IProjection<Contact> Projection =>
            Sitecore.Processing.Engine.Projection.Projection.Of<Contact>().CreateTabular(
                _tableName,
                contact =>
                    contact.Interactions.Select(interaction =>
                        new
                        {
                            Contact = contact,
                            Movies = contact.Interactions.SelectMany(i => i.Events.OfType<Goal>()).Where(g => g.DefinitionId.Equals(MovieWatchedGoalId)).OrderByDescending(g => g.Timestamp).Take(5).Select(x => x.CustomValues["movieId"])

                        }
                    ).Last(),
                cfg => cfg
                    .Key("ContactID", x => x.Contact.Id)
                    .Attribute("LastMoviesId", x => string.Join(",", x.Movies))
            );

        public Task<ModelStatistics> TrainAsync(string schemaName, CancellationToken cancellationToken, params TableDefinition[] tables)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<object>> EvaluateAsync(string schemaName, CancellationToken cancellationToken, params TableDefinition[] tables)
        {
            throw new NotImplementedException();
        }
        
    }
}
