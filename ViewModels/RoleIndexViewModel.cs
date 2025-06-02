using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace TCC_Web_ERP.ViewModels
{
    public class RoleIndexViewModel
    {
        public required SelectList RoleList { get; set; }
    }
}
