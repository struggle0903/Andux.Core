using System.ComponentModel.DataAnnotations;

namespace Andux.Core.Testing.Model
{
    public class ValidationModel
    {
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "用户名长度必须在2-20之间")]
        public string UserName { get; set; }

        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }
    }
}
