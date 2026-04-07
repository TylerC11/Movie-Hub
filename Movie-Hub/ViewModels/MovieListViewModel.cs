using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Movie_Hub.Commands;
using Movie_Hub.Models;
using Movie_Hub.Services;

namespace Movie_Hub.ViewModels
{
    public class MovieListViewModel : BaseViewModel
    {
        // ── Services ───────────────────────────────────────────────────────
        private readonly IMovieService _movieService;
        private readonly IGenreService _genreService;
        private readonly RelayCommand _navigateDetailsCommand;

        // ── Backing fields ─────────────────────────────────────────────────
        private string _searchText = string.Empty;
        private string? _selectedGenre = null;
        private int? _yearFrom = null;
        private int? _yearTo = null;
        private double? _minRating = null;
        private bool _isLoading = false;
        private string _statusMessage = string.Empty;

        // ── Public collections ─────────────────────────────────────────────

        public ObservableCollection<Title> Movies { get; } = new();
        public ObservableCollection<string> Genres { get; } = new();

        // ── Filter properties ──────────────────────────────────────────────

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    SearchCommand.RaiseCanExecuteChanged();
            }
        }

        public string? SelectedGenre
        {
            get => _selectedGenre;
            set => SetProperty(ref _selectedGenre, value);
        }

        public int? YearFrom
        {
            get => _yearFrom;
            set => SetProperty(ref _yearFrom, value);
        }

        public int? YearTo
        {
            get => _yearTo;
            set => SetProperty(ref _yearTo, value);
        }

        public double? MinRating
        {
            get => _minRating;
            set => SetProperty(ref _minRating, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        // ── Commands ───────────────────────────────────────────────────────

        public RelayCommand SearchCommand { get; }
        public RelayCommand FilterCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand ViewDetailsCommand => _navigateDetailsCommand;

        // ── Constructor ────────────────────────────────────────────────────

        public MovieListViewModel(
            IMovieService movieService,
            IGenreService genreService,
            RelayCommand navigateDetailsCommand)
        {
            _movieService = movieService;
            _genreService = genreService;
            _navigateDetailsCommand = navigateDetailsCommand;

            SearchCommand = new RelayCommand(
                execute: _ => ExecuteSearch(),
                canExecute: _ => !string.IsNullOrWhiteSpace(SearchText));

            FilterCommand = new RelayCommand(_ => ExecuteFilter());
            ClearCommand = new RelayCommand(_ => ExecuteClear());

            LoadGenres();
            LoadPopularMovies();
        }

        // ── Service calls ──────────────────────────────────────────────────

        public void LoadPopularMovies()
        {
            IsLoading = true;
            try
            {
                var results = _movieService.GetPopularMovies(count: 50);
                RefreshMovies(results);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading movies: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return;

            IsLoading = true;
            try
            {
                var results = _movieService.SearchByTitle(SearchText.Trim());
                RefreshMovies(results);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Search error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void ExecuteFilter()
        {
            IsLoading = true;
            try
            {
                // Treat MinRating == 0 as "no filter" so the slider resting
                // at zero doesn't exclude movies that have no rating set.
                double? ratingFilter = (MinRating is > 0) ? MinRating : null;

                var results = _movieService.FilterMovies(
                    genre: SelectedGenre,
                    yearFrom: YearFrom,
                    yearTo: YearTo,
                    minRating: ratingFilter);

                RefreshMovies(results);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Filter error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void ExecuteClear()
        {
            SearchText = string.Empty;
            SelectedGenre = null;
            YearFrom = null;
            YearTo = null;
            MinRating = null;
            LoadPopularMovies();
        }

        // ── Private helpers ────────────────────────────────────────────────

        private void LoadGenres()
        {
            try
            {
                var genres = _genreService.GetAllGenres();
                Genres.Clear();
                Genres.Add("All Genres");
                foreach (var g in genres)
                    Genres.Add(g);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Could not load genres: {ex.Message}";
            }
        }

        private void RefreshMovies(List<Title> results)
        {
            Movies.Clear();
            foreach (var m in results)
                Movies.Add(m);

            StatusMessage = results.Count switch
            {
                0 => "No results found — try adjusting your search or filters.",
                1 => "1 movie found.",
                _ => $"{results.Count} movies found."
            };
        }
    }
}