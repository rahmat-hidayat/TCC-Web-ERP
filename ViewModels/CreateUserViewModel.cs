using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CreateUserViewModel
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string UserPassword { get; set; } = string.Empty;

    [Required]
    public int RoleId { get; set; }

    // Untuk dropdown
    public IEnumerable<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();
}
