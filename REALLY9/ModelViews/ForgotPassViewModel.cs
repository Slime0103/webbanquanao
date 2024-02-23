using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace REALLY9.ModelViews
{
    public class ForgotPassViewModel
    {
        [Key]
        [Required(ErrorMessage  = "Registered email address")]
        
        [Display(Name = "Địa chỉ Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Remote(action: "ValidateEmail", controller: "Accounts")]
        public bool EmailSent { get; set; }

    }
}
