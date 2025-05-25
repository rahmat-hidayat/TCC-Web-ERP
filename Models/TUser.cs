using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCC_Web_ERP.Models
{
    [Table("T_USER")]
    public class TUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("USER_ID")]
        public int UserId { get; set; }

        [Required]
        [Column("USER_NAME")]
        [StringLength(30)]
        public string UserName { get; set; } = string.Empty;

        
        [Column("GROUP_ID")]
        public decimal GroupId { get; set; }

        [Required]
        [Column("USER_PASSWORD")]
        [StringLength(128)]
        public required string UserPassword { get; set; } 

        [Column("LAST_LOGIN")]
        public DateTime? LastLogin { get; set; }

        
        [Column("ENT_USER")]
        [StringLength(50)]
        public required string EntUser { get; set; } 
        
        [Column("ENT_DATE")]
        public DateTime? EntDate { get; set; }

        [Column("UPT_USER")]
        [StringLength(50)]
        public string? UptUser { get; set; }

        [Column("UPT_DATE")]
        public DateTime? UptDate { get; set; }

        
        [Column("UPT_PROGRAMM")]
        [StringLength(50)]
        public required string UptProgramm { get; set; } 

        [Column("REMARK")]
        [StringLength(100)]
        public string? Remark { get; set; }

        [Column("VERSION")]
        [StringLength(10)]
        public string? Version { get; set; }

        
        [Column("STATUS")]
        [StringLength(3)]
        public string Status { get; set; } = "ACT";

        
        [Column("VALID", TypeName = "smalldatetime")]
        public DateTime Valid { get; set; }

        [Required]
        [Column("change_pass")]
        [StringLength(1)]
        public required string ChangePass { get; set; }

        
        [Column("BLOCKED")]
        public decimal Blocked { get; set; } 

        [Column("mac_add")]
        [StringLength(20)]
        public string? MacAdd { get; set; }

        [Column("esign")]
        public byte[]? Esign { get; set; }

        [Column("email")]
        [StringLength(50)]
        public string? Email { get; set; }

        
        [Column("super_user")]
        public double SuperUser { get; set; } 

        
        [Column("tablet")]
        public double Tablet { get; set; }

        
        [Column("driver")]
        public double Driver { get; set; }

        [Required]
        [Column("ROLE_ID")]
        public int? RoleId { get; set; }

        // Navigation property
        public TRole? Role { get; set; }
    }
}