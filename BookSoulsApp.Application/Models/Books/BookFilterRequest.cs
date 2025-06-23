using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSoulsApp.Application.Models.Books;
public class BookFilterRequest
{
    public string? Title { get; set; } // Tên sách
    public string? Author { get; set; } // Tác giả của sách
    public string? Isbn { get; set; } // Mã ISBN của sách
    public string? PublisherId { get; set; } // ID nhà xuất bản
    public List<string>? CategoryIds { get; set; } // Danh sách ID danh mục sản phẩm
    public int? ReleaseYear { get; set; } // Năm phát hành
    public bool? IsStricted { get; set; } // Trạng thái sản phẩm có hạn chế hay không

    public decimal? MinPrice { get; set; } // Giá tối thiểu
    public decimal? MaxPrice { get; set; } // Giá tối đa
    public int? MinStockQuantity { get; set; } // Số lượng tồn kho tối thiểu
    public int? MaxStockQuantity { get; set; } // Số lượng tồn kho tối đa
}
