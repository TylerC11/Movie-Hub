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
        private readonly IRecommendationService _recommendationService;

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
        public RelayCommand NavigateRecommendationsCommand { get; }
        public RelayCommand NavigateDetailsCommand { get; }
        public RelayCommand AddFavouriteCommand { get; }

        public MainViewModel(IMovieService movieService, IGenreService genreService,
                             IRecommendationService recommendationService)
        {
            _movieService = movieService;
            _genreService = genreService;
            _recommendationService = recommendationService;

            NavigateHomeCommand = new RelayCommand(_ => NavigateTo("home"));
            NavigateFavouritesCommand = new RelayCommand(_ => NavigateTo("favourites"));
            NavigateRecommendationsCommand = new RelayCommand(_ => NavigateTo("recommendations"));
            NavigateDetailsCommand = new RelayCommand(param => NavigateTo("details", param));

            AddFavouriteCommand = new RelayCommand(
                execute: param => FavouritesVm.Add(param as Title),
                canExecute: param => param is Title t && !FavouritesVm.IsFavourite(t));

            // Keep the Add button refreshed when favourites list changes
            FavouritesVm.Favourites.CollectionChanged += (_, _) =>
                AddFavouriteCommand.RaiseCanExecuteChanged();

            NavigateTo("home");
        }

        // Cached page instances — avoids rebuilding and re-querying on every navigation
        private MovieListView? _homeView;
        private FavouritesView? _favouritesView;
        private RecommendationsView? _recommendationsView;

        private void NavigateTo(string route, object? parameter = null)
        {
            CurrentRoute = route;

            CurrentPage = route switch
            {
                "home" => GetHomeView(),
                "favourites" => GetFavouritesView(),
                "recommendations" => GetRecommendationsView(),
                "details" when parameter is Title title => CreateDetailsView(title),
                _ => GetHomeView()
            };
        }

        private MovieListView GetHomeView()
        {
            if (_homeView != null) return _homeView;
            _homeView = new MovieListView
            {
                DataContext = new MovieListViewModel(
                    _movieService, _genreService,
                    NavigateDetailsCommand, AddFavouriteCommand)
            };
            return _homeView;
        }

        private FavouritesView GetFavouritesView()
        {
            if (_favouritesView != null) return _favouritesView;
            _favouritesView = new FavouritesView { DataContext = FavouritesVm };
            return _favouritesView;
        }

        private RecommendationsView GetRecommendationsView()
        {
            if (_recommendationsView != null) return _recommendationsView;
            _recommendationsView = new RecommendationsView
            {
                DataContext = new RecommendationViewModel(
                    _recommendationService, FavouritesVm, AddFavouriteCommand)
            };
            return _recommendationsView;
        }

        private MovieDetailsView CreateDetailsView(Title title)
        {
            var vm = new MovieDetailsViewModel(_movieService)
            {
                TitleId = title.TitleId,
                NavigateBack = () => NavigateTo("home"),
                AddToFavourites = () => FavouritesVm.Add(title)
            };

            vm.IsAlreadyFavourite = FavouritesVm.IsFavourite(title);

            return new MovieDetailsView { DataContext = vm };
        }
    }
}