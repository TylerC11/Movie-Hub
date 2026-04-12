using Movie_Hub.Models;

namespace Movie_Hub.Services
{
    public interface IRecommendationService
    {
        List<Title> GetRecommendations(IEnumerable<Title> favourites, int count = 20);
    }
}