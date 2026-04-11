using Microsoft.EntityFrameworkCore;
using Movie_Hub.Data;
using Movie_Hub.Models;

namespace Movie_Hub.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ImdbContext _context;

        public RecommendationService(ImdbContext context)
        {
            _context = context;
        }

        public List<Title> GetRecommendations(IEnumerable<Title> favourites, int count = 20)
        {
            var favouriteIds = favourites.Select(f => f.TitleId).ToHashSet();

            if (favouriteIds.Count == 0)
                return new List<Title>();

            var favouriteGenres = favourites
                .SelectMany(f => f.Genres.Select(g => g.Name))
                .Distinct()
                .ToList();

            if (favouriteGenres.Count == 0)
                return new List<Title>();

            // Let SQL do the filtering and sorting — only pull top candidates by rating,
            // then rank by genre overlap in memory on that small set
            var candidates = _context.Titles
                .AsNoTracking()
                .Include(t => t.Rating)
                .Include(t => t.Genres)
                .Where(t => t.TitleType == "movie"
                         && t.Rating != null
                         && !favouriteIds.Contains(t.TitleId)
                         && t.Genres.Any(g => favouriteGenres.Contains(g.Name)))
                .OrderByDescending(t => t.Rating!.AverageRating)
                .Take(100)  // fetch top 100 by rating, re-rank in memory
                .ToList();

            return candidates
                .OrderByDescending(t => t.Genres.Count(g => favouriteGenres.Contains(g.Name)))
                .ThenByDescending(t => t.Rating!.AverageRating)
                .Take(count)
                .ToList();
        }
    }
}