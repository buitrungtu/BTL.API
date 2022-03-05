using BTL.API.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SocialNetwork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly Social_NetworkContext _db;
        public static IWebHostEnvironment _environment;

        public PostsController(Social_NetworkContext context, IWebHostEnvironment environment)
        {
            _db = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<PagingData>> GetPostByPage([FromQuery] string search, [FromQuery] int? page = 1, [FromQuery] int? record = 20)
        {
            var pagingData = new PagingData();
            List<Post> records = await _db.Posts.OrderByDescending(x => x.CreateDate).ToListAsync();
            //Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                string sql_search = "select * from sme.post where CHARINDEX(@txtSeach,content) > 0";
                var param = new SqlParameter("@txtSeach", search);
                records = _db.Posts.FromSqlRaw(sql_search, param).OrderByDescending(x => x.CreateDate).ToList();
            }
            //Tổng số bản ghi
            pagingData.TotalRecord = records.Count();
            //Tổng số trangalue
            pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value));
            //Dữ liệu của từng trang
            List<Post> result = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList();
            foreach(var item in result)
            {
                item.post_image = _db.Images.Where(_ => _.PostId == item.Id).ToList();
                item.CreateDateString = this.ChuyenThoiGian(DateTime.Now.Subtract(item.CreateDate).Hours, DateTime.Now.Subtract(item.CreateDate).Minutes, DateTime.Now.Subtract(item.CreateDate).Seconds);
            }
            pagingData.Data = result;
            return pagingData;
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetImage(string fileName)
        {
            string path = _environment.WebRootPath + "\\Upload\\";
            var filePath = path + fileName;
            if (System.IO.File.Exists(filePath))
            {
                byte[] b = System.IO.File.ReadAllBytes(filePath);
                return File(b, "image/jpg");
            }
            else
            {
                byte[] b = System.IO.File.ReadAllBytes(path + "default.png");
                return File(b, "image/jpg");
            }
            return null;
        }

        [HttpGet("detail")]
        public async Task<ServiceResponse> GetPost(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var post = await _db.Posts.FindAsync(id);
            if (post == null)
            {
                res.Message = SysMessage.NotFound;
                res.ErrorCode = 404;
                res.Success = false;
                res.Data = null;
            }
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("post", post);
            //Lấy post image
            var post_image = _db.Images.Where(_ => _.PostId == post.Id);
            result.Add("post_image", post_image);
            //Lấy post comment
            var post_comment = _db.Comments.Where(_ => _.PostId == post.Id);
            result.Add("post_comment", post_comment);
            res.Data = result;
            res.Success = true;
            return res;
        }

        [HttpPost]
        public async Task<ServiceResponse> PostPost([FromForm] Post post)
        {
            ServiceResponse res = new ServiceResponse();
            post.Id = Guid.NewGuid();
            post.FullName = post.FullName;
            int i = 0;
            if(post.objFile != null)
            {
                if (!Directory.Exists(_environment.WebRootPath + "\\Upload\\"))
                {
                    Directory.CreateDirectory(_environment.WebRootPath + "\\Upload\\");
                }
                foreach (var objFile in post.objFile)
                {
                    if (objFile.Length > 0)
                    {
                        string fileName = post.Id + "_" + i + System.IO.Path.GetExtension(objFile.FileName);
                        using (FileStream fileStream = System.IO.File.Create(_environment.WebRootPath + "\\Upload\\" + fileName))
                        {
                            objFile.CopyTo(fileStream);
                            fileStream.Flush();
                            //Add vào bảng image
                            Image img = new Image();
                            img.Id = Guid.NewGuid();
                            img.PostId = post.Id;
                            img.Url = fileName;
                            _db.Images.Add(img);
                        }
                    }
                    i++;
                }
            }
            post.CreateDate = DateTime.Now;
            post.ModifiedDate = DateTime.Now;
            post.LikesCount = 0;
            post.CommentCount = 0;
            await _db.Posts.AddAsync(post);
            await _db.SaveChangesAsync();
            res.Success = true;
            res.Data = post;
            return res;
        }

        [HttpPut("edit")]
        public async Task<ServiceResponse> PutPost(Post post)
        {
            ServiceResponse res = new ServiceResponse();
            Post postDb = await _db.Posts.FindAsync(post.Id);
            if (postDb == null)
            {
                res.Message = SysMessage.NotFound;
                res.ErrorCode = 404;
                res.Success = false;
            }
            postDb.Content = post.Content;
            if (post.post_image != null && post.post_image.Count > 0)
            {
                var listOldImg = await _db.Images.Where(_ => _.PostId == postDb.Id).ToListAsync();
                _db.Images.RemoveRange(listOldImg);
                foreach (var img in post.post_image)
                {
                    img.Id = Guid.NewGuid();
                    img.PostId = postDb.Id;
                    _db.Images.Add(img);
                }
            }
            await _db.SaveChangesAsync();
            res.Success = true;
            res.Message = SysMessage.Success;
            res.Data = post;
            return res;
        }

        [HttpDelete("remove")]
        public async Task<ServiceResponse> DeletePost(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var post = await _db.Posts.FindAsync(id);
            if (post == null)
            {
                res.Message = SysMessage.NotFound;
                res.ErrorCode = 404;
                res.Success = false;
            }
            _db.Posts.Remove(post);
            //delete ở các bảng liên quan 
            var listImg = await _db.Images.Where(_ => _.PostId == post.Id).ToListAsync();
            _db.Images.RemoveRange(listImg);
            var listComment = await _db.Comments.Where(_ => _.PostId == post.Id).ToListAsync();
            _db.Comments.RemoveRange(listComment);
            var listLikes = await _db.UserLikes.Where(_ => _.PostId == post.Id).ToListAsync();
            _db.UserLikes.RemoveRange(listLikes);
            await _db.SaveChangesAsync();
            res.Success = true;
            res.Message = SysMessage.Success;
            res.Data = post;
            return res;
        }
        string ChuyenThoiGian(int gio, int phut, int giay)
        {
            if(phut < 0)
            {
                return giay + " giây trước";
            }
            if(gio <= 0)
            {
                return phut + " phút trước";
            }
            if (gio < 24)
            {
                return gio + " giờ trước";
            }
            else if (gio >= 24 && gio < 168)
            {
                return (gio / 24) + " ngày trước";
            }
            else if (gio >= 168 && gio < 672)
            {
                return (gio / 168) + " tuần trước";
            }
            else if (gio >= 672 && gio < 8064)
            {
                return (gio / 672) + " tháng trước";
            }
            else
            {
                return (gio / 8064) + " năm trước";
            }
        }
    }
}
