using Sitecore.Data;

namespace NetflixSitecore.Services
{
    public interface IGoalsRepository
    {
        void TriggerGoal(ID goalId, string movieid, string genre, string rating);
    }
}
