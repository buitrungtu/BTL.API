using System;
using System.Collections.Generic;

#nullable disable

namespace BTL.API.Model
{
    public partial class Comment
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? PostId { get; set; }
        public string UserName { get; set; }
        public Guid? UserId { get; set; }
    }
}
