using Ajimen.Models;

namespace Ajimen.Data
{
    public class SeedShift
    {
        public static void Initialize(AppDbContext context)
        {
            if (!context.Shifts.Any())
            {
                context.Shifts.Add(new Shift
                {
                    ShiftId = "000001",
                    ShiftDay = new DateTime(2025, 6, 28),
                    ShiftSelect = "A",
                    ShiftMembers = new List<string>
                    {
                        "0001", 
                        "0002", 
                        "0003", 
                        "0004"  
                    },
                    UseShiftLog = "谷川大虎"
                });
                context.SaveChanges();
            }
        }
    }
}
