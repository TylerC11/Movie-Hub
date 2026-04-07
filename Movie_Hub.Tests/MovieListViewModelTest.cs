using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Movie_Hub.Models;
using Movie_Hub.Services;
using Movie_Hub.ViewModels;
using Movie_Hub.Commands;

namespace Movie_Hub.Tests
{
    // ══════════════════════════════════════════════════════════════════════════
    // FAKE SERVICES
    // These replicate the real service method signatures exactly — same parameter
    // types (int?, double?, string?) — but operate on in-memory data instead of
    // hitting the IMDB SQL database. No DbContext, no EF, no connection string.
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// In-memory stand-in for MovieService.
    /// Mirrors the exact method signatures:
    ///   GetPopularMovies(int count)
    ///   SearchByTitle(string query)
    ///   FilterMovies(string? genre, int? yearFrom, int? yearTo, double? minRating)
    /// </summary>
    internal class FakeMovieService : IMovieService
    {
        private readonly List<Title> _titles;

        public FakeMovieService()
        {
            _titles = new List<Title>
        {
            MakeTitle("tt0001", "The Dark Knight",  2008, 152, 9.0m,  2_500_000, "Action", "Crime", "Drama"),
            MakeTitle("tt0002", "Inception",        2010, 148, 8.8m,  2_200_000, "Action", "Sci-Fi"),
            MakeTitle("tt0003", "The Godfather",    1972, 175, 9.2m,  1_900_000, "Crime",  "Drama"),
            MakeTitle("tt0004", "Interstellar",     2014, 169, 8.6m,  1_700_000, "Adventure", "Drama", "Sci-Fi"),
            MakeTitle("tt0005", "Parasite",         2019, 132, 8.5m,    850_000, "Comedy", "Drama", "Thriller"),
            MakeTitle("tt0006", "Spirited Away",    2001, 125, 8.6m,    720_000, "Animation", "Adventure"),
            MakeTitle("tt0007", "Dark Waters",      2019, 126, 7.6m,     95_000, "Drama", "Thriller"),
            MakeTitle("tt0008", "A Low-Rated Film", 2015,  90, 4.1m,     12_000, "Horror"),
        };
        }

        public List<Title> GetPopularMovies(int count = 50)
            => _titles.OrderByDescending(t => t.Rating!.NumVotes).Take(count).ToList();

        public List<Title> SearchByTitle(string query)
            => _titles
                .Where(t => t.PrimaryTitle != null &&
                            t.PrimaryTitle.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.Rating!.NumVotes)
                .ToList();

        public List<Title> FilterMovies(string? genre = null, int? yearFrom = null,
                                        int? yearTo = null, double? minRating = null)
            => _titles
                .Where(t =>
                    (string.IsNullOrEmpty(genre) || t.Genres.Any(g => g.Name == genre)) &&
                    (!yearFrom.HasValue || t.StartYear >= yearFrom.Value) &&
                    (!yearTo.HasValue || t.StartYear <= yearTo.Value) &&
                    (!minRating.HasValue || (double?)t.Rating!.AverageRating >= minRating.Value))
                .OrderByDescending(t => t.Rating!.AverageRating)
                .ToList();

        public Title? GetMovieDetails(string titleId)
            => _titles.FirstOrDefault(t => t.TitleId == titleId);

        public List<Principal> GetCast(string titleId)
            => new List<Principal>();

        private static Title MakeTitle(string titleId, string name, short year,
            short runtime, decimal rating, int votes, params string[] genres)
        {
            var t = new Title
            {
                TitleId = titleId,
                TitleType = "movie",
                PrimaryTitle = name,
                StartYear = year,
                RuntimeMinutes = runtime,
                Rating = new Rating
                {
                    TitleId = titleId,
                    AverageRating = rating,
                    NumVotes = votes
                }
            };
            foreach (var g in genres)
                t.Genres.Add(new Genre { Name = g });
            return t;
        }
    }

    internal class FakeGenreService : IGenreService
    {
        public List<string> GetAllGenres()
            => new() { "Action", "Adventure", "Animation", "Comedy",
                   "Crime", "Drama", "Horror", "Sci-Fi", "Thriller" };
    }

    // ══════════════════════════════════════════════════════════════════════════
    // HELPER — builds a ViewModel with a no-op navigate command
    // ══════════════════════════════════════════════════════════════════════════

    internal static class VmFactory
    {
        private static readonly RelayCommand NoOpNavigate =
            new RelayCommand(_ => { }); // stands in for MainViewModel.NavigateDetailsCommand

        public static MovieListViewModel Create() =>
            new MovieListViewModel(new FakeMovieService(), new FakeGenreService(), NoOpNavigate);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // TESTS
    // ══════════════════════════════════════════════════════════════════════════

    [TestClass]
    public class MovieListViewModelTests
    {
        // ── Startup ───────────────────────────────────────────────────────────

        [TestMethod]
        public void OnConstruction_MoviesCollectionIsPopulated()
        {
            var vm = VmFactory.Create();
            Assert.IsTrue(vm.Movies.Count > 0,
                "Movies should be populated with popular movies on startup.");
        }

        [TestMethod]
        public void OnConstruction_GenresCollectionStartsWithAllGenresSentinel()
        {
            var vm = VmFactory.Create();
            Assert.IsTrue(vm.Genres.Count > 0, "Genres should be loaded on startup.");
            Assert.AreEqual("All Genres", vm.Genres[0],
                "First entry should be the 'All Genres' sentinel.");
        }

        [TestMethod]
        public void OnConstruction_StatusMessageReflectsLoadedCount()
        {
            var vm = VmFactory.Create();
            StringAssert.Contains(vm.StatusMessage, "found");
        }

        [TestMethod]
        public void OnConstruction_AllFilterPropertiesAreAtDefaults()
        {
            var vm = VmFactory.Create();
            Assert.AreEqual(string.Empty, vm.SearchText);
            Assert.IsNull(vm.SelectedGenre);
            Assert.IsNull(vm.YearFrom);
            Assert.IsNull(vm.YearTo);
            Assert.IsNull(vm.MinRating);
        }

        // ── Search ────────────────────────────────────────────────────────────

        [TestMethod]
        public void ExecuteSearch_PartialTitle_ReturnsMatchingMovies()
        {
            var vm = VmFactory.Create();
            vm.SearchText = "Dark";
            vm.ExecuteSearch();

            // "The Dark Knight" and "Dark Waters"
            Assert.AreEqual(2, vm.Movies.Count,
                "Search for 'Dark' should match exactly 2 titles.");
        }

        [TestMethod]
        public void ExecuteSearch_IsCaseInsensitive()
        {
            var vm = VmFactory.Create();
            vm.SearchText = "INCEPTION";
            vm.ExecuteSearch();

            Assert.AreEqual(1, vm.Movies.Count);
            Assert.AreEqual("Inception", vm.Movies[0].PrimaryTitle);
        }

        [TestMethod]
        public void ExecuteSearch_NoMatch_ReturnsEmptyCollection()
        {
            var vm = VmFactory.Create();
            vm.SearchText = "xyzzy_no_match";
            vm.ExecuteSearch();

            Assert.AreEqual(0, vm.Movies.Count);
        }

        [TestMethod]
        public void ExecuteSearch_EmptySearchText_DoesNotChangeMovieList()
        {
            // The guard inside ExecuteSearch() blocks empty queries.
            var vm = VmFactory.Create();
            int before = vm.Movies.Count;
            vm.SearchText = "   ";
            vm.ExecuteSearch();

            Assert.AreEqual(before, vm.Movies.Count,
                "Whitespace-only search should not alter the movie list.");
        }

        // ── Filter — genre ────────────────────────────────────────────────────

        [TestMethod]
        public void ExecuteFilter_ByGenre_OnlyReturnsMatchingGenre()
        {
            var vm = VmFactory.Create();
            vm.SelectedGenre = "Sci-Fi";
            vm.ExecuteFilter();

            Assert.IsTrue(vm.Movies.Count > 0);
            foreach (var m in vm.Movies)
                Assert.IsTrue(m.Genres.Any(g => g.Name == "Sci-Fi"),
                    $"'{m.PrimaryTitle}' does not contain genre 'Sci-Fi'.");
        }

        [TestMethod]
        public void ExecuteFilter_NullGenre_ReturnsAllMovies()
        {
            var vm = VmFactory.Create();
            vm.SelectedGenre = null;  // null = no genre filter
            vm.YearFrom = null;
            vm.YearTo = null;
            vm.MinRating = null;
            vm.ExecuteFilter();

            Assert.AreEqual(8, vm.Movies.Count,
                "Null genre filter should return all 8 test movies.");
        }

        // ── Filter — year ─────────────────────────────────────────────────────

        [TestMethod]
        public void ExecuteFilter_YearFromAndTo_ExcludesOutOfRangeMovies()
        {
            var vm = VmFactory.Create();
            vm.SelectedGenre = null;
            vm.YearFrom = 2010;
            vm.YearTo = 2015;
            vm.MinRating = null;
            vm.ExecuteFilter();

            foreach (var m in vm.Movies)
            {
                Assert.IsTrue(m.StartYear >= 2010,
                    $"'{m.PrimaryTitle}' ({m.StartYear}) is before YearFrom.");
                Assert.IsTrue(m.StartYear <= 2015,
                    $"'{m.PrimaryTitle}' ({m.StartYear}) is after YearTo.");
            }
        }

        [TestMethod]
        public void ExecuteFilter_OnlyYearFrom_ExcludesOlderMovies()
        {
            var vm = VmFactory.Create();
            vm.SelectedGenre = null;
            vm.YearFrom = 2010;
            vm.YearTo = null;
            vm.MinRating = null;
            vm.ExecuteFilter();

            foreach (var m in vm.Movies)
                Assert.IsTrue(m.StartYear >= 2010,
                    $"'{m.PrimaryTitle}' ({m.StartYear}) is before YearFrom.");
        }

        [TestMethod]
        public void ExecuteFilter_NullYears_DoesNotFilterByYear()
        {
            var vm = VmFactory.Create();
            vm.SelectedGenre = null;
            vm.YearFrom = null;
            vm.YearTo = null;
            vm.MinRating = null;
            vm.ExecuteFilter();

            // Oldest test movie is 1972 — should still appear
            Assert.IsTrue(vm.Movies.Any(m => m.StartYear == 1972),
                "Null year filters should not exclude any movies by year.");
        }

        // ── Filter — rating ───────────────────────────────────────────────────

        [TestMethod]
        public void ExecuteFilter_MinRating_ExcludesLowRatedMovies()
        {
            var vm = VmFactory.Create();
            vm.SelectedGenre = null;
            vm.YearFrom = null;
            vm.YearTo = null;
            vm.MinRating = 8.5;
            vm.ExecuteFilter();

            foreach (var m in vm.Movies)
                Assert.IsTrue((double?)m.Rating!.AverageRating >= 8.5,
                    $"'{m.PrimaryTitle}' rating {m.Rating.AverageRating} is below 8.5.");
        }

        [TestMethod]
        public void ExecuteFilter_ZeroRatingTreatedAsNoFilter()
        {
            // MinRating == 0 → VM sends null to service → no rating filter
            var vm = VmFactory.Create();
            vm.SelectedGenre = null;
            vm.YearFrom = null;
            vm.YearTo = null;
            vm.MinRating = 0;
            vm.ExecuteFilter();

            // The low-rated Horror film (4.1) should still appear
            Assert.IsTrue(vm.Movies.Any(m => m.PrimaryTitle == "A Low-Rated Film"),
                "MinRating of 0 should not exclude any movies.");
        }

        // ── Filter — combined ─────────────────────────────────────────────────

        [TestMethod]
        public void ExecuteFilter_GenreAndRating_AppliesBothConditions()
        {
            var vm = VmFactory.Create();
            vm.SelectedGenre = "Drama";
            vm.YearFrom = null;
            vm.YearTo = null;
            vm.MinRating = 8.5;
            vm.ExecuteFilter();

            foreach (var m in vm.Movies)
            {
                Assert.IsTrue(m.Genres.Any(g => g.Name == "Drama"),
                    $"'{m.PrimaryTitle}' is not a Drama.");
                Assert.IsTrue((double?)m.Rating!.AverageRating >= 8.5,
                    $"'{m.PrimaryTitle}' rating {m.Rating.AverageRating} is below 8.5.");
            }
        }

        [TestMethod]
        public void ExecuteFilter_ImpossibleCriteria_ReturnsEmptyCollection()
        {
            var vm = VmFactory.Create();
            vm.SelectedGenre = "Horror";
            vm.MinRating = 9.5; // no Horror film in test data reaches 9.5
            vm.ExecuteFilter();

            Assert.AreEqual(0, vm.Movies.Count);
        }

        // ── Clear ─────────────────────────────────────────────────────────────

        [TestMethod]
        public void ExecuteClear_ResetsAllPropertiesAndReloadsPopularMovies()
        {
            var vm = VmFactory.Create();

            // Apply a restrictive filter first
            vm.SelectedGenre = "Horror";
            vm.MinRating = 9.5;
            vm.ExecuteFilter();
            Assert.AreEqual(0, vm.Movies.Count, "Pre-condition: zero results after impossible filter.");

            vm.ExecuteClear();

            Assert.AreEqual(string.Empty, vm.SearchText);
            Assert.IsNull(vm.SelectedGenre);
            Assert.IsNull(vm.YearFrom);
            Assert.IsNull(vm.YearTo);
            Assert.IsNull(vm.MinRating);
            Assert.IsTrue(vm.Movies.Count > 0,
                "Clear should reload popular movies.");
        }

        // ── Status message ────────────────────────────────────────────────────

        [TestMethod]
        public void StatusMessage_NoResults_SaysNoResultsFound()
        {
            var vm = VmFactory.Create();
            vm.SearchText = "xyzzy_no_match";
            vm.ExecuteSearch();

            StringAssert.Contains(vm.StatusMessage, "No results");
        }

        [TestMethod]
        public void StatusMessage_OneResult_UsesSingularForm()
        {
            var vm = VmFactory.Create();
            vm.SearchText = "Parasite"; // exactly one match
            vm.ExecuteSearch();

            Assert.AreEqual("1 movie found.", vm.StatusMessage);
        }

        [TestMethod]
        public void StatusMessage_MultipleResults_IncludesCount()
        {
            var vm = VmFactory.Create();
            vm.SearchText = "Dark"; // two matches
            vm.ExecuteSearch();

            Assert.AreEqual("2 movies found.", vm.StatusMessage);
        }

        // ── INotifyPropertyChanged ────────────────────────────────────────────

        [TestMethod]
        public void SearchText_Set_FiresPropertyChanged()
        {
            var vm = VmFactory.Create();
            bool fired = false;
            vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.SearchText)) fired = true; };
            vm.SearchText = "Test";
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void MinRating_Set_FiresPropertyChanged()
        {
            var vm = VmFactory.Create();
            bool fired = false;
            vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.MinRating)) fired = true; };
            vm.MinRating = 7.5;
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void YearFrom_Set_FiresPropertyChanged()
        {
            var vm = VmFactory.Create();
            bool fired = false;
            vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.YearFrom)) fired = true; };
            vm.YearFrom = 2010;
            Assert.IsTrue(fired);
        }
    }
}