using Movie_Hub.Commands;
using Movie_Hub.Models;
using Movie_Hub.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Movie_Hub.ViewModels
{
    public class MovieDetailsViewModel : BaseViewModel
    {
        // Services
        private readonly IMovieService _movieService;
        private readonly OmdbService _omdbService = new(); // OMDb fallback for missing data

        private string _titleId;
        private Title _movie;
        private bool _isLoading;
        private string _errorMessage;
        private bool _isAlreadyFavourite;
        private string? _plot;

        // Navigation
        public Action NavigateBack { get; set; }

        // Favourites
        public Action AddToFavourites { get; set; }

        public bool IsAlreadyFavourite
        {
            get => _isAlreadyFavourite;
            set => SetProperty(ref _isAlreadyFavourite, value);
        }

        // Plot/Description fetched from OMDb API
        public string? Plot
        {
            get => _plot;
            set => SetProperty(ref _plot, value);
        }

        // Public properties for binding
        public ObservableCollection<CastMemberDisplay> Cast { get; } = new();
        public ObservableCollection<Genre> Genres { get; } = new();

        // Commands
        public ICommand LoadMovieCommand { get; }
        public ICommand NavigateBackCommand { get; }
        public ICommand AddToFavouritesCommand { get; }

        public MovieDetailsViewModel(IMovieService movieService)
        {
            _movieService = movieService;
            LoadMovieCommand = new RelayCommand(LoadMovie);
            NavigateBackCommand = new RelayCommand(() => NavigateBack?.Invoke());
            AddToFavouritesCommand = new RelayCommand(() =>
            {
                AddToFavourites?.Invoke();
                IsAlreadyFavourite = true;
            });
        }

        public string TitleId
        {
            get => _titleId;
            set
            {
                _titleId = value;
                LoadMovieCommand.Execute(null); // Auto-load when TitleId is set
            }
        }

        public Title Movie
        {
            get => _movie;
            set => SetProperty(ref _movie, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        // Load movie details, genres, and cast information
        private void LoadMovie()
        {
            if (string.IsNullOrEmpty(TitleId)) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                // Clear previous data to avoid showing stale information while loading
                Cast.Clear();
                Genres.Clear();
                Plot = null;

                Movie = _movieService.GetMovieDetails(TitleId);

                if (Movie == null)
                {
                    ErrorMessage = "Movie not found";
                    return;
                }

                // Load Genres
                if (Movie.Genres != null)
                {
                    foreach (var genre in Movie.Genres)
                        Genres.Add(genre);
                }

                //// Load Picture
                //if (Movie.Pictures != null && Movie.Pictures.Any())
                //    Movie.PictureUrl = Movie.Pictures.First().Url;

                // Load Cast from local database first
                var castMembers = _movieService.GetCast(TitleId);

                if (castMembers != null && castMembers.Any())
                {
                    foreach (var principal in castMembers)
                    {
                        if (principal.Name == null) continue; // Skip entries with no name

                        Cast.Add(new CastMemberDisplay
                        {
                            Name = principal.Name.PrimaryName ?? "Unknown",
                            CharacterName = !string.IsNullOrEmpty(principal.Characters)
                                ? principal.Characters
                                : principal.Job ?? principal.JobCategory ?? "Unknown Role"
                        });
                    }
                }

                // If no cast from database, fall back to OMDb for both cast and plot
                if (Cast.Count == 0)
                    _ = LoadFromOmdbAsync(TitleId, loadCast: true, loadPlot: true);
                else
                    _ = LoadFromOmdbAsync(TitleId, loadCast: false, loadPlot: true);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Fetches plot and/or cast from OMDb API as a fallback
        private async Task LoadFromOmdbAsync(string imdbId, bool loadCast, bool loadPlot)
        {
            try
            {
                if (loadCast)
                {
                    var omdbCast = await _omdbService.GetCastAsync(imdbId);

                    if (omdbCast != null && omdbCast.Any())
                    {
                        Cast.Clear();
                        foreach (var member in omdbCast)
                        {
                            Cast.Add(new CastMemberDisplay
                            {
                                Name = member.Name,
                                CharacterName = member.Role
                            });
                        }
                    }
                    else
                    {
                        Cast.Add(new CastMemberDisplay
                        {
                            Name = "No cast information available",
                            CharacterName = "This movie doesn't have cast data"
                        });
                    }
                }

                if (loadPlot)
                {
                    Plot = await _omdbService.GetPlotAsync(imdbId)
                           ?? "No description available.";
                }
            }
            catch
            {
                if (loadCast)
                    Cast.Add(new CastMemberDisplay
                    {
                        Name = "Could not load cast",
                        CharacterName = "Failed to fetch cast information"
                    });

                if (loadPlot)
                    Plot = "Could not load description.";
            }
        }
    }

    // Represents a single cast member displayed in the UI
    public class CastMemberDisplay
    {
        public string Name { get; set; }
        public string CharacterName { get; set; }
        public string DisplayText => $"{Name} as \"{CharacterName}\"";
    }
}