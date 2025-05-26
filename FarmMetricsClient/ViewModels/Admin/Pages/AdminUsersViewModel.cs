using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FarmMetricsClient.Services;

namespace FarmMetricsClient.ViewModels.Admin.Pages
{
    public class AdminUsersViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;
        private string _filterText = string.Empty;
        private List<ApiClient.UserProfile> _allUsers = new();
        public ICommand BanUserCommand { get; }
        public ICommand UnbanUserCommand { get; }
        public ICommand ReloadUsersCommand { get; }

        public ObservableCollection<ApiClient.UserProfile> FilteredUsers { get; } = new();

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

        public AdminUsersViewModel()
        {
            _apiClient = new ApiClient("http://localhost:5148/");

            BanUserCommand = new RelayCommand(async (userId) => await ToggleBanUserAsync((int)userId, true));
            UnbanUserCommand = new RelayCommand(async (userId) => await ToggleBanUserAsync((int)userId, false));
            ReloadUsersCommand = new RelayCommand(async _ => await LoadUsersAsync());

            _ = LoadUsersAsync();
        }

        public async Task ToggleBanUserAsync(int userId, bool ban)
        {
            var response = ban
                ? await _apiClient.BanUserAsync(userId)
                : await _apiClient.UnbanUserAsync(userId);

            if (response.IsSuccessStatusCode)
            {
                await LoadUsersAsync();
            }
        }

        public async Task LoadUsersAsync()
        {
            try
            {
                var users = await _apiClient.GetAllUsersAsync();
                _allUsers = users.Where(u => u.Role != "Admin").ToList();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке пользователей: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allUsers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterText))
            {
                filtered = filtered.Where(u =>
                    (u.Name?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (u.Email?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (u.Phone?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) == true) ||
                    (u.Settlement?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) == true));
            }

            FilteredUsers.Clear();
            foreach (var user in filtered.OrderBy(u => u.Name))
            {
                FilteredUsers.Add(user);
            }
        }

        public async Task BanUserAsync(int userId)
        {
            var response = await _apiClient.BanUserAsync(userId);
            if (response.IsSuccessStatusCode)
            {
                await LoadUsersAsync();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}