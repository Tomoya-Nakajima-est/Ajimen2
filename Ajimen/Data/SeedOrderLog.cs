using Ajimen.Models;

namespace Ajimen.Data
{
    public class SeedOrderLog
    {
        public static void Initialize(AppDbContext context)
        {
            if (!context.OrderLogs.Any())
            {
                context.OrderLogs.AddRange(
                    new OrderLog
                    {
                        OrderLogId = 1,
                        StaffId = "0001",
                        StaffName = "谷川大虎",
                        OrderDay = new DateTime(2025, 6, 23),
                        OrderNum = 100,
                        ItemId = 1000,
                        ItemName = "麺",
                        ItemKinds = "食材"
                    },
                    new OrderLog
                    {
                        OrderLogId = 2,
                        StaffId = "0002",
                        StaffName = "中島朋弥",
                        OrderDay = new DateTime(2025, 6, 23),
                        OrderNum = 300,
                        ItemId = 1001,
                        ItemName = "こんぶ",
                        ItemKinds = "食材"
                    },
                    new OrderLog
                    {
                        OrderLogId = 3,
                        StaffId = "0002",
                        StaffName = "中島朋弥",
                        OrderDay = new DateTime(2025, 6, 25),
                        OrderNum = 50,
                        ItemId = 2000,
                        ItemName = "味噌",
                        ItemKinds = "調味料"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
