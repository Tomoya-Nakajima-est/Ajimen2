using System;
using System.Collections.Generic;

namespace Ajimen.Models;

public class Shift
{
    public string ShiftId { get; set; }          // 例: "20240715-A"
    public DateTime ShiftDay { get; set; }       // 例: 2024/07/15
    public string ShiftSelect { get; set; }      // "A" or "B"
    public List<string> ShiftMembers { get; set; } = new();
    public string UseShiftLog { get; set; }      // 正社員が編集した場合用
}
