using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Tên đăng nhập ")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [MaxLength(20,ErrorMessage ="Toi da 20 ki tu")]
        public string UserName { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }


    }
}
