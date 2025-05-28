using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using FarmMetricsClient.Services;

namespace FarmMetricsClient.ViewModels.Admin.Pages
{
    public class AdminSettlementsViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;
        private readonly Action<int, string> _navigateToDevices;
        private string _filterText = string.Empty;
        private string _newSettlementName = string.Empty;
        private bool _isAddingSettlement;
        private List<SettlementItem> _allSettlements = new();

        public ObservableCollection<SettlementItem> FilteredSettlements { get; } = new();

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public string NewSettlementName
        {
            get => _newSettlementName;
            set
            {
                if (_newSettlementName != value)
                {
                    _newSettlementName = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsAddingSettlement
        {
            get => _isAddingSettlement;
            set
            {
                if (_isAddingSettlement != value)
                {
                    _isAddingSettlement = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoadSettlementsCommand { get; }
        public ICommand AddSettlementCommand { get; }
        public ICommand StartAddSettlementCommand { get; }
        public ICommand CancelAddSettlementCommand { get; }
        public ICommand DeleteSettlementCommand { get; }
        public ICommand ShowDevicesCommand { get; }

        public AdminSettlementsViewModel(Action<int, string> navigateToDevices)
        {
            _apiClient = new ApiClient("http://localhost:5148/");
            _navigateToDevices = navigateToDevices;

            LoadSettlementsCommand = new RelayCommand(async _ => await LoadSettlementsAsync());
            AddSettlementCommand = new RelayCommand(async _ => await AddSettlementAsync());
            StartAddSettlementCommand = new RelayCommand(_ => StartAddSettlement());
            CancelAddSettlementCommand = new RelayCommand(_ => CancelAddSettlement());
            DeleteSettlementCommand = new RelayCommand<int>(async (id) => await DeleteSettlementAsync(id));
            ShowDevicesCommand = new RelayCommand<int>(async (id) => await ShowDevicesAsync(id));

            _ = LoadSettlementsAsync();
        }

        private async Task LoadSettlementsAsync()
        {
            try
            {
                var settlements = await _apiClient.GetSettlementsAsync();
                _allSettlements.Clear();

                foreach (var settlement in settlements)
                {
                    var canDelete = await _apiClient.CanDeleteSettlementAsync(settlement.Id);
                    _allSettlements.Add(new SettlementItem
                    {
                        Id = settlement.Id,
                        Name = settlement.Name,
                        CanDelete = canDelete
                    });
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке населенных пунктов: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allSettlements.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterText))
            {
                filtered = filtered.Where(s =>
                    s.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
            }

            FilteredSettlements.Clear();
            foreach (var settlement in filtered.OrderBy(s => s.Name))
            {
                FilteredSettlements.Add(settlement);
            }
        }

        private void StartAddSettlement()
        {
            NewSettlementName = string.Empty;
            IsAddingSettlement = true;
        }

        private void CancelAddSettlement()
        {
            IsAddingSettlement = false;
        }

        private async Task AddSettlementAsync()
        {
            if (string.IsNullOrWhiteSpace(NewSettlementName))
            {
                Console.WriteLine("Название населенного пункта не может быть пустым");
                return;
            }

            try
            {
                var settlement = await _apiClient.AddSettlementAsync(NewSettlementName);

                if (settlement != null)
                {
                    IsAddingSettlement = false;
                    await LoadSettlementsAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении населенного пункта: {ex.Message}");
            }
        }

        public async Task DeleteSettlementAsync(int settlementId)
        {
            try
            {
                var settlement = _allSettlements.FirstOrDefault(s => s.Id == settlementId);
                if (settlement == null || !settlement.CanDelete) return;

                var success = await _apiClient.DeleteSettlementAsync(settlementId);
                if (success)
                {
                    await LoadSettlementsAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении населенного пункта: {ex.Message}");
            }
        }

        public async Task ShowDevicesAsync(int settlementId)
        {
            try
            {
                var settlement = _allSettlements.FirstOrDefault(s => s.Id == settlementId);
                if (settlement != null)
                {
                    _navigateToDevices(settlementId, settlement.Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при переходе к устройствам: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SettlementItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool CanDelete { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}