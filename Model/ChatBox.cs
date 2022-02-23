using System;
using System.Collections.Generic;

#nullable disable

namespace BTL.API.Model
{
    public partial class ChatBox
    {
        public Guid Id { get; set; }
        public Guid? UserId1 { get; set; }
        public Guid? UserId2 { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string UserName1 { get; set; }
        public string UserName2 { get; set; }
    }
}
