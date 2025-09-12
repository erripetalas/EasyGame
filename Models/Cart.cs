using System;
using System.ComponentModel.DataAnnotations;

namespace EasyGame.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }

        public DateTime DateAdded { get; set; }
    }
}
