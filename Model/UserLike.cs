using System;
using System.Collections.Generic;

#nullable disable

namespace BTL.API.Model
{
    public partial class UserLike
    {
        public Guid Id { get; set; }
        public Guid? PostId { get; set; }
        public Guid? UserId { get; set; }
    }
}
