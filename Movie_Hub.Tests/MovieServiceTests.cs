using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Movie_Hub.Data;
using Movie_Hub.Models;
using Movie_Hub.Services;

namespace Movie_Hub.Tests;

[TestClass]
[DoNotParallelize]
public class MovieServiceTests
{
    private ImdbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<ImdbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ImdbContext(options);

        // Seed genres
        var action = new Genre { GenreId = 1, Name = "Action" };
        var drama = new Genre { GenreId = 2, Name = "Drama" };
        context.Genres.AddRange(action, drama);

        // Seed titles
        var t1 = new Title
        {
            TitleId = "tt0001",
            TitleType = "movie",
            PrimaryTitle = "Action Movie",
            StartYear = 2010,
            Genres = new List<Genre> { action }
        };
        var t2 = new Title
        {
            TitleId = "tt0002",
            TitleType = "movie",
            PrimaryTitle = "Drama Film",
            StartYear = 2015,
            Genres = new List<Genre> { drama }
        };
        var t3 = new Title
        {
            TitleId = "tt0003",
            TitleType = "tvSeries",
            PrimaryTitle = "A TV Show",
            StartYear = 2018,
            Genres = new List<Genre> { drama }
        };
        context.Titles.AddRange(t1, t2, t3);

        context.Ratings.AddRange(
            new Rating { TitleId = "tt0001", AverageRating = 7.5m, NumVotes = 50000 },
            new Rating { TitleId = "tt0002", AverageRating = 8.2m, NumVotes = 20000 },
            new Rating { TitleId = "tt0003", AverageRating = 6.0m, NumVotes = 5000 }
        );

        context.SaveChanges();
        return context;
    }

    [TestMethod]
    public void GetPopularMovies_ReturnsOnlyMovies()
    {
        var service = new MovieService(GetContext());
        var results = service.GetPopularMovies();
        Assert.IsTrue(results.All(t => t.TitleType == "movie"));
    }

    [TestMethod]
    public void GetPopularMovies_OrderedByNumVotes()
    {
        var service = new MovieService(GetContext());
        var results = service.GetPopularMovies();
        Assert.AreEqual("tt0001", results.First().TitleId);
    }

    [TestMethod]
    public void SearchByTitle_ReturnsMatchingTitles()
    {
        var service = new MovieService(GetContext());
        var results = service.SearchByTitle("Action");
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("tt0001", results.First().TitleId);
    }

    [TestMethod]
    public void SearchByTitle_NoMatch_ReturnsEmpty()
    {
        var service = new MovieService(GetContext());
        var results = service.SearchByTitle("Nonexistent");
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void FilterMovies_ByGenre_ReturnsCorrectMovies()
    {
        var service = new MovieService(GetContext());
        var results = service.FilterMovies(genre: "Action");
        Assert.IsTrue(results.All(t => t.Genres.Any(g => g.Name == "Action")));
    }

    [TestMethod]
    public void FilterMovies_ByYear_ReturnsCorrectMovies()
    {
        var service = new MovieService(GetContext());
        var results = service.FilterMovies(yearFrom: 2013);
        Assert.IsTrue(results.All(t => t.StartYear >= 2013));
    }

    [TestMethod]
    public void FilterMovies_ByMinRating_ExcludesLowRated()
    {
        var service = new MovieService(GetContext());
        var results = service.FilterMovies(minRating: 8.0);
        Assert.IsTrue(results.All(t => t.Rating!.AverageRating >= 8.0m));
    }

    [TestMethod]
    public void GetMovieDetails_ReturnsCorrectTitle()
    {
        var service = new MovieService(GetContext());
        var result = service.GetMovieDetails("tt0001");
        Assert.IsNotNull(result);
        Assert.AreEqual("Action Movie", result.PrimaryTitle);
    }

    [TestMethod]
    public void GetMovieDetails_InvalidId_ReturnsNull()
    {
        var service = new MovieService(GetContext());
        var result = service.GetMovieDetails("tt9999");
        Assert.IsNull(result);
    }
}