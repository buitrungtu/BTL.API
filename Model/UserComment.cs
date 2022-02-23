using System;
using System.Collections.Generic;

#nullable disable

namespace BTL.API.Model
{
    public partial class UserComment
    {
        public Guid Id { get; set; }
        public Guid? CommentId { get; set; }
        public Guid? UserId { get; set; }
    }
}
