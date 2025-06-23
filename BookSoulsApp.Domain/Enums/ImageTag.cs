using System.Runtime.Serialization;

namespace BookSoulsApp.Domain.Enums
{
    public enum ImageTag
    {
        [EnumMember(Value = "Users_Profile")]
        Users_Profile,

        [EnumMember(Value = "Book")]
        Book
    }
}
