using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCC_Web_ERP.Models
{
    [Table("T_MENU")]
    public class TMenu
    {
        [Key]
        [Column("MENU_ID")]
        public int MenuId { get; set; }

        [Required]
        [Column("TITLE")]
        [StringLength(100)]
        public string? Title { get; set; }

        [Column("ACTION_NAME")]
        [StringLength(100)]
        public string? ActionName { get; set; }

        [Column("CONTROLLER_NAME")]
        [StringLength(100)]
        public string? ControllerName { get; set; }

        [Column("ICON")]
        [StringLength(50)]
        public string? Icon { get; set; }

        [Column("URL")]
        [StringLength(200)]
        public string? Url { get; set; }

        [Column("PARENT_ID")]
        public int? ParentId { get; set; }

        [Column("ORDER_NO")]
        public int? OrderNo { get; set; }

        [Column("IS_ACTIVE")]
        public bool IsActive { get; set; }

        public ICollection<TRoleMenu> RoleMenus { get; set; } = [];
    }
}