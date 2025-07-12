using System.Runtime.Serialization;

namespace BookSoulsApp.Domain.Enums
{
    public enum OrderStatus
    {
        [EnumMember(Value = "Chờ duyệt đơn")]
        Pending = 0,
        [EnumMember(Value = "Nhận đơn")]
        Accepted = 1,
        [EnumMember(Value = "Hủy đơn")]
        Cancel = 2,
        [EnumMember(Value = "Hoàn thành")]
        Completed = 3,
    }
}
