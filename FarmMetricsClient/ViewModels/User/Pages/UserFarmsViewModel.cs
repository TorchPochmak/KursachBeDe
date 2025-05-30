using System;
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
    // RelayCommand для простых команд
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
    // AsyncRelayCommand для асинхронных операций
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, Task> _execute;
        private readonly Predicate<object?>? _canExecute;
        public AsyncRelayCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public async void Execute(object? parameter) => await _execute(parameter);
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }

    public class FarmViewModel : INotifyPropertyChanged
    {
        public Farm Farm { get; }
        private readonly ApiClient _apiClient;

        // Модель для отображения устройств в ComboBox
        public class DeviceDisplayModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public double MinValue { get; set; }
            public double MaxValue { get; set; }

            public override string ToString() => Name;
        }

        // Культуры
        private bool _isAddCultureVisible;
        public bool IsAddCultureVisible
        {
            get => _isAddCultureVisible;
            set { _isAddCultureVisible = value; OnPropertyChanged(); }
        }
        public string NewCultureName { get; set; } = "";
        public string NewCultureArea { get; set; } = "";

        // Метрики
        private bool _isAddMetricVisible;
        public bool IsAddMetricVisible
        {
            get => _isAddMetricVisible;
            set { _isAddMetricVisible = value; OnPropertyChanged(); }
        }
        public ObservableCollection<DeviceDisplayModel> AvailableDevices { get; } = new();
        private DeviceDisplayModel _selectedDevice;
        public DeviceDisplayModel SelectedDevice
        {
            get => _selectedDevice;
            set { _selectedDevice = value; OnPropertyChanged(); }
        }
        public string NewMetricValue { get; set; } = "";

        // Сборы урожая
        private bool _isAddHarvestVisible;
        public bool IsAddHarvestVisible
        {
            get => _isAddHarvestVisible;
            set { _isAddHarvestVisible = value; OnPropertyChanged(); }
        }
        public string NewHarvestName { get; set; } = "";
        public string NewHarvestInfo { get; set; } = "";

        // Комментарии
        public string NewComment { get; set; } = "";

        // Команды
        public ICommand ShowAddCultureCommand { get; }
        public ICommand AddCultureCommand { get; }
        public ICommand CancelAddCultureCommand { get; }
        public ICommand DeleteCultureCommand { get; }

        public ICommand ShowAddMetricCommand { get; }
        public ICommand AddMetricCommand { get; }
        public ICommand CancelAddMetricCommand { get; }
        public ICommand DeleteMetricCommand { get; }

        public ICommand ShowAddHarvestCommand { get; }
        public ICommand AddHarvestCommand { get; }
        public ICommand CancelAddHarvestCommand { get; }

        public ICommand AddCommentCommand { get; }

        public event Action? FarmChanged;

        public FarmViewModel(Farm farm, ApiClient apiClient, Action? farmChangedCallback = null)
        {
            Farm = farm;
            _apiClient = apiClient;
            FarmChanged = farmChangedCallback;


            // Инициализация команд
            ShowAddCultureCommand = new RelayCommand(_ => IsAddCultureVisible = true);
            CancelAddCultureCommand = new RelayCommand(_ =>
            {
                IsAddCultureVisible = false;
                NewCultureName = "";
                NewCultureArea = "";
            });
            AddCultureCommand = new AsyncRelayCommand(async _ => await AddCulture());
            DeleteCultureCommand = new AsyncRelayCommand(async id => await DeleteCulture(id as string));

            ShowAddMetricCommand = new AsyncRelayCommand(async _ => await ShowAddMetric());
            CancelAddMetricCommand = new RelayCommand(_ =>
            {
                IsAddMetricVisible = false;
                SelectedDevice = null;
                NewMetricValue = "";
            });
            AddMetricCommand = new AsyncRelayCommand(async _ => await AddMetric());
            DeleteMetricCommand = new AsyncRelayCommand(async id => await DeleteMetric((int)id));

            ShowAddHarvestCommand = new RelayCommand(_ => IsAddHarvestVisible = true);
            CancelAddHarvestCommand = new RelayCommand(_ =>
            {
                IsAddHarvestVisible = false;
                NewHarvestName = "";
                NewHarvestInfo = "";
            });
            AddHarvestCommand = new AsyncRelayCommand(async _ => await AddHarvest());

            AddCommentCommand = new AsyncRelayCommand(async _ => await AddComment());
        }

        private async Task AddCulture()
        {
            if (string.IsNullOrWhiteSpace(NewCultureName) || !double.TryParse(NewCultureArea, out var area))
                return;

            var result = await _apiClient.AddCultureAsync(Farm.Id, NewCultureName, area);
            if (result)
            {
                FarmChanged?.Invoke(); // Вызовем обновление всех ферм
            }
        }

        private async Task DeleteCulture(string cultureId)
        {
            if (string.IsNullOrEmpty(cultureId)) return;

            var result = await _apiClient.DeleteCultureAsync(Farm.Id, cultureId);
            if (result)
            {
                FarmChanged?.Invoke();
            }
        }

        private async Task ShowAddMetric()
        {
            AvailableDevices.Clear();
            var devices = await _apiClient.GetAvailableDevicesAsync(Farm.SettlementId);

            foreach (var d in devices)
            {
                if (!Farm.Metrics.Exists(m => m.DeviceId == d.Id))
                {
                    AvailableDevices.Add(new DeviceDisplayModel
                    {
                        Id = d.Id,
                        Name = d.Name,
                        MinValue = d.MinValue,
                        MaxValue = d.MaxValue
                    });
                }
            }
            IsAddMetricVisible = true;
        }

        private async Task AddMetric()
        {
            if (SelectedDevice == null || !double.TryParse(NewMetricValue, out var value))
                return;

            var result = await _apiClient.AddMetricAsync(Farm.Id, SelectedDevice.Id, value);
            if (result)
            {
                FarmChanged?.Invoke();
            }
        }

        private async Task DeleteMetric(int deviceId)
        {
            var result = await _apiClient.DeleteMetricAsync(Farm.Id, deviceId);
            if (result)
            {
                FarmChanged?.Invoke();
            }
        }

        private async Task AddHarvest()
        {
            if (string.IsNullOrWhiteSpace(NewHarvestName)) return;

            var result = await _apiClient.AddHarvestAsync(Farm.Id, NewHarvestName, NewHarvestInfo);
            if (result)
            {
                FarmChanged?.Invoke();
            }
        }

        private async Task AddComment()
        {
            if (string.IsNullOrWhiteSpace(NewComment)) return;

            var result = await _apiClient.AddCommentAsync(Farm.Id, NewComment);
            if (result)
            {
                FarmChanged?.Invoke();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Основная VM для списка ферм
    public class UserFarmsViewModel : INotifyPropertyChanged
    {
        private readonly int _userId;
        private readonly ApiClient _apiClient;
        public ObservableCollection<FarmViewModel> Farms { get; set; } = new();

        // Для добавления фермы
        public ObservableCollection<ApiClient.Settlement> Settlements { get; set; } = new();
        private ApiClient.Settlement _selectedSettlement;
        public ApiClient.Settlement SelectedSettlement
        {
            get => _selectedSettlement;
            set { _selectedSettlement = value; OnPropertyChanged(); }
        }
        public string NewFarmName { get; set; } = "";

        private bool _isAddFarmVisible;
        public bool IsAddFarmVisible
        {
            get => _isAddFarmVisible;
            set { _isAddFarmVisible = value; OnPropertyChanged(); }
        }

        public ICommand ShowAddFarmCommand { get; }
        public ICommand CancelAddFarmCommand { get; }
        public ICommand AddFarmCommand { get; }
        public ICommand ReloadFarmsCommand { get; }

        public UserFarmsViewModel(int userId)
        {
            _userId = userId;
            _apiClient = new ApiClient("http://localhost:5148/");
            ReloadFarmsCommand = new AsyncRelayCommand(async _ => await ReloadFarms());
            ShowAddFarmCommand = new RelayCommand(_ => IsAddFarmVisible = true);
            CancelAddFarmCommand = new RelayCommand(_ =>
            {
                IsAddFarmVisible = false;
                NewFarmName = "";
                SelectedSettlement = null;
                OnPropertyChanged(nameof(NewFarmName));
                OnPropertyChanged(nameof(SelectedSettlement));
            });
            AddFarmCommand = new AsyncRelayCommand(async _ => await AddFarm());
            _ = ReloadFarms();
        }
        private async Task ReloadFarms()
        {
            Farms.Clear();
            var farms = await _apiClient.GetUserFarmsAsync(_userId);
            var settlements = await _apiClient.GetSettlementsAsync();

            Settlements.Clear();
            foreach (var s in settlements)
                Settlements.Add(s);

            foreach (var farm in farms)
            {
                var settlement = settlements.FirstOrDefault(s => s.Id == farm.SettlementId);
                if (settlement != null)
                {
                    farm.SettlementName = settlement.Name;
                }
                Farms.Add(new FarmViewModel(farm, _apiClient, () => _ = ReloadFarms()));
            }
        }
        private async Task AddFarm()
        {
            if (SelectedSettlement == null || string.IsNullOrWhiteSpace(NewFarmName))
                return;

            var response = await _apiClient.CreateFarmAsync(NewFarmName, SelectedSettlement.Id, _userId);
            if (response)
            {
                IsAddFarmVisible = false;
                NewFarmName = "";
                SelectedSettlement = null;
                OnPropertyChanged(nameof(NewFarmName));
                OnPropertyChanged(nameof(SelectedSettlement));
                await ReloadFarms();
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}