namespace Ajimen.Models
{
    public class Attendance
    {
        // 勤怠ID（一意な識別子）
        public string AttendanceId { get; set; }

        // スタッフID（外部キーになる）※Userテーブルと紐づけ
        public string StaffId { get; set; }

        // 勤務した日付（年月日）→ 時間は含まず日付のみでOK
        public DateTime ShiftDay { get; set; }

        // 出勤時間（打刻）
        public TimeSpan? StartTime { get; set; }

        // 退勤時間（打刻）
        public TimeSpan? EndTime { get; set; }

        // 勤務時間（分または秒単位）→ 自動計算ならDB保存不要
        public int? WorkMinutes { get; set; }  // ← 保存したい場合

        // 勤務状態（現在出勤中か）→ true = 出勤中 / false = 未出勤・退勤済
        public bool IsWorking { get; set; }

        // 勤怠記録を編集した管理者のID（任意）a
        public string? UseworkLog { get; set; }
    }
}
