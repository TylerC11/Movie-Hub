using System.Windows.Controls;
using Movie_Hub.Commands;
using Movie_Hub.Models;
using Movie_Hub.Services;
using Movie_Hub.Views;

namespace Movie_Hub.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // ── Services ───────────────────────────────────────────────────────
        private readonly IMovieService _movieService;
        private readonly IGenreService _genreService;

        // ── Backing fields ─────────────────────────────────────────────────
        private Page _currentPage = null!;
        private string _currentRoute = string.Empty;

        // ── Properties ─────────────────────────────────────────────────────

        public Page CurrentPage
        {
            get => _currentPage;
            private set => SetProperty(ref _currentPage, value);
        }

        public string CurrentRoute
        {
            get => _currentRoute;
            private set => SetProperty(ref _currentRoute, value);
        }

        // ── Commands ───────────────────────────────────────────────────────

        public RelayCommand NavigateHomeCommand { get; }
        public RelayCommand NavigateFavouritesCommand { get; }
        public RelayCommand NavigateDetailsCommand { get; }

        // ── Constructor ────────────────────────────────────────────────────

        public MainViewModel(IMovieService movieService, IGenreService genreService)
        {
            _movieService = movieService;
            _genreService = genreService;

            NavigateHomeCommand = new RelayCommand(_ => NavigateTo("home"));
            NavigateFavouritesCommand = new RelayCommand(_ => NavigateTo("favourites"));
            NavigateDetailsCommand = new RelayCommand(param => NavigateTo("details", param));

            // Startup page
            NavigateTo("home");
        }

        // ── Private helpers ────────────────────────────────────────────────

        private void NavigateTo(string route, object? parameter = null)
        {
            CurrentRoute = route;

            CurrentPage = route switch
            {
                "home" => new MovieListView
                {
                    DataContext = new MovieListViewModel(
                        _movieService,
                        _genreService,
                        NavigateDetailsCommand)
                },

                "favourites" => new FavouritesView
                {
                    DataContext = new FavouritesViewModel()
                },

                // Pass the Title directly as DataContext until
                // MovieDetailsViewModel is fully implemented.
                "details" when parameter is Title title => new MovieDetailsView
                {
                    DataContext = title
                },

                _ => new MovieListView
                {
                    DataContext = new MovieListViewModel(
                        _movieService,
                        _genreService,
                        NavigateDetailsCommand)
                }
            };
        }
    }
}