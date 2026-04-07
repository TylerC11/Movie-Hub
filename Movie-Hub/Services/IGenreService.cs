using System.Collections.Generic;

namespace Movie_Hub.Services
{
    public interface IGenreService
    {
        List<string> GetAllGenres();
    }
}