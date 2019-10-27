using System;
using System.Linq;
using Sitecore.Analytics;
using Sitecore.Common;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;

namespace NetflixSitecore.Services
{
    public class GoalsRepository : IGoalsRepository
    {
        public void TriggerGoal(ID goalId, string movieid, string genre, string rating)
        {
            Assert.IsNotNull(Tracker.Current, "Tracker.Current");
            Assert.IsNotNull(Tracker.Current.Session, "Tracker.Current.Session");
            Assert.IsNotNull(goalId, "goalId");

            using (XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
            {
                try
                {
                    var reference = new IdentifiedContactReference("xDB.Tracker", Tracker.Current.Session.Contact.ContactId.ToString("N"));
                    var actualContact = client.Get(reference, new ContactExpandOptions());

                    string userAgent = System.Web.HttpContext.Current?.Request.UserAgent;
                    var interaction = new Sitecore.XConnect.Interaction(actualContact, InteractionInitiator.Brand, ChannelIds.OnlineDirectChannelId, userAgent);

                    var goal = new Goal(goalId.ToGuid(), DateTime.UtcNow);
                    goal.CustomValues.Add("movieId", movieid);
                    goal.CustomValues.Add("rating", rating);
                    goal.CustomValues.Add("genre", genre);
                    interaction.Events.Add(goal);
                    client.AddInteraction(interaction);
                    client.Submit();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex, this);
                }
            }
        }

        public bool IsGoalTriggered(ID goalId, decimal hours)
        {
            Assert.IsNotNull(Tracker.Current, "Tracker.Current");
            Assert.IsNotNull(Tracker.Current.Session, "Tracker.Current.Session");
            Assert.IsNotNull(goalId, "goalId");
            Assert.IsNotNull(hours, "hours");

            using (XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
            {
                try
                {
                    var time = Decimal.ToDouble(hours);
                    var allInteractionsExpandOptions = new ContactExpandOptions()
                    {
                        Interactions = new RelatedInteractionsExpandOptions()
                        {
                            Limit = int.MaxValue
                        }
                    };

                    var reference = new IdentifiedContactReference("xDB.Tracker", Tracker.Current.Session.Contact.ContactId.ToString("N"));
                    var actualContact = client.Get(reference, allInteractionsExpandOptions);

                    var interactions = actualContact.Interactions;
                    var goals = interactions.Select(x => x.Events.OfType<Goal>()).SelectMany(x => x).OrderByDescending(x => x.Timestamp);

                    var desiredGoal = goals.FirstOrDefault(x => TypeExtensions.ToID(x.DefinitionId) == goalId);

                    return desiredGoal?.Timestamp.AddHours(time) >= DateTime.UtcNow;

                }
                catch (XdbExecutionException ex)
                {
                    Log.Error(ex.Message, ex, this);
                }
            }
            return false;
        }

        public static class GoalIds
        {
            public static readonly ID MovieWatchedId = new ID("{56367C18-B211-431B-A2C7-975F9C59372F}");

        }

        private static class ChannelIds
        {
            /// <summary>
            /// Gets the ID for Sitecore Channel: "/sitecore/system/Marketing Control Panel/Taxonomies/Channel/Online/Direct/Direct"
            /// </summary>
            public static readonly Guid OnlineDirectChannelId = new Guid("{B418E4F2-1013-4B42-A053-B6D4DCA988BF}");
        }
    }
}
