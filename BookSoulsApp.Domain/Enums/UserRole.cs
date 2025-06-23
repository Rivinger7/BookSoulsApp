using System.Runtime.Serialization;

namespace BookSoulsApp.Domain.Enums;
public enum UserRole
{
    [EnumMember(Value = "Customer")]
    Customer,

    [EnumMember(Value = "Staff")]
    Staff,

    [EnumMember(Value = "Admin")]
    Admin,
}
