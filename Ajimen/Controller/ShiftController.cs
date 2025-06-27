using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Ajimen.Data;
using Ajimen.Models;
using Ajimen.DTOs;
using Microsoft.AspNetCore.Identity;
using Ajimen.Dtos;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ShiftController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ShiftController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ✅ カレンダー用：月別シフト取得
    [HttpGet("get")]
    public IActionResult GetShifts([FromQuery] int year, [FromQuery] int month, [FromQuery] string memberId)
    {
        var shifts = _context.Shifts
            .Where(s =>
                s.ShiftDay.Year == year &&
                s.ShiftDay.Month == month
            )
            .ToList();

        var result = shifts
            .GroupBy(s => s.ShiftDay.Date)
            .Select(g => new {
                date = g.Key.ToString("yyyy-MM-dd"),
                hasA = g.Any(s => s.ShiftSelect == "A" && s.ShiftMembers.Contains(memberId)),
                hasB = g.Any(s => s.ShiftSelect == "B" && s.ShiftMembers.Contains(memberId)),
                isConfirmed = g.Any(s => s.IsConfirmed)
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
            UseShiftLog = dto.MemberId,
            IsConfirmed = false
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
        var shifts = _context.Shifts
            .Select(s => new {
                shiftId = s.ShiftId,
                shiftDay = s.ShiftDay.ToString("yyyy-MM-dd"),
                shiftSelect = s.ShiftSelect,
                memberId = s.ShiftMembers.FirstOrDefault(), // 必要なら複数対応に
                isConfirmed = s.IsConfirmed
            }).ToList();

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
    [AllowAnonymous]
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmShifts([FromBody] List<ShiftConfirmRequest> confirmList)
    {
        if (confirmList == null || confirmList.Count == 0)
            return BadRequest("編集内容が空です");

        var editor = await _userManager.GetUserAsync(User);
        if (editor == null || string.IsNullOrEmpty(editor.Email))
            return Unauthorized("ログインユーザーが見つかりません");

        try
        {
            var notifyMap = new Dictionary<string, List<ShiftConfirmRequest>>();

            foreach (var item in confirmList)
            {
                var start = item.Date.Date;
                var end = start.AddDays(1);

                var shift = _context.Shifts
                    .FirstOrDefault(s => s.ShiftDay >= start && s.ShiftDay < end && s.ShiftSelect == item.Shift);

                if (shift != null)
                {
                    shift.IsConfirmed = true;

                    foreach (var memberId in shift.ShiftMembers)
                    {
                        if (!notifyMap.ContainsKey(memberId))
                            notifyMap[memberId] = new List<ShiftConfirmRequest>();

                        notifyMap[memberId].Add(item);
                    }
                }
            }

            _context.SaveChanges();

            // 各メンバーにメール送信（1人1通）
            foreach (var kvp in notifyMap)
            {
                var memberId = kvp.Key;
                var shiftItems = kvp.Value;

                var user = await _userManager.FindByNameAsync(memberId);
                if (user == null || string.IsNullOrEmpty(user.Email))
                    continue;

                var bodyBuilder = new StringBuilder();
                bodyBuilder.AppendLine("麺屋麺道　スタッフ　各位\n");
                bodyBuilder.AppendLine($"お疲れ様です。{editor.UserFullName}です。\n");
                bodyBuilder.AppendLine("このたび、シフトが確定いたしましたのでご連絡いたします。\n");

                foreach (var item in shiftItems)
                {
                    bodyBuilder.AppendLine($"{item.Date:yyyy年MM月dd日}：シフト{item.Shift}");
                }

                bodyBuilder.AppendLine("\nこちらのシフトでお願いします。\nよろしくお願いいたします。");

                await SendEmailAsync(editor.Email, user.Email, "シフト確定のおしらせ", bodyBuilder.ToString());
            }

            return Ok("シフト確定＆通知メール送信完了");
        }
        catch (Exception ex)
        {
            return BadRequest("エラー: " + ex.Message);
        }
    }

    // ✅ メール送信用メソッド（services.cs 不使用）
    private async Task SendEmailAsync(string from, string to, string subject, string body)
    {
        var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("tomoya.marionette.august@gmail.com", "deam soei cgrr qjvb"),
            EnableSsl = true,
        };

        var message = new MailMessage(from, to, subject, body);
        await smtp.SendMailAsync(message);
    }
    [HttpPost("confirm-by-id")]
    public IActionResult ConfirmByIds([FromBody] List<string> shiftIds)
    {
        if (shiftIds == null || shiftIds.Count == 0)
            return BadRequest("ShiftIdリストが空です");

        var targetShifts = _context.Shifts
            .Where(s => shiftIds.Contains(s.ShiftId))
            .ToList();

        if (targetShifts.Count == 0)
            return NotFound("対象のシフトが見つかりません");

        foreach (var shift in targetShifts)
        {
            shift.IsConfirmed = true;
        }

        _context.SaveChanges();

        return Ok(new { message = "対象のシフトを確定しました", count = targetShifts.Count });
    }
}
