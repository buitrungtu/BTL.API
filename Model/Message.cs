using System;
using System.Collections.Generic;

#nullable disable

namespace BTL.API.Model
{
    public partial class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime? CreateDate { get; set; }
        public Guid? UserId { get; set; }
        public int? IsLike { get; set; }
        public string UserName { get; set; }
        public Guid? ChatBoxId { get; set; }
    }
}
