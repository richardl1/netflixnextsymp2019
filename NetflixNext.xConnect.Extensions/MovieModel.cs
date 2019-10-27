using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;

namespace NetflixNext.xConnect.Extensions
{
    public class MovieModel
    {
        private static XdbModel _model;

        public static XdbModel Model => _model ?? (_model = InitModel());

        private static XdbModel InitModel()
        {
            var builder = new XdbModelBuilder("NetflixNextMovie", new XdbModelVersion(1, 0));

            builder.ReferenceModel(CollectionModel.Model);
            builder.DefineEventType<Goal>(false);
            builder.DefineFacet<Contact, MovieRecommendationFacet>(MovieRecommendationFacet.DefaultFacetKey);

            return builder.BuildModel();
        }
    }
}
