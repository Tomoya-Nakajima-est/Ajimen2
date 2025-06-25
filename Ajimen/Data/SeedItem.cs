using Ajimen.Models;

namespace Ajimen.Data
{
    public class SeedItem
    {
        public static void Initialize(AppDbContext context)
        {
            if (!context.Items.Any())
            {
                context.Items.AddRange(
                    new Item
                    {
                        ItemId = 1000,
                        ItemName = "麺",
                        ItemKinds = "食材"
                    },
                    new Item
                    {
                        ItemId = 2000,
                        ItemName = "みそ",
                        ItemKinds = "調味料"
                    },
                    new Item
                    {
                        ItemId = 1001,
                        ItemName = "こんぶ",
                        ItemKinds = "食材"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
