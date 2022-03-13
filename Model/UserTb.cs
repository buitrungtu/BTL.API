using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace BTL.API.Model
{
    public partial class UserTb
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public int? Gender { get; set; }
        [NotMapped]
        public string GenderString { get
            {
                
                if (this.Gender == 0) return "Nam";
                if (this.Gender == 1) return "Nữ";
                return "Khác";
            } 
        }
        public DateTime? Birthday { get; set; }
        public DateTime? CreateDate { get; set; }
        [NotMapped]
        public IFormFile? objFile { get; set; }
        [NotMapped]
        public string NewPassword { get; set; }
    }
}
