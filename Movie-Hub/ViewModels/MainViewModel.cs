using Movie_Hub.Commands;
using Movie_Hub.Models;
using Movie_Hub.Services;
using Movie_Hub.Views;

namespace Movie_Hub.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IMovieService _movieService;
        private readonly IGenreService _genreService;

        private object _currentPage = null!;
        private string _currentRoute = string.Empty;

        public FavouritesViewModel FavouritesVm { get; } = new();

        public object CurrentPage
        {
            get => _currentPage;
            private set => SetProperty(ref _currentPage, value);
        }

        public string CurrentRoute
        {
            get => _currentRoute;
            private set => SetProperty(ref _currentRoute, value);
        }

        public RelayCommand NavigateHomeCommand { get; }
        public RelayCommand NavigateFavouritesCommand { get; }
        public RelayCommand NavigateDetailsCommand { get; }
        public RelayCommand AddFavouriteCommand { get; }

        public MainViewModel(IMovieService movieService, IGenreService genreService)
        {
            _movieService = movieService;
            _genreService = genreService;

            NavigateHomeCommand = new RelayCommand(_ => NavigateTo("home"));
            NavigateFavouritesCommand = new RelayCommand(_ => NavigateTo("favourites"));
            NavigateDetailsCommand = new RelayCommand(param => NavigateTo("details", param));

            AddFavouriteCommand = new RelayCommand(
                execute: param => FavouritesVm.Add(param as Title),
                canExecute: param => param is Title t && !FavouritesVm.IsFavourite(t));

            NavigateTo("home");
        }

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
                    DataContext = FavouritesVm
                },
                "details" when parameter is Title title => new MovieDetailsView
                {
                    DataContext = new MovieDetailsViewModel(_movieService)
                    {
                        TitleId = title.TitleId
                    }
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