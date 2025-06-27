using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Ajimen.Data;
using Ajimen.Models;
using Microsoft.AspNetCore.Identity;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public AttendanceController(AppDbContext context)
    {
        _context = context;
    }

    public class StaffDto
    {
        public string staffId { get; set; }
    }

    // 出勤打刻
    [HttpPost("clock-in")]
    //public IActionResult ClockIn([FromBody] string staffId)

    public IActionResult ClockIn([FromBody] StaffDto dto)

    {
        Console.WriteLine($"受け取ったstaffId: {dto.staffId}");
        var now = DateTime.Now;
        var today = now.Date;
        var startTime = now.TimeOfDay; // ← TimeSpan で取得

        //var alreadyClockedIn = _context.Attendances
        //    .Any(a => a.StaffId == dto.staffId && a.ShiftDay == today);

        //if (alreadyClockedIn)
        //    return BadRequest("すでに本日の出勤打刻があります");

        // AttendanceId 自動採番
        var lastId = _context.Attendances
            .OrderByDescending(a => a.AttendanceId)
            .Select(a => a.AttendanceId)
            .FirstOrDefault();

        int nextId = 1;
        if (!string.IsNullOrEmpty(lastId) && int.TryParse(lastId, out var parsed))
            nextId = parsed + 1;

        var newAttendanceId = nextId.ToString("D6");

        var attendance = new Attendance
        {
            AttendanceId = newAttendanceId,
            StaffId = dto.staffId,
            ShiftDay = today,
            StartTime = startTime, // ← TimeSpan型に変更
            IsWorking = true
        };

        _context.Attendances.Add(attendance);
        _context.SaveChanges();

        return Ok(new { success = true, message = "出勤打刻しました", attendanceId = newAttendanceId });
    }

    // 退勤打刻
    [HttpPost("clock-out")]
    public IActionResult ClockOut([FromBody] StaffDto dto)
    {
        var staffId = dto.staffId;
        var now = DateTime.Now;
        var today = now.Date;
        var endTime = now.TimeOfDay; // ← TimeSpanで取得

        //var attendance = _context.Attendances
        //    .FirstOrDefault(a => a.StaffId == dto.staffId && a.ShiftDay == today);


        var attendance = _context.Attendances
         .Where(a => a.StaffId == staffId && a.ShiftDay == today && a.IsWorking)
         .ToList()
         .OrderByDescending(a => a.StartTime)
         .FirstOrDefault();



        if (attendance == null)
            return NotFound("本日の出勤データが見つかりません");

        if (!attendance.IsWorking)
            return BadRequest("すでに退勤済みです");

        attendance.EndTime = endTime;

        if (attendance.StartTime.HasValue)
        {
            // ⏰ 勤務時間（分）を計算
            attendance.WorkMinutes = (int)(endTime - attendance.StartTime.Value).TotalMinutes;
        }

        attendance.IsWorking = false;

        _context.SaveChanges();

        return Ok(new { success = true, message = "退勤打刻しました" });
    }

    // 勤怠一覧（ユーザー別、月指定）
    [HttpGet("list")]
    public IActionResult GetAttendanceList([FromQuery] string staffId, [FromQuery] int year, [FromQuery] int month)
    {
        var records = _context.Attendances
            .Where(a => a.StaffId == staffId && a.ShiftDay.Year == year && a.ShiftDay.Month == month)
            .OrderBy(a => a.ShiftDay)
            .ToList();

        return Ok(records);
    }

    // 管理者による修正
    [HttpPost("edit")]
    public IActionResult EditAttendance([FromBody] Attendance edited)
    {
        var attendance = _context.Attendances.FirstOrDefault(a => a.AttendanceId == edited.AttendanceId);
        if (attendance == null)
            return NotFound("該当する勤怠が見つかりません");

        attendance.StartTime = edited.StartTime;
        attendance.EndTime = edited.EndTime;
        attendance.WorkMinutes = (int)(edited.EndTime - edited.StartTime.Value)?.TotalMinutes;
        attendance.IsWorking = edited.IsWorking;
        attendance.UseworkLog = edited.UseworkLog;

        _context.SaveChanges();

        return Ok(new { success = true, message = "勤怠情報を更新しました" });
    }

    [HttpGet("all")]
    public IActionResult GetAllAttendances()
    {
        var attendances = _context.Attendances.ToList();
        return Ok(attendances);
    }



    [HttpGet("today")]
    public IActionResult GetTodayAttendance([FromQuery] string staffId)
    {
        var today = DateTime.Today;
        var attendance = _context.Attendances
        .FirstOrDefault(a => a.StaffId == staffId && a.ShiftDay == today);

        if (attendance == null)
            return NotFound(new { message = "本日の勤怠情報が見つかりません" });

        return Ok(attendance);
    }


    [HttpGet("debug-all")]
    public IActionResult DebugAll()
    {
        var all = _context.Attendances.ToList();
        return Ok(all);
    }


    [HttpGet("summary")]
    public IActionResult GetDailySummary([FromQuery] string staffId)
    {
        var today = DateTime.Today;

        var attendance = _context.Attendances
        .FirstOrDefault(a => a.StaffId == staffId && a.ShiftDay == today);

        var shift = _context.Shifts
        .FirstOrDefault(s => s.ShiftDay == today && s.ShiftMembers.Contains(staffId));

        var shiftType = shift?.ShiftSelect;
        var shiftTime = shiftType == "A" ? "9:00〜13:00" :
        shiftType == "B" ? "16:00〜20:00" : "未定";

        return Ok(new
        {
            date = today.ToString("yyyy-MM-dd"),
            isWorkingDay = shift != null,
            shiftType,
            shiftTime,
            isClockedIn = attendance?.IsWorking ?? false,
            isClockedOut = attendance != null && !attendance.IsWorking
        });
    }


}
