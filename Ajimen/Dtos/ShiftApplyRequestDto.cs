namespace Ajimen.DTOs
{
    public class ShiftApplyRequestDto
    {
        public DateTime Day { get; set; }
        public string ShiftSelect { get; set; }
        public string MemberId { get; set; }
    }
}
