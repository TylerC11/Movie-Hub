using Microsoft.EntityFrameworkCore;
using Movie_Hub.Data;
using Movie_Hub.Models;

namespace Movie_Hub.Services;

public class MovieService
{
    private readonly ImdbContext _context;

    public MovieService(ImdbContext context)
    {
        _context = context;
    }

    // Home page
    public List<Title> GetPopularMovies(int count = 50)
    {
        return _context.Titles
            .Include(t => t.Rating)
            .Include(t => t.Genres)
            .Where(t => t.TitleType == "movie" && t.Rating != null)
            .OrderByDescending(t => t.Rating!.NumVotes)
            .Take(count)
            .ToList();
    }

    // Search bar
    public List<Title> SearchByTitle(string query)
    {
        return _context.Titles
            .Include(t => t.Rating)
            .Include(t => t.Genres)
            .Where(t => t.TitleType == "movie"
                     && t.PrimaryTitle != null
                     && t.PrimaryTitle.Contains(query))
            .OrderByDescending(t => t.Rating!.NumVotes)
            .Take(100)
            .ToList();
    }

    // Filter page
    public List<Title> FilterMovies(
        string? genre = null,
        int? yearFrom = null,
        int? yearTo = null,
        double? minRating = null)
    {
        var query = _context.Titles
            .Include(t => t.Rating)
            .Include(t => t.Genres)
            .Where(t => t.TitleType == "movie" && t.Rating != null)
            .AsQueryable();

        if (!string.IsNullOrEmpty(genre))
            query = query.Where(t => t.Genres.Any(g => g.Name == genre));

        if (yearFrom.HasValue)
            query = query.Where(t => t.StartYear >= yearFrom.Value);

        if (yearTo.HasValue)
            query = query.Where(t => t.StartYear <= yearTo.Value);

        if (minRating.HasValue)
            query = query.Where(t => (double?)t.Rating!.AverageRating >= minRating.Value);

        return query
            .OrderByDescending(t => t.Rating!.AverageRating)
            .Take(100)
            .ToList();
    }

    // Movie details page
    public Title? GetMovieDetails(string titleId)
    {
        return _context.Titles
            .Include(t => t.Rating)
            .Include(t => t.Genres)
            .Include(t => t.TitleAliases)
            .Include(t => t.Names) // Directors
            .Include(t => t.Names1) // Writers
            .FirstOrDefault(t => t.TitleId == titleId);
    }


    public List<Principal> GetCast(string titleId)
    {
        return _context.Principals
            .Include(p => p.Name)
            .Where(p => p.TitleId == titleId)
            .OrderBy(p => p.Ordering)
            .ToList();
    }
}