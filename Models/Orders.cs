using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyGame.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }
}