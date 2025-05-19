using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCC_Web_ERP.Models
{
    [Table("T_ROLE_MENU")]
    public class TRoleMenu
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Role")]
        public int RoleId { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("Menu")]
        public int MenuId { get; set; }

        [Required]
        [Column("IS_VISIBLE")]
        public bool IsVisible { get; set; }

        public required virtual TRole Role { get; set; }
        public required virtual TMenu Menu { get; set; }
        public bool IsActive { get; internal set; }
    }
}
