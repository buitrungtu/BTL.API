using BTL.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SocialNetwork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly Social_NetworkContext _db;

        public UsersController(Social_NetworkContext context)
        {
            _db = context;
        }
        [HttpPost("login")]
        public async Task<ServiceResponse> Login(UserTb user)
        {
            ServiceResponse res = new ServiceResponse();
            var userDb = await _db.UserTbs.Where(_ => _.UserName == user.UserName && _.Password == user.Password).ToListAsync();
            if (user == null)
            {
                res.Message = SysMessage.LoginErr;
                res.ErrorCode = 404;
                res.Success = false;
                res.Data = null;
            }
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("user_data", userDb);
            res.Data = result;
            res.Success = true;
            return res;
        }

        [HttpGet]
        public async Task<ActionResult<PagingData>> GetUserByPage([FromQuery] string search, [FromQuery] int? page = 1, [FromQuery] int? record = 20)
        {
            var pagingData = new PagingData();
            List<UserTb> records = await _db.UserTbs.ToListAsync();
            //Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                string strSearch = "'%" + search + "%'";
                string sql_search = "select * from sme.user where CHARINDEX(@txtSeach,user_name) > 0 or CHARINDEX(@txtSeach,full_name) > 0";
                var param = new SqlParameter("@txtSeach", search);
                records = _db.UserTbs.FromSqlRaw(sql_search, param).ToList();
            }
            //Tổng số bản ghi
            pagingData.TotalRecord = records.Count();
            //Tổng số trangalue
            pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value));
            //Dữ liệu của từng trang
            pagingData.Data = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList();
            return pagingData;
        }

        [HttpGet("detail")]
        public async Task<ServiceResponse> GetUserDetail(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var user = await _db.UserTbs.FindAsync(id);
            if (user == null)
            {
                res.Message = SysMessage.NotFound;
                res.ErrorCode = 404;
                res.Success = false;
                res.Data = null;
            }
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("user_data", user);
            res.Data = result;
            res.Success = true;
            return res;
        }

        [HttpPost]
        public async Task<ServiceResponse> UserPost(UserTb user)
        {
            ServiceResponse res = new ServiceResponse();
            user.Id = Guid.NewGuid();
            await _db.UserTbs.AddAsync(user);
            await _db.SaveChangesAsync();
            res.Success = true;
            res.Data = user;
            return res;
        }

        [HttpPut("edit")]
        public async Task<ServiceResponse> EditUser(UserTb user)
        {
            ServiceResponse res = new ServiceResponse();
            UserTb userDb = await _db.UserTbs.FindAsync(user.Id);
            if (userDb == null)
            {
                res.Message = SysMessage.NotFound;
                res.ErrorCode = 404;
                res.Success = false;
            }
            userDb.FullName = user.FullName;
            userDb.Email = user.Email;
            userDb.PhoneNumber = user.PhoneNumber;
            userDb.Address = user.Address;
            userDb.Department = user.Department;
            userDb.Position = user.Position;
            await _db.SaveChangesAsync();
            res.Success = true;
            res.Message = SysMessage.Success;
            res.Data = userDb;
            return res;
        }

        [HttpDelete("remove")]
        public async Task<ServiceResponse> DeleteUser(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var user = await _db.UserTbs.FindAsync(id);
            if (user == null)
            {
                res.Message = SysMessage.NotFound;
                res.ErrorCode = 404;
                res.Success = false;
            }
            _db.UserTbs.Remove(user);
            //delete ở các bảng liên quan 
            var listUserComment = await _db.Comments.Where(_ => _.UserId == user.Id).ToListAsync();
            _db.Comments.RemoveRange(listUserComment);
            var listUserLikes = await _db.UserLikes.Where(_ => _.UserId == user.Id).ToListAsync();
            _db.UserLikes.RemoveRange(listUserLikes);
            var listUserGroupPost = await _db.UserGroupPosts.Where(_ => _.UserId == user.Id).ToListAsync();
            _db.UserGroupPosts.RemoveRange(listUserGroupPost);
            await _db.SaveChangesAsync();
            res.Success = true;
            res.Message = SysMessage.Success;
            res.Data = user;
            return res;
        }
    }
}
