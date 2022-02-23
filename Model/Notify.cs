using System;
using System.Collections.Generic;

#nullable disable

namespace BTL.API.Model
{
    public partial class Notify
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime? CreateDate { get; set; }
        public Guid? UserId { get; set; }
        public string Url { get; set; }
    }
}
