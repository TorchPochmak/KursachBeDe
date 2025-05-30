using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FarmMetricsClient.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(string baseUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public async Task<AuthResponse?> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsync("api/auth/login",
                    new StringContent(
                        JsonConvert.SerializeObject(new UserLogin { Email = email, Password = password }),
                        Encoding.UTF8,
                        "application/json"));

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<AuthResponse>(responseContent);
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<AuthErrorResponse>(responseContent);

                    return new AuthResponse
                    {
                        IsBanned = errorResponse?.ErrorType == "UserBanned",
                        BanMessage = errorResponse?.Message ?? "Ошибка авторизации",
                        ErrorType = errorResponse?.ErrorType ?? "UnknownError"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при авторизации: {ex.Message}");
                return new AuthResponse
                {
                    BanMessage = $"Ошибка подключения: {ex.Message}",
                    ErrorType = "ConnectionError"
                };
            }
        }

        public async Task<HttpResponseMessage> RegisterAsync(UserRegister registrationData)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(registrationData),
                Encoding.UTF8,
                "application/json"
            );

            return await _httpClient.PostAsync("api/auth/register", content);
        }

        public async Task<UserProfile?> GetUserProfileAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/auth/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var profile = JsonConvert.DeserializeObject<UserProfile>(content);

                if (profile != null && profile.Name?.StartsWith("[BANNED]") == true)
                {
                    profile.IsBanned = true;
                }

                return profile;
            }
            return null;
        }
        public async Task<HttpResponseMessage> UpdateUserProfileAsync(int userId, UserUpdateRequest request)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );
            return await _httpClient.PutAsync($"api/auth/user/{userId}", content);
        }

        public async Task<HttpResponseMessage> DeleteUserAsync(int userId)
        {
            return await _httpClient.DeleteAsync($"api/auth/user/{userId}");
        }

        public async Task<List<UserProfile>> GetAllUsersAsync(string filter = "")
        {
            var response = await _httpClient.GetAsync($"api/admin/users?filter={Uri.EscapeDataString(filter)}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<UserProfile>>(
                    await response.Content.ReadAsStringAsync());
            }
            return new List<UserProfile>();
        }
        public async Task<HttpResponseMessage> BanUserAsync(int userId)
        {
            return await _httpClient.PostAsync($"api/admin/users/{userId}/ban", null);
        }

        public async Task<HttpResponseMessage> UnbanUserAsync(int userId)
        {
            return await _httpClient.PostAsync($"api/admin/users/{userId}/unban", null);
        }

        public async Task<List<Settlement>> GetSettlementsAsync()
        {
            var response = await _httpClient.GetAsync("api/settlements");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<Settlement>>(await response.Content.ReadAsStringAsync());
            }
            return new List<Settlement>();
        }
        public async Task<HttpResponseMessage> UpdateUserSettlementAsync(int userId, int settlementId)
        {
            var request = new UpdateSettlementRequest { SettlementId = settlementId };
            var content = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );
            return await _httpClient.PutAsync($"api/settlements/user/{userId}/settlement", content);
        }

        public async Task<HttpResponseMessage> RemoveUserSettlementAsync(int userId)
        {
            return await _httpClient.DeleteAsync($"api/settlements/user/{userId}/settlement");
        }

        public async Task<Settlement?> AddSettlementAsync(string name)
        {
            var response = await _httpClient.PostAsync("api/settlements",
                new StringContent(
                    JsonConvert.SerializeObject(new { Name = name }),
                    Encoding.UTF8,
                    "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Settlement>(
                    await response.Content.ReadAsStringAsync());
            }
            return null;
        }

        public async Task<bool> DeleteSettlementAsync(int settlementId)
        {
            var response = await _httpClient.DeleteAsync($"api/settlements/{settlementId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CanDeleteSettlementAsync(int settlementId)
        {
            var response = await _httpClient.GetAsync($"api/settlements/{settlementId}/can-delete");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<bool>(await response.Content.ReadAsStringAsync());
            }
            return false;
        }

        public async Task<List<Device>> GetDevicesBySettlementAsync(int settlementId)
        {
            var response = await _httpClient.GetAsync($"api/devices/getall?settlementId={settlementId}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<Device>>(
                    await response.Content.ReadAsStringAsync());
            }
            return new List<Device>();
        }

        public async Task<bool> DeleteDeviceAsync(int deviceId)
        {
            var response = await _httpClient.DeleteAsync($"api/devices/delete?deviceId={deviceId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Metric>> GetAllMetricsAsync()
        {
            var response = await _httpClient.GetAsync("api/metrics/getall");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<Metric>>(
                    await response.Content.ReadAsStringAsync());
            }
            return new List<Metric>();
        }

        public async Task<bool> AddDeviceAsync(SettleMetricDevice device)
        {
            var response = await _httpClient.PostAsync("api/devices/create",
                new StringContent(
                    JsonConvert.SerializeObject(device),
                    Encoding.UTF8,
                    "application/json"));

            return response.IsSuccessStatusCode;
        }

        public async Task<List<MetricData>> GetMetricsBySettlementAsync(int settlementId)
        {
            var response = await _httpClient.GetAsync($"api/metricdata/{settlementId}/getall");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<MetricData>>(
                    await response.Content.ReadAsStringAsync());
            }
            return new List<MetricData>();
        }

        public class MetricData
        {
            public int Id { get; set; }
            public DateTime RegisteredAt { get; set; }
            public double MetricValue { get; set; }
            public int SettleMetricDeviceId { get; set; }
            public MetricDevice Device { get; set; }
        }

        public class MetricDevice
        {
            public int Id { get; set; }
            public Metric Metric { get; set; }
            public Settlement Settlement { get; set; }
            public DateTime RegisteredAt { get; set; }
        }
        // модели
        public class UserLogin
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
        public class AuthErrorResponse
        {
            public string ErrorType { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }
        public class AuthResponse
        {
            public string Token { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public int UserId { get; set; }
            public bool IsBanned { get; set; }
            public string BanMessage { get; set; } = string.Empty;
            public string ErrorType { get; set; } = string.Empty;
        }
        public class UserRegister
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
        public class UserProfile
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string Settlement { get; set; } = string.Empty;
            public int? SettlementId { get; set; }
            public bool IsBanned { get; set; }
        }
        public class UserUpdateRequest
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Password { get; set; }
        }
        public class Settlement
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public bool CanBeDeleted { get; set; }
        }

        public class UpdateSettlementRequest
        {
            public int SettlementId { get; set; }
        }

        public class Device
        {
            public int Id { get; set; }
            public int MetricId { get; set; }
            public string MetricName { get; set; } = string.Empty;
            public double MinValue { get; set; }
            public double MaxValue { get; set; }
            public DateTime RegisteredAt { get; set; }
        }
        public class SettleMetricDevice
        {
            public int Id { get; set; }
            public int MetricId { get; set; }
            public string MetricName { get; set; } = string.Empty;
            public double MinValue { get; set; }
            public double MaxValue { get; set; }
            public int SettlementId { get; set; }
            public DateTime RegisteredAt { get; set; }

        }
        public class Metric
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public double MinValue { get; set; }
            public double MaxValue { get; set; }
        }
        public class AvailableDevice
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public double MinValue { get; set; }
            public double MaxValue { get; set; }
        }

        public async Task<Farm?> GetFarmAsync(string farmId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/farm/get?id={Uri.EscapeDataString(farmId)}");
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<Farm>(
                        await response.Content.ReadAsStringAsync());
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        public class Farm
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public int UserId { get; set; }
            public int SettlementId { get; set; }
            public string SettlementName { get; set; } = string.Empty;
            public List<Culture> Cultures { get; set; } = new();
            public List<FarmMetric> Metrics { get; set; } = new();
            public List<Harvest> Harvests { get; set; } = new();
            public List<FarmComment> Comments { get; set; } = new();
        }

        public class Culture
        {
            public string Name { get; set; } = string.Empty;
            public double SquareMeters { get; set; }
        }

        public class FarmMetric
        {
            public string Name { get; set; } = string.Empty;
            public double Value { get; set; }
            public int DeviceId { get; set; }
        }

        public class Harvest
        {
            public string Id { get; set; } = string.Empty;
            public DateTime RegisteredAt { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Info { get; set; } = string.Empty;
        }

        public class FarmComment
        {
            public string Id { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public string Info { get; set; } = string.Empty;
        }

        public async Task<List<Farm>> GetUserFarmsAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/farm/getall?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<Farm>>(
                    await response.Content.ReadAsStringAsync());
            }
            return new List<Farm>();
        }

        public async Task<List<AvailableDevice>> GetAvailableDevicesAsync(int settlementId)
        {
            var response = await _httpClient.GetAsync($"api/farm/available-metrics?settlementId={settlementId}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<AvailableDevice>>(await response.Content.ReadAsStringAsync());
            }
            return new List<AvailableDevice>();
        }

        public async Task<bool> CreateFarmAsync(string name, int settlementId, int userId)
        {
            var url = $"api/farm/create?name={Uri.EscapeDataString(name)}&settlementId={settlementId}&userId={userId}";
            var response = await _httpClient.PostAsync(url, null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddCultureAsync(string farmId, string name, double area)
        {
            var response = await _httpClient.PostAsync(
                $"api/farm/cultures/add?farmId={farmId}&name={Uri.EscapeDataString(name)}&squareMeters={area}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCultureAsync(string farmId, string name)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/farm/cultures/delete?farmId={farmId}&cultureName={Uri.EscapeDataString(name)}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddMetricAsync(string farmId, int deviceId, double value)
        {
            var response = await _httpClient.PostAsync(
                $"api/farm/metrics/add?farmId={farmId}&deviceId={deviceId}&value={value}",
                null
            );
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMetricAsync(string farmId, int deviceId)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/farm/metrics/delete?farmId={farmId}&deviceId={deviceId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddHarvestAsync(string farmId, string name, string info)
        {
            var response = await _httpClient.PostAsync(
                $"api/farm/harvests/add?farmId={farmId}&name={Uri.EscapeDataString(name)}&info={Uri.EscapeDataString(info)}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddCommentAsync(string farmId, string info)
        {
            var response = await _httpClient.PostAsync(
                $"api/farm/comments/add?farmId={farmId}&info={Uri.EscapeDataString(info)}", null);
            return response.IsSuccessStatusCode;
        }
    }
}
