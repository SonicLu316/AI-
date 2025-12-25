using AI錄音文字轉換.Models;
using AI錄音文字轉換.Services;
using Microsoft.AspNetCore.Mvc;

namespace AI錄音文字轉換.Controllers;

/// <summary>
/// 會議管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MeetingsController : ControllerBase
{
    private readonly IMeetingService _meetingService;
    private readonly ILogger<MeetingsController> _logger;

    public MeetingsController(IMeetingService meetingService, ILogger<MeetingsController> logger)
    {
        _meetingService = meetingService;
        _logger = logger;
    }

    /// <summary>
    /// 查詢會議列表
    /// </summary>
    /// <param name="request">查詢條件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>會議列表及總筆數</returns>
    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody] MeetingQueryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var (meetings, totalCount) = await _meetingService.QueryAsync(request, cancellationToken);
            
            return Ok(new
            {
                data = meetings,
                totalCount = totalCount,
                pageIndex = request.PageIndex,
                pageSize = request.PageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying meetings");
            return StatusCode(500, new { error = "查詢會議列表失敗", message = ex.Message });
        }
    }

    /// <summary>
    /// 根據ID取得會議
    /// </summary>
    /// <param name="id">會議ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>會議資料</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _meetingService.GetByIdAsync(id, cancellationToken);
            
            if (meeting == null)
            {
                return NotFound(new { error = "會議不存在" });
            }
            
            return Ok(meeting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting meeting by ID: {Id}", id);
            return StatusCode(500, new { error = "取得會議資料失敗", message = ex.Message });
        }
    }

    /// <summary>
    /// 新增會議
    /// </summary>
    /// <param name="request">會議資料</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的會議資料</returns>
    [HttpPost]
    public async Task<IActionResult> Insert([FromBody] MeetingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { error = "會議標題為必填欄位" });
            }

            if (string.IsNullOrWhiteSpace(request.Location))
            {
                return BadRequest(new { error = "會議地點為必填欄位" });
            }

            var meeting = await _meetingService.InsertAsync(request, cancellationToken);
            
            return Ok(new
            {
                success = true,
                message = "會議新增成功",
                data = meeting
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting meeting");
            return StatusCode(500, new { error = "新增會議失敗", message = ex.Message });
        }
    }

    /// <summary>
    /// 修改會議
    /// </summary>
    /// <param name="id">會議ID</param>
    /// <param name="request">會議資料</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>修改後的會議資料</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] MeetingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { error = "會議標題為必填欄位" });
            }

            if (string.IsNullOrWhiteSpace(request.Location))
            {
                return BadRequest(new { error = "會議地點為必填欄位" });
            }

            request.Id = id;
            var meeting = await _meetingService.UpdateAsync(request, cancellationToken);
            
            return Ok(new
            {
                success = true,
                message = "會議修改成功",
                data = meeting
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Meeting not found for update: {Id}", id);
            return NotFound(new { error = "會議不存在或已被刪除" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating meeting: {Id}", id);
            return StatusCode(500, new { error = "修改會議失敗", message = ex.Message });
        }
    }

    /// <summary>
    /// 刪除會議
    /// </summary>
    /// <param name="id">會議ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>刪除結果</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _meetingService.DeleteAsync(id, cancellationToken);
            
            return Ok(new
            {
                success = true,
                message = "會議刪除成功"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting meeting: {Id}", id);
            return StatusCode(500, new { error = "刪除會議失敗", message = ex.Message });
        }
    }
}
