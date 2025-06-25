using Ajimen.Models;

namespace Ajimen.Data
{
    public class SeedAttendance
    {
        public static void Initialize(AppDbContext context)
        {
            if (!context.Attendances.Any())
            {
                context.Attendances.Add(new Attendance
                {
                    AttendanceId = "000001",
                    StaffId = "0001",
                    ShiftDay = new DateTime(2025, 6, 23),
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    WorkMinutes = 540,
                    IsWorking = false,
                    UseworkLog = "0001"
                });
                context.SaveChanges();
            }
        }
    }
}
