using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCC_Web_ERP.Models
{
    [Table("T_ROLE")]
    public class TRole
    {
        [Key]
        [Column("ROLE_ID")]
        public int RoleId { get; set; }

        [Required]
        [Column("ROLE_NAME")]
        [StringLength(100)]
        public required string RoleName { get; set; }

        [Column("DESCRIPTION")]
        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        public bool IsActive { get; set; }

        public ICollection<TRoleMenu> RoleMenus { get; set; } = new List<TRoleMenu>();


    }
}
