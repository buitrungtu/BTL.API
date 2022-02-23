using System;
using System.Collections.Generic;

#nullable disable

namespace BTL.API.Model
{
    public partial class Image
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public Guid? PostId { get; set; }
    }
}
