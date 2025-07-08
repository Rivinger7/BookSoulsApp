using System.Runtime.Serialization;

namespace BookSoulsApp.Domain.Enums
{
    public enum PaymentStatus
    {
        [EnumMember(Value = "Chưa trả tiền")]
        None = 0,
        [EnumMember(Value = "Đã trả tiền")]
        Paid = 1,
        [EnumMember(Value = "Hoàn tiền")]
        Refund = 2,
    }
}
