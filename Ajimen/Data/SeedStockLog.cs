using Ajimen.Models;

namespace Ajimen.Data
{
    public class SeedStockLog
    {
        public static void Initialize(AppDbContext context)
        {
            if (!context.StockLogs.Any())
            {
                context.StockLogs.Add(new StockLog
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
                });
                context.SaveChanges();
            }
        }
    }
}
