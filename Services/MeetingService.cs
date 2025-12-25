using AI錄音文字轉換.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AI錄音文字轉換.Services;

/// <summary>
/// 會議資料庫服務
/// </summary>
public interface IMeetingService
{
    Task<Meeting> InsertAsync(MeetingRequest request, CancellationToken cancellationToken = default);
    Task<Meeting> UpdateAsync(MeetingRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<Meeting> meetings, int totalCount)> QueryAsync(MeetingQueryRequest request, CancellationToken cancellationToken = default);
    Task<Meeting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// 會議資料庫服務實作
/// </summary>
public class MeetingService : IMeetingService
{
    private readonly string _connectionString;
    private readonly ILogger<MeetingService> _logger;

    public MeetingService(IConfiguration configuration, ILogger<MeetingService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    /// <summary>
    /// 新增會議
    /// </summary>
    public async Task<Meeting> InsertAsync(MeetingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var meetingId = Guid.NewGuid();
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = new SqlCommand("sp_Meeting_Insert", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Id", meetingId);
            command.Parameters.AddWithValue("@Title", request.Title);
            command.Parameters.AddWithValue("@MeetingTime", request.MeetingTime);
            command.Parameters.AddWithValue("@Location", request.Location);
            command.Parameters.AddWithValue("@Members", request.Members);
            command.Parameters.AddWithValue("@AudioFileId", (object?)request.AudioFileId ?? DBNull.Value);
            command.Parameters.AddWithValue("@AudioFileName", (object?)request.AudioFileName ?? DBNull.Value);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            if (await reader.ReadAsync(cancellationToken))
            {
                return MapToMeeting(reader);
            }

            throw new InvalidOperationException("Failed to insert meeting.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting meeting: {Title}", request.Title);
            throw;
        }
    }

    /// <summary>
    /// 修改會議
    /// </summary>
    public async Task<Meeting> UpdateAsync(MeetingRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.Id.HasValue)
        {
            throw new ArgumentException("Meeting ID is required for update.", nameof(request));
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = new SqlCommand("sp_Meeting_Update", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Id", request.Id.Value);
            command.Parameters.AddWithValue("@Title", request.Title);
            command.Parameters.AddWithValue("@MeetingTime", request.MeetingTime);
            command.Parameters.AddWithValue("@Location", request.Location);
            command.Parameters.AddWithValue("@Members", request.Members);
            command.Parameters.AddWithValue("@AudioFileId", (object?)request.AudioFileId ?? DBNull.Value);
            command.Parameters.AddWithValue("@AudioFileName", (object?)request.AudioFileName ?? DBNull.Value);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            if (await reader.ReadAsync(cancellationToken))
            {
                return MapToMeeting(reader);
            }

            throw new InvalidOperationException($"Meeting with ID {request.Id} not found or already deleted.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating meeting: {Id}", request.Id);
            throw;
        }
    }

    /// <summary>
    /// 刪除會議 (軟刪除)
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = new SqlCommand("sp_Meeting_Delete", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Id", id);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting meeting: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// 查詢會議列表
    /// </summary>
    public async Task<(List<Meeting> meetings, int totalCount)> QueryAsync(MeetingQueryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var meetings = new List<Meeting>();
            int totalCount = 0;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = new SqlCommand("sp_Meeting_Query", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@PageIndex", request.PageIndex);
            command.Parameters.AddWithValue("@PageSize", request.PageSize);
            command.Parameters.AddWithValue("@SearchKeyword", (object?)request.SearchKeyword ?? DBNull.Value);
            command.Parameters.AddWithValue("@StartDate", (object?)request.StartDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@EndDate", (object?)request.EndDate ?? DBNull.Value);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            // 讀取會議列表
            while (await reader.ReadAsync(cancellationToken))
            {
                meetings.Add(MapToMeeting(reader));
            }

            // 讀取總筆數
            if (await reader.NextResultAsync(cancellationToken) && await reader.ReadAsync(cancellationToken))
            {
                totalCount = reader.GetInt32(0);
            }

            return (meetings, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying meetings");
            throw;
        }
    }

    /// <summary>
    /// 根據ID取得會議
    /// </summary>
    public async Task<Meeting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = new MeetingQueryRequest { PageIndex = 1, PageSize = 1 };
        var (meetings, _) = await QueryAsync(request, cancellationToken);
        return meetings.FirstOrDefault(m => m.Id == id);
    }

    /// <summary>
    /// 將 DataReader 映射到 Meeting 物件
    /// </summary>
    private static Meeting MapToMeeting(SqlDataReader reader)
    {
        return new Meeting
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            MeetingTime = reader.GetDateTimeOffset(reader.GetOrdinal("MeetingTime")),
            Location = reader.GetString(reader.GetOrdinal("Location")),
            Members = reader.GetString(reader.GetOrdinal("Members")),
            AudioFileId = reader.IsDBNull(reader.GetOrdinal("AudioFileId")) 
                ? null 
                : reader.GetGuid(reader.GetOrdinal("AudioFileId")),
            AudioFileName = reader.IsDBNull(reader.GetOrdinal("AudioFileName")) 
                ? null 
                : reader.GetString(reader.GetOrdinal("AudioFileName")),
            CreatedAt = reader.GetDateTimeOffset(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) 
                ? null 
                : reader.GetDateTimeOffset(reader.GetOrdinal("UpdatedAt")),
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
        };
    }
}
