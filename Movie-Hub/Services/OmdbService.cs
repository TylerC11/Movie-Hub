using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Movie_Hub.Services
{
    // Fetches movie plot, cast, and poster from OMDb API when local database is missing data
    public class OmdbService
    {
        private readonly HttpClient _httpClient = new(); // Shared HTTP client for API requests
        private const string ApiKey = "24b3aa81"; // OMDb API key required for all requests

        // Returns the full plot of a movie by IMDB ID 
        public async Task<string?> GetPlotAsync(string imdbId)
        {
            var json = await FetchAsync(imdbId);
            if (json == null) return null;

            if (json.RootElement.TryGetProperty("Plot", out var plot))
            {
                var value = plot.GetString();
                return value == "null" ? null : value; // OMDb returns "null" when no plot exists
            }
            return null; // null if unavailable
        }

        // Returns a list of actors, directors, and writers for a movie by IMDB ID
        public async Task<List<OmdbCastMember>> GetCastAsync(string imdbId)
        {
            var result = new List<OmdbCastMember>();
            var json = await FetchAsync(imdbId);
            if (json == null) return result;

            // OMDb returns actors, directors, and writers as comma-separated strings
            if (json.RootElement.TryGetProperty("Actors", out var actors))
            {
                var actorList = actors.GetString();
                if (!string.IsNullOrEmpty(actorList) && actorList != "null")
                    foreach (var actor in actorList.Split(','))
                        result.Add(new OmdbCastMember { Name = actor.Trim(), Role = "Actor" });
            }

            if (json.RootElement.TryGetProperty("Director", out var director))
            {
                var directorName = director.GetString();
                if (!string.IsNullOrEmpty(directorName) && directorName != "null")
                    foreach (var d in directorName.Split(','))
                        result.Add(new OmdbCastMember { Name = d.Trim(), Role = "Director" });
            }

            if (json.RootElement.TryGetProperty("Writer", out var writer))
            {
                var writerName = writer.GetString();
                if (!string.IsNullOrEmpty(writerName) && writerName != "null")
                    foreach (var w in writerName.Split(','))
                        result.Add(new OmdbCastMember { Name = w.Trim(), Role = "Writer" });
            }

            return result;
        }

        // Returns the poster URL for a movie by IMDB ID
        public async Task<string?> GetPosterAsync(string imdbId)
        {
            var json = await FetchAsync(imdbId);
            if (json == null) return null;

            if (json.RootElement.TryGetProperty("Poster", out var poster))
            {
                var value = poster.GetString();
                return value == "N/A" ? null : value; // OMDb returns "N/A" when no poster exists
            }

            return null; // null if unavailable
        }

        // Calls the OMDb API and returns parsed JSON — shared by all fetch methods
        private async Task<JsonDocument?> FetchAsync(string imdbId)
        {
            try
            {
                var url = $"https://www.omdbapi.com/?i={imdbId}&plot=full&apikey={ApiKey}";
                var response = await _httpClient.GetStringAsync(url);
                return JsonDocument.Parse(response);
            }
            catch
            {
                return null; // Return null on any network or parsing error
            }
        }
    }

    // Represents a single cast member returned
    public class OmdbCastMember
    {
        public string Name { get; set; } = ""; // Full name of the person
        public string Role { get; set; } = ""; // Role: Actor, Director, or Writer
    }
}