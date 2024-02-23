using System.ComponentModel.DataAnnotations;

namespace REALLY9.ModelViews
{
    public class CommentViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; }
       
        public int ProductId { get; set; }

        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Message is required.")]
        public string Comment { get; set; }
    }
}
