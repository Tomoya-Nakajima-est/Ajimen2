using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Ajimen.Data;
using Ajimen.Models;
using Ajimen.DTOs;

[ApiController]
[Route("api/[controller]")]
public class ShiftController : ControllerBase
{
    private readonly AppDbContext _context;

    public ShiftController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ カレンダー用：月別シフト取得
    [HttpGet("get")]
    public IActionResult GetShifts([FromQuery] int year, [FromQuery] int month, [FromQuery] string memberId)
    {
        var shifts = _context.Shifts
            .Where(s =>
                s.ShiftDay.Year == year &&
                s.ShiftDay.Month == month &&
                s.ShiftMembers.Contains(memberId) // ← ユーザーIDで絞る！
            )
            .ToList();

        var result = shifts
            .GroupBy(s => s.ShiftDay.Date)
            .Select(g => new {
                date = g.Key.ToString("yyyy-MM-dd"),
                hasA = g.Any(s => s.ShiftSelect == "A"),
                hasB = g.Any(s => s.ShiftSelect == "B")
            })
            .ToList();

        return Ok(result);
    }

    // ✅ 登録処理：申請内容を受け取る
    [HttpPost("apply")]
    public IActionResult ApplyShift([FromBody] ShiftApplyRequestDto dto)
    {
        var targetDate = dto.Day.Date;
        // ✅ 同じ日付+シフトタイプがあるか探す（"Date"で比較）
        var existing = _context.Shifts
            .FirstOrDefault(s => s.ShiftDay.Date == dto.Day.Date && s.ShiftSelect == dto.ShiftSelect);

        if (existing != null)
        {
            // 既に存在 → メンバーが未登録なら追加
            if (!existing.ShiftMembers.Contains(dto.MemberId))
            {
                existing.ShiftMembers.Add(dto.MemberId);
                existing.UseShiftLog = dto.MemberId;
                _context.SaveChanges();
            }

            return Ok(new { success = true });
        }

        // ✅ 新しい ShiftId を連番で作成（既存の最大IDを調べる）
        var last = _context.Shifts
            .OrderByDescending(s => s.ShiftId)
            .FirstOrDefault();

        int nextNumber = 1;
        if (last != null && int.TryParse(last.ShiftId, out int lastId))
        {
            nextNumber = lastId + 1;
        }

        var newShift = new Shift
        {
            ShiftId = nextNumber.ToString("D6"),
            ShiftDay = dto.Day.Date,
            ShiftSelect = dto.ShiftSelect,
            ShiftMembers = new List<string> { dto.MemberId },
            UseShiftLog = dto.MemberId
        };

        _context.Shifts.Add(newShift);
        _context.SaveChanges();

        return Ok(new { success = true });
    }

    // ✅ 対象日に申請済みかどうか確認（任意で使用）
    [HttpGet("check")]
    public IActionResult CheckShift([FromQuery] DateTime day, [FromQuery] string memberId)
    {
        var result = _context.Shifts
            .Where(s => s.ShiftDay == day && s.ShiftMembers.Contains(memberId))
            .Select(s => new { s.ShiftSelect })
            .ToList();

        return Ok(result);
    }
    [HttpGet("all")]
    public IActionResult GetAllShifts()
    {
        var shifts = _context.Shifts.ToList();
        return Ok(shifts);
    }

    [HttpGet("detail")]
    public IActionResult GetShiftDetail([FromQuery] DateTime day)
    {
        var shifts = _context.Shifts
            .Where(s => s.ShiftDay.Date == day.Date)
            .ToList();

        var result = shifts.Select(s => new
        {
            shiftSelect = s.ShiftSelect,
            members = s.ShiftMembers,
            useShiftLog = s.UseShiftLog
        });

        return Ok(result);
    }
    [HttpGet("/api/members")]
    public IActionResult GetAllMembers()
    {
        // 仮の静的データ（本来はDBから）
        var members = new[]
        {
        new { id = "0001", name = "山田さん" },
        new { id = "0002", name = "田中さん" },
        new { id = "0003", name = "佐藤さん" },
        new { id = "0004", name = "鈴木さん" }
    };
        return Ok(members);
    }
    [HttpGet("editview")]
    public IActionResult GetShiftMembers([FromQuery] DateTime day, [FromQuery] string shiftSelect)
    {
        var shift = _context.Shifts
            .FirstOrDefault(s => s.ShiftDay.Date == day.Date && s.ShiftSelect == shiftSelect);

        if (shift == null)
            return Ok(new List<string>()); // 空リストを返す

        return Ok(shift.ShiftMembers);
    }
}
