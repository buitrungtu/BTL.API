using System;
using System.Collections.Generic;

#nullable disable

namespace BTL.API.Model
{
    public partial class UserGroupPost
    {
        public Guid Id { get; set; }
        public Guid? GroupPostId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? RoleNumber { get; set; }
    }
}
