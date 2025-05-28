using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FarmMetricsClient.Services;
using static FarmMetricsClient.Services.ApiClient;

namespace FarmMetricsClient.ViewModels.User.Pages
{
    public class UserMetricsViewModel : INotifyPropertyChanged
    {
        private readonly int _userId;
        private readonly ApiClient _apiClient;
        private string _settlementName;
        private bool _isLoading;
        private int? _settlementId;

        public ObservableCollection<MetricData> Metrics { get; } = new ObservableCollection<MetricData>();

        public string SettlementName
        {
            get => _settlementName;
            set
            {
                _settlementName = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool HasSettlement => _settlementId.HasValue;

        public UserMetricsViewModel(int userId, Action goBack)
        {
            _userId = userId;
            _apiClient = new ApiClient("http://localhost:5148/");
            GoBackCommand = new RelayCommand(_ => goBack());
            
            _ = LoadInitialData();
        }

        public ICommand GoBackCommand { get; }

        private async Task LoadInitialData()
        {
            try
            {
                IsLoading = true;
                
                var profile = await _apiClient.GetUserProfileAsync(_userId);
                if (profile == null)
                {
                    SettlementName = "Ошибка загрузки профиля";
                    return;
                }

                _settlementId = profile.SettlementId;
                SettlementName = profile.Settlement ?? "Неизвестный населенный пункт";
                
                OnPropertyChanged(nameof(HasSettlement));

                if (_settlementId.HasValue)
                {
                    await LoadMetrics();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
                SettlementName = "Ошибка загрузки данных";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadMetrics()
        {
            if (!_settlementId.HasValue) return;

            try
            {
                IsLoading = true;
                Metrics.Clear();
                
                var metrics = await _apiClient.GetMetricsBySettlementAsync(_settlementId.Value);
                foreach (var metric in metrics.OrderByDescending(m => m.RegisteredAt))
                {
                    Metrics.Add(metric);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке метрик: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}