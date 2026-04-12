using System.Collections.ObjectModel;
using Movie_Hub.Commands;
using Movie_Hub.Models;
using Movie_Hub.Services;

namespace Movie_Hub.ViewModels
{
    public class RecommendationViewModel : BaseViewModel
    {
        private readonly IRecommendationService _recommendationService;
        private readonly FavouritesViewModel _favouritesVm;
        private bool _isLoading;
        private string _statusMessage = string.Empty;

        public ObservableCollection<Title> Recommendations { get; } = new();

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

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddFavouriteCommand { get; }

        public RecommendationViewModel(
            IRecommendationService recommendationService,
            FavouritesViewModel favouritesVm,
            RelayCommand addFavouriteCommand)
        {
            _recommendationService = recommendationService;
            _favouritesVm = favouritesVm;
            AddFavouriteCommand = addFavouriteCommand;

            RefreshCommand = new RelayCommand(_ => LoadRecommendations());

            // Reload whenever favourites change
            _favouritesVm.Favourites.CollectionChanged += (_, _) => LoadRecommendations();

            LoadRecommendations();
        }

        public void LoadRecommendations()
        {
            IsLoading = true;
            try
            {
                var results = _recommendationService.GetRecommendations(_favouritesVm.Favourites);
                Recommendations.Clear();
                foreach (var r in results)
                    Recommendations.Add(r);

                StatusMessage = _favouritesVm.Favourites.Count == 0
                    ? "Add movies to your favourites to get recommendations."
                    : Recommendations.Count == 0
                        ? "No recommendations found — try adding more favourites."
                        : $"{Recommendations.Count} movies recommended based on your favourites.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading recommendations: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 