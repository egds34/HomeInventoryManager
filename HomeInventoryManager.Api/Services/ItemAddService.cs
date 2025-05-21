using HomeInventoryManager.Data;
using HomeInventoryManager.Dto;
using HomeInventoryManager.Api.Services.Interfaces;
using HomeInventoryManager.Api.Utilities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace HomeInventoryManager.Api.Services
{
    public class ItemAddService(AppDbContext _context, IConfiguration _configuration, ILogger<AuthService> _logger) : IItemAddService
    {
        public async Task<ServiceResult<ItemAddDto>> AddItemAsync(ItemAddDto item)
        {
            var conn = _context.Database.GetDbConnection();

            await using var command = conn.CreateCommand();
            command.CommandText = "SELECT add_item(@name, @description, @user_id, @barcode, @category_id, @location_id, @photo_id, @receipt_id)";
            command.CommandType = CommandType.Text;

            command.Parameters.Add(new NpgsqlParameter("@name", item.name));
            command.Parameters.Add(new NpgsqlParameter("@description", (object?)item.description ?? DBNull.Value));
            command.Parameters.Add(new NpgsqlParameter("@user_id", item.user_id));
            command.Parameters.Add(new NpgsqlParameter("@barcode", (object?)item.barcode ?? DBNull.Value));
            command.Parameters.Add(new NpgsqlParameter("@category_id", item.category_id));
            command.Parameters.Add(new NpgsqlParameter("@location_id", item.location_id));
            command.Parameters.Add(new NpgsqlParameter("@photo_id", item.photo_id));
            command.Parameters.Add(new NpgsqlParameter("@receipt_id", item.receipt_id));

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            var result = await command.ExecuteScalarAsync();
            return ServiceResult<ItemAddDto>.Ok(item);
        }
    }
}
