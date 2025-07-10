using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSoulsApp.Application.Models.Notifications
{
	public class NotificationFilterRequest
	{
		public string? UserID {  get; set; }
		public string? Title { get; set; }
		public string? Content { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
	}
}
