using Microsoft.EntityFrameworkCore;
using Movie_Hub.Data;
using Movie_Hub.Models;

namespace Movie_Hub.Services;

public class GenreService
{
    private readonly ImdbContext _context;

    public GenreService(ImdbContext context)
    {
        _context = context;
    }

    public List<string> GetAllGenres()
    {
        return _context.Genres
            .Select(g => g.Name)
            .Distinct()
            .OrderBy(g => g)
            .ToList();
    }
}