using System.Collections.Generic;
using Movie_Hub.Models;

namespace Movie_Hub.Services
{
    public interface IMovieService
    {
        List<Title> GetPopularMovies(int count = 50);
        List<Title> SearchByTitle(string query);
        List<Title> FilterMovies(string? genre = null, int? yearFrom = null,
                                 int? yearTo = null, double? minRating = null);
        Title? GetMovieDetails(string titleId);
        List<Principal> GetCast(string titleId);
    }
}