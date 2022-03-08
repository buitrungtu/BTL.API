using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace BTL.API.Model
{
    public partial class GroupPost
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? MemberCount { get; set; }
        [NotMapped]
        public List<UserTb> list_user { get; set; }
        public Guid UserId { get; set; }
    }
}
