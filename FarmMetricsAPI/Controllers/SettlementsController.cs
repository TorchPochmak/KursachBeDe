using Microsoft.AspNetCore.Mvc;
using FarmMetricsAPI.Data;
using FarmMetricsAPI.Models.Postgres;
using Microsoft.EntityFrameworkCore;
using FarmMetricsAPI.Models.Mongo;
using FarmMetricsAPI.Data.MongoDb;
using MongoDB.Driver;

namespace FarmMetricsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettlementsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MongoDbContext _mongoDbContext;

        public SettlementsController(AppDbContext context, MongoDbContext mongoDbContext)
        {
            _context = context;
            _mongoDbContext = mongoDbContext;
        }

        /// <summary>
        /// Получает список всех поселений
        /// </summary>
        /// <returns>Список поселений с их идентификаторами и названиями</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var settlements = await _context.Settlements
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToListAsync();

            return Ok(settlements);
        }

        /// <summary>
        /// Обновляет привязку пользователя к поселению
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="request">Данные о новом поселении</param>
        /// <returns>Результат обновления</returns>
        [HttpPut("user/{userId}/settlement")]
        public async Task<IActionResult> UpdateUserSettlement(int userId, [FromBody] UpdateSettlementRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var settlement = await _context.Settlements.FindAsync(request.SettlementId);
            if (settlement == null)
            {
                return NotFound("Settlement not found");
            }

            user.SettlementId = request.SettlementId;
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Удаляет привязку пользователя к поселению
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Результат удаления привязки</returns>
        [HttpDelete("user/{userId}/settlement")]
        public async Task<IActionResult> RemoveUserSettlement(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.SettlementId = null;
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Создает новое поселение
        /// </summary>
        /// <param name="request">Данные нового поселения</param>
        /// <returns>Информация о созданном поселении</returns>
        [HttpPost]
        public async Task<IActionResult> AddSettlement([FromBody] AddSettlementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Name required");

            var exists = await _context.Settlements.AnyAsync(s => s.Name == request.Name);
            if (exists)
                return BadRequest("Settlement already exists");

            var settlement = new Settlement { Name = request.Name };
            _context.Settlements.Add(settlement);
            await _context.SaveChangesAsync();

            return Ok(new { settlement.Id, settlement.Name });
        }

        /// <summary>
        /// Проверяет возможность удаления поселения
        /// </summary>
        /// <param name="settlementId">Идентификатор поселения</param>
        /// <returns>Результат проверки возможности удаления</returns>
        [HttpGet("{settlementId}/can-delete")]
        public async Task<IActionResult> CanDeleteSettlement(int settlementId)
        {
            var hasUsers = await _context.Users.AnyAsync(u => u.SettlementId == settlementId);

            var hasDevices = await _context.SettleMetricDevices.AnyAsync(d => d.SettlementId == settlementId);

            var hasFarms = await _mongoDbContext.Farms.Find(f => f.SettlementId == settlementId).AnyAsync();

            return Ok(!hasUsers && !hasDevices);
        }

        /// <summary>
        /// Удаляет поселение
        /// </summary>
        /// <param name="id">Идентификатор поселения</param>
        /// <returns>Результат удаления</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var settlement = await _context.Settlements.FindAsync(id);
            if (settlement == null)
            {
                return NotFound();
            }

            bool hasUsers = await _context.Users.AnyAsync(u => u.SettlementId == id);
            bool hasDevices = await _context.SettleMetricDevices.AnyAsync(d => d.SettlementId == id);

            if (hasUsers || hasDevices)
            {
                return BadRequest("Cannot delete settlement with related users or devices");
            }

            _context.Settlements.Remove(settlement);
            await _context.SaveChangesAsync();
            return Ok();
        }

        public class AddSettlementRequest
        {
            public string Name { get; set; } = string.Empty;
        }

        public class UpdateSettlementRequest
        {
            public int SettlementId { get; set; }
        }
    }
}