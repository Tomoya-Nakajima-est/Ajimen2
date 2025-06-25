namespace Ajimen.Models
{
    public class StockLog
    {
        public int StockLogId { get; set; }           // 主キー

        public string StaffId { get; set; }           // 登録者のID（＝ApplicationUser.UserName）
        public string StaffName { get; set; }         // 登録者の氏名（＝ApplicationUser.UserFullName）

        public DateTime StockDay { get; set; }        // 記録日
        public int StockNum { get; set; }          // 在庫数
        public int StockMinNum { get; set; }       // 最低在庫数

        public int ItemId { get; set; }               // 対象の物品ID
        public string ItemName { get; set; }          // 物品名（冗長化）
        public string ItemKinds { get; set; }　　　　　　//物品の種類
    }
}
