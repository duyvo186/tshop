using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TeaShop.Models
{
    public class OrderViewModel
    {
        [Required(ErrorMessage = "Tên khách hàng không để trống")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Số điện thoại không để trống")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Địa chỉ khổng để trống")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Email không để trống")]
        public string Email { get; set; }
        public int TypePayment { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int DeliveryHour { get; set; }
        public int DeliveryMinute { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string PassWord { get; set; }

        public string FullName { get; set; }

        public string DeliveryAddress { get; set; }
        public string DeliveryName { get; set; }
        public string DeliveryPhoneNumber { get; set; }
    }
}