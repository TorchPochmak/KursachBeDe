using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

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
        public class UserLogin
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
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


        public class Settlement
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public async Task<List<Settlement>> GetSettlementsAsync()
        {
            var response = await _httpClient.GetAsync("api/settlements");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<Settlement>>(
                    await response.Content.ReadAsStringAsync());
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

        public class UpdateSettlementRequest
        {
            public int SettlementId { get; set; }
        }
        public async Task<HttpResponseMessage> RemoveUserSettlementAsync(int userId)
        {
            return await _httpClient.DeleteAsync($"api/settlements/user/{userId}/settlement");
        }



        // todo после ипорта - проверить ------------------------------
        public class Farm
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Settlement { get; set; } = string.Empty;
            public List<Culture> Cultures { get; set; } = new();
            public List<Metric> Metrics { get; set; } = new();
            public List<Harvest> Harvests { get; set; } = new();
        }

        public class Culture
        {
            public string Name { get; set; } = string.Empty;
            public double Area { get; set; }
        }

        public class Metric
        {
            public string Name { get; set; } = string.Empty;
            public double Value { get; set; }
        }

        public class Harvest
        {
            public DateTime Date { get; set; }
            public string CultureName { get; set; } = string.Empty;
            public double Amount { get; set; }
        }
        public async Task<List<Farm>?> GetUserFarmsAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/farms/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<Farm>>(
                    await response.Content.ReadAsStringAsync());
            }
            return null;
        }

        public async Task<Farm?> GetFarmDetailsAsync(int farmId)
        {
            var response = await _httpClient.GetAsync($"api/farms/{farmId}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Farm>(
                    await response.Content.ReadAsStringAsync());
            }
            return null;
        }

        public async Task<HttpResponseMessage> AddFarmAsync(Farm farm)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(farm),
                Encoding.UTF8,
                "application/json"
            );
            return await _httpClient.PostAsync("api/farms", content);
        }

        public async Task<HttpResponseMessage> DeleteFarmAsync(int farmId)
        {
            return await _httpClient.DeleteAsync($"api/farms/{farmId}");
        }
        // -----------------------------------------------------------------------------------------------------------------------
        public async Task<List<Watch>?> GetAvailableWatchesAsync(string? filter = null, string? sortOption = null)
        {
            // Формируем URL с параметрами
            var url = "api/Watches";

            var parameters = new List<string>();

            if (!string.IsNullOrEmpty(filter))
            {
                parameters.Add($"filter={Uri.EscapeDataString(filter)}");
            }

            if (!string.IsNullOrEmpty(sortOption))
            {
                parameters.Add($"sortOption={Uri.EscapeDataString(sortOption)}");
            }

            if (parameters.Any())
            {
                url += "?" + string.Join("&", parameters);
            }

            // Делаем запрос к серверу
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Watch>>(json);
            }

            return null;
        }

        public async Task<List<UserRequest>?> GetRequestsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/EmployeeRequests");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<UserRequest>>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении заявок: {ex.Message}");
            }
            return null;
        }

        // Обновление статуса заявки
        public async Task<bool> UpdateRequestStatusAsync(int requestId, int newStatusId, int employeeId)
        {
            var payload = new
            {
                NewStatusId = newStatusId,
                EmployeeId = employeeId
            };

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/EmployeeRequests/{requestId}/status", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении статуса заявки: {ex.Message}");
                return false;
            }
        }

        // Получение employeeId на основе userId
        public async Task<int?> GetEmployeeIdByUserIdAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/EmployeeRequests/{userId}/employeeId");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<int?>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении employeeId: {ex.Message}");
            }
            return null;
        }

        public async Task<List<UserOrder>?> GetOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/EmployeeOrders");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<UserOrder>>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении заказов: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, int newStatusId, int employeeId)
        {
            var payload = new
            {
                NewStatusId = newStatusId,
                EmployeeId = employeeId
            };

            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PutAsync($"api/EmployeeOrders/{orderId}/status", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении статуса заказа: {ex.Message}");
            }
            return false;
        }

        public class Watch
        {
            public int WatchId { get; set; }
            public string ModelName { get; set; } = string.Empty;
            public string Brand { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public string? CaseMaterial { get; set; }
            public string? StrapMaterial { get; set; }
            public decimal? CaseDiameter { get; set; }
            public string? WaterResistance { get; set; }
            public byte[]? Image { get; set; }

            public Bitmap? ImagePreview
            {
                get
                {
                    if (Image == null)
                        return null;

                    using (var stream = new MemoryStream(Image))
                    {
                        return new Bitmap(stream);
                    }
                }
            }


        }
        public async Task<List<Order>?> GetUserOrdersAsync(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("api/Orders");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Order>>(json);
            }

            return null;
        }

        public async Task<bool> CreateOrderAsync(CreateOrderRequest orderRequest)
        {
            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(orderRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("api/Orders", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Server Response: {responseContent}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateOrderAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<List<UserOrder>?> GetUserOrderHistoryAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/Orders/{userId}/history");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<UserOrder>>(json);
            }
            return null;
        }

        public async Task<bool> CreateRequestAsync(CreateRequestRequest request)
        {
            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("api/Requests", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании заявки: {ex.Message}");
                return false;
            }
        }

        public async Task<List<UserRequest>?> GetUserRequestHistoryAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Requests/{userId}/history");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<UserRequest>>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке заявок: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> AddWatchAsync(ApiClient.AddWatchRequest request)
        {
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/EmployeeWatches", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UploadWatchImageAsync(int watchId, byte[] imageBytes, string fileName)
        {
            try
            {
                var byteArrayContent = new ByteArrayContent(imageBytes);
                byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                using var content = new MultipartFormDataContent
        {
            { byteArrayContent, "image", fileName }
        };

                var response = await _httpClient.PostAsync($"api/EmployeeWatches/upload-image/{watchId}", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                return false;
            }
        }

        public async Task<byte[]?> GetWatchImageAsync(int watchId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/EmployeeWatches/{watchId}/image");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения для часов {watchId}: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> RemoveWatchAsync(int watchId)
        {
            var response = await _httpClient.DeleteAsync($"api/EmployeeWatches/{watchId}");
            return response.IsSuccessStatusCode;
        }
        public async Task<List<WatchType>?> GetWatchTypesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/WatchTypes");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<WatchType>>(json);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetWatchTypesAsync: {ex.Message}");
                return null;
            }
        }

        public class WatchType
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class AddWatchRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Brand { get; set; } = string.Empty;
            public int TypeId { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public string? CaseMaterial { get; set; }
            public string? StrapMaterial { get; set; }
            public decimal? CaseDiameter { get; set; }
            public string? WaterResistance { get; set; }
        }

        public class UserRequest
        {
            public int RequestId { get; set; }
            public int UserId { get; set; }
            public string ClientName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string RequestType { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime CreationDate { get; set; }
            public string TargetWatchName { get; set; } = string.Empty;
            public string TargetBrand { get; set; } = string.Empty;
            public decimal? TargetPriceRange { get; set; }
        }

        public class CreateRequestRequest
        {
            public int UserId { get; set; }
            public string Description { get; set; } = string.Empty;
            public string RequestType { get; set; } = string.Empty;
            public string? TargetWatchName { get; set; }
            public string? TargetBrand { get; set; }
            public decimal? TargetPriceRange { get; set; }
        }

        public class UserOrder
        {
            public int OrderId { get; set; }
            public string ClientName { get; set; } = string.Empty;
            public string ModelName { get; set; } = string.Empty;
            public string Brand { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime OrderDate { get; set; }
            public string DeliveryAddress { get; set; } = string.Empty;
        }

        public class CreateOrderRequest
        {
            public int UserId { get; set; }
            public int WatchId { get; set; }
            public int Quantity { get; set; }
            public string DeliveryAddress { get; set; } = string.Empty;
        }
        public class Order
        {
            public int Id { get; set; }
            public string WatchName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public DateTime OrderDate { get; set; }
            public string DeliveryAddress { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        public class OrderCreateRequest
        {
            public int WatchId { get; set; }
            public int Quantity { get; set; }
            public string DeliveryAddress { get; set; } = string.Empty;
        }



        public class RegistrationRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
        }

        public async Task<List<ArchivedRequestView>?> GetArchivedRequestsAsync(
            int? userId = null, DateTime? fromDate = null, DateTime? toDate = null, bool descending = false)
        {
            try
            {
                var uri = "api/Archives/requests";
                var query = new List<string>();

                if (userId.HasValue)
                {
                    query.Add($"userId={userId}");
                }
                if (fromDate.HasValue)
                {
                    query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                }
                if (toDate.HasValue)
                {
                    query.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                }
                query.Add($"descending={descending.ToString().ToLower()}");

                uri += "?" + string.Join("&", query);

                var response = await _httpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ArchivedRequestView>>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении архивных заявок: {ex.Message}");
            }
            return null;
        }

        public async Task<List<ArchivedOrderView>?> GetArchivedOrdersAsync(
            int? userId = null, DateTime? fromDate = null, DateTime? toDate = null, bool descending = false)
        {
            try
            {
                var uri = "api/Archives/orders";
                var query = new List<string>();

                if (userId.HasValue)
                {
                    query.Add($"userId={userId}");
                }
                if (fromDate.HasValue)
                {
                    query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                }
                if (toDate.HasValue)
                {
                    query.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                }
                query.Add($"descending={descending.ToString().ToLower()}");

                uri += "?" + string.Join("&", query);

                var response = await _httpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ArchivedOrderView>>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении архивных заказов: {ex.Message}");
            }
            return null;
        }

        public class ArchivedOrderView
        {
            public int OrderId { get; set; }
            public int ClientId { get; set; }
            public string ClientName { get; set; } = string.Empty;
            public string WatchName { get; set; } = string.Empty;
            public string Brand { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime ArchivedDate { get; set; }
            public string DeliveryAddress { get; set; } = string.Empty;
        }

        public class ArchivedRequestView
        {
            public int RequestId { get; set; }
            public int ClientId { get; set; }
            public string ClientName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string RequestType { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime CreationDate { get; set; }
            public DateTime ArchivedDate { get; set; }
            public string? AssignedEmployee { get; set; }
        }
        public async Task<List<ClientUser>?> GetAllClientsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/EmployeeClients");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ClientUser>>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetAllClientsAsync: {ex.Message}");
            }
            return null;
        }

        public async Task<List<ClientUser>?> SearchClientsAsync(string name)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/EmployeeClients/search?name={Uri.EscapeDataString(name)}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ClientUser>>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в SearchClientsAsync: {ex.Message}");
            }
            return null;
        }

        public class ClientUser
        {
            public int ClientId { get; set; }
            public string ClientName { get; set; } = string.Empty;
            public string ClientEmail { get; set; } = string.Empty;
            public string ClientPhone { get; set; } = string.Empty;
        }
        public async Task<List<EmployeeInfo>?> GetEmployeesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Admin/employees");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var employees = JsonConvert.DeserializeObject<List<EmployeeInfo>>(json);

                    // Логируем информацию о полученном количестве сотрудников
                    Console.WriteLine($"Получено сотрудников из API: {employees?.Count}");
                    return employees;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetEmployeesAsync: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> AddEmployeeAsync(AddEmployeeRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/Admin/employees", content);
                return response.IsSuccessStatusCode; // Возвращаем true, если запрос успешен
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в AddEmployeeAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(int employeeId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Admin/employees/{employeeId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в DeleteEmployeeAsync: {ex.Message}");
                return false;
            }
        }

        public class EmployeeInfo
        {
            public int Id { get; set; }
            public string Position { get; set; } = string.Empty;
            public DateTime HireDate { get; set; }
            public EmployeeUserInfo UserInfo { get; set; } = new EmployeeUserInfo();
        }

        public class EmployeeUserInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
        }

        public class AddEmployeeRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Position { get; set; } = string.Empty;
        }

        public async Task<List<string>?> GetBackupFilesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Backup/backup-files");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<string>>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения бэкапов: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> CreateBackupAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("api/Backup/create-backup", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания бэкапа: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RestoreBackupAsync(string backupFileName)
        {
            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(backupFileName),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("api/Backup/restore-backup", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка восстановления: {ex.Message}");
                return false;
            }
        }

    }
}
