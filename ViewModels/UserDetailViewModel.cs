using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace TCC_Web_ERP.ViewModels
{
    public class UserDetailViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal GroupId { get; set; }
        public string UserPassword { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public string EntUser { get; set; } = string.Empty;
        public DateTime? EntDate { get; set; }
        public string? UptUser { get; set; }
        public DateTime? UptDate { get; set; }
        public string UptProgramm { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public string? Version { get; set; }
        public string Status { get; set; } = "ACT";
        public DateTime Valid { get; set; }
        public string ChangePass { get; set; } = string.Empty;
        public decimal Blocked { get; set; }
        public string? MacAdd { get; set; }
        public string? Email { get; set; }
        public double SuperUser { get; set; }
        public double Tablet { get; set; }
        public double Driver { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
