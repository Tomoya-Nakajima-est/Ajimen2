using Ajimen.Models;

namespace Ajimen.Data
{
    public class SeedStockLog
    {
        public static void Initialize(AppDbContext context)
        {
            if (!context.StockLogs.Any())
            {
                context.StockLogs.AddRange(
                    new StockLog
                    {
                        StockLogId = 1,
                        StaffId = "0001",
                        StaffName = "谷川大虎",
                        StockDay = new DateTime(2025, 6, 23),
                        StockNum = 100,
                        StockMinNum = 300,
                        ItemId = 1000,
                        ItemName = "麺",
                        ItemKinds = "食材"
                    },
                    new StockLog
                    {
                        StockLogId = 2,
                        StaffId = "0002",
                        StaffName = "中島朋弥",
                        StockDay = new DateTime(2025, 6, 23),
                        StockNum = 300,
                        StockMinNum = 100,
                        ItemId = 1001,
                        ItemName = "こんぶ",
                        ItemKinds = "食材"
                    },
                    new StockLog
                    {
                        StockLogId = 3,
                        StaffId = "0002",
                        StaffName = "中島朋弥",
                        StockDay = new DateTime(2025, 6, 25),
                        StockNum = 50,
                        StockMinNum = 70,
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
