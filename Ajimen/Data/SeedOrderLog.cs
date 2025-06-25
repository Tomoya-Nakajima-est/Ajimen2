using Ajimen.Models;

namespace Ajimen.Data
{
    public class SeedOrdeLog
    {
        public static void Initialize(AppDbContext context)
        {
            if (!context.OrderLogs.Any())
            {
                context.OrderLogs.Add(new OrderLog
                {
                    OrderLogId = 1,
                    StaffId = "0001",
                    StaffName = "谷川大虎",
                    OrderDay = new DateTime(2025, 6, 23),
                    OrderNum = 100,
                    ItemId = 1000,
                    ItemName = "麺",
                    ItemKinds = "食材"
                });
                context.SaveChanges();
            }
        }
    }
}
