using Microsoft.EntityFrameworkCore;
using Movie_Hub.Data;
using Movie_Hub.Models;

namespace Movie_Hub.Services;

public class MovieService : IMovieService
{
    private readonly ImdbContext _context;

    // Simple in-memory cache — avoids re-querying every time Home is navigated to
    private List<Title>? _popularMoviesCache;

    public MovieService(ImdbContext context)
    {
        _context = context;
    }

    public List<Title> GetPopularMovies(int count = 250)
    {
        if (_popularMoviesCache != null)
            return _popularMoviesCache;

        _popularMoviesCache = _context.Titles
            .AsNoTracking()
            .Include(t => t.Rating)
            .Include(t => t.Genres)
            .Where(t => t.TitleType == "movie" && t.Rating != null)
            .OrderByDescending(t => t.Rating!.NumVotes)
            .Take(count)
            .ToList();

        return _popularMoviesCache;
    }

    public List<Title> SearchByTitle(string query)
    {
        return _context.Titles
            .AsNoTracking()
            .Include(t => t.Rating)
            .Include(t => t.Genres)
            .Where(t => t.TitleType == "movie"
                     && t.PrimaryTitle != null
                     && t.PrimaryTitle.Contains(query))
            .OrderByDescending(t => t.Rating!.NumVotes)
            .Take(200)
            .ToList();
    }

    public List<Title> FilterMovies(
        string? genre = null,
        int? yearFrom = null,
        int? yearTo = null,
        double? minRating = null,
        bool sortDescending = true)
    {
        var query = _context.Titles
            .AsNoTracking()
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

        query = sortDescending
            ? query.OrderByDescending(t => t.Rating!.AverageRating)
            : query.OrderBy(t => t.Rating!.AverageRating);

        return query.Take(200).ToList();
    }

    public Title? GetMovieDetails(string titleId)
    {
        return _context.Titles
            .AsNoTracking()
            .Include(t => t.Rating)
            .Include(t => t.Genres)
            .Include(t => t.TitleAliases)
            .Include(t => t.Names)
            .Include(t => t.Names1)
            .FirstOrDefault(t => t.TitleId == titleId);
    }

    public List<Principal> GetCast(string titleId)
    {
        return _context.Principals
            .AsNoTracking()
            .Include(p => p.Name)
            .Where(p => p.TitleId == titleId)
            .OrderBy(p => p.Ordering)
            .ToList();
    }
}