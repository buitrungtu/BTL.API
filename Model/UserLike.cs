using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace BTL.API.Model
{
    public partial class UserLike
    {
        public Guid Id { get; set; }
        public Guid? PostId { get; set; }
        public Guid? UserId { get; set; }
        [NotMapped]
        public bool isLike { get; set; }
    }
}
