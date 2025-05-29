using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TCC_Web_ERP.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "User Name wajib diisi")]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role wajib dipilih")]
        public int RoleId { get; set; }

        public List<SelectListItem> RoleList { get; set; } = new();
    }
}
