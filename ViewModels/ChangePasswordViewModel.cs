using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace TCC_Web_ERP.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "UserId wajib diisi")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Password baru wajib diisi")]
        [DataType(DataType.Password)]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Konfirmasi password wajib diisi")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password dan konfirmasi password tidak sama")]
        public required string ConfirmPassword { get; set; }
    }
}
