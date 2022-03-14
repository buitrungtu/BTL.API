using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace BTL.API.Model
{
    public partial class Post
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreateDate { get; set; }
        [NotMapped]
        public string CreateDateString { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? GroupId { get; set; }
        public Guid? UserId { get; set; }
        public string FullName { get; set; }
        public int? LikesCount { get; set; }
        public int? CommentCount { get; set; }
        [NotMapped]
        public List<Image> post_image { get; set; }

        [NotMapped]
        public List<IFormFile> objFile { get; set; }
        [NotMapped]
        public List<Comment> lstCmt { get; set; }

        [NotMapped]
        public bool isLike { get; set; }
    }
}
