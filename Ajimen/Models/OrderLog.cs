namespace Ajimen.Models
{
    public class OrderLog
    {
        public int OrderLogId { get; set; }           // 主キー

        public string StaffId { get; set; }           // 発注者のID（＝ApplicationUser.UserName）
        public string StaffName { get; set; }         // 発注者名（＝ApplicationUser.UserFullName）

        public DateTime OrderDay { get; set; }        // 発注日
        public int OrderNum { get; set; }          // 発注数
        public DateTime useOrderLog { get; set; }          // 発注数量

        public int ItemId { get; set; }               // 発注対象の物品ID
        public string ItemName { get; set; }          // 物品名（冗長化）

        public string ItemKinds { get; set; }         //物品の種類
    }
}
