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

namespace BTL.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupPostsController : ControllerBase
    {
        private readonly Social_NetworkContext _db;
        public GroupPostsController(Social_NetworkContext context)
        {
            _db = context;
        }
        [HttpGet]
        public async Task<ActionResult<PagingData>> GetGroupByPage([FromQuery] string search, [FromQuery] int? page = 1, [FromQuery] int? record = 20)
        {
            var pagingData = new PagingData();
            List<GroupPost> records = await _db.GroupPosts.OrderByDescending(x => x.CreateDate).ToListAsync();
            //Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                string sql_search = "select * from group_post where CHARINDEX(@txtSeach,group_name) > 0";
                var param = new SqlParameter("@txtSeach", search);
                records = _db.GroupPosts.FromSqlRaw(sql_search, param).OrderByDescending(x => x.CreateDate).ToList();
            }
            //Tổng số bản ghi
            pagingData.TotalRecord = records.Count();
            //Tổng số trangalue
            pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value));
            //Dữ liệu của từng trang
            List<GroupPost> result = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList();
            pagingData.Data = result;
            return pagingData;
        }

        [HttpGet("{id}")]
        public async Task<ServiceResponse> GetGroupByUser(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            string script = "select * from group_post where id in (select group_post_id from user_group_post where user_id = @id)";
            var param = new SqlParameter("@id", id);
            res.Data = _db.GroupPosts.FromSqlRaw(script, param).OrderByDescending(x => x.CreateDate).ToList();
            res.Success = true;
            return res;
        }
        [HttpGet("users")]
        public async Task<ServiceResponse> GetUserByGroup(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            string script = "select * from user_tb where id in (select user_id from user_group_post where group_post_id = @id)";
            var param = new SqlParameter("@id", id);
            res.Data = _db.UserTbs.FromSqlRaw(script, param).ToList();
            res.Success = true;
            return res;
        }
        [HttpPost]
        public async Task<ServiceResponse> PostGroup(GroupPost groupPost)
        {
            ServiceResponse res = new ServiceResponse();
            groupPost.Id = Guid.NewGuid();
            int countMember = 0;
            if(groupPost.list_user != null && groupPost.list_user.Count() > 0)
            {
                foreach(var user in groupPost.list_user)
                {
                    //Thêm thành viên
                    UserGroupPost user_group = new UserGroupPost();
                    user_group.Id = Guid.NewGuid();
                    user_group.GroupPostId = groupPost.Id;
                    user_group.UserId = user.Id;
                    user_group.RoleNumber = 0;
                    _db.UserGroupPosts.Add(user_group);
                    countMember++;
                }
            }
            //Thêm thằng admin
            UserGroupPost admin_group = new UserGroupPost();
            admin_group.Id = Guid.NewGuid();
            admin_group.GroupPostId = groupPost.Id;
            admin_group.UserId = groupPost.UserId;
            admin_group.RoleNumber = 1;// 1 là admin
            _db.UserGroupPosts.Add(admin_group);
            groupPost.MemberCount = countMember + 1;
            groupPost.CreateDate = DateTime.Now;
            groupPost.ModifiedDate = DateTime.Now;
            _db.GroupPosts.Add(groupPost);
            await _db.SaveChangesAsync();
            res.Data = groupPost;
            res.Success = true;
            return res;

        }

        [HttpPut("edit_member")]
        public async Task<ServiceResponse> PutGroup(GroupPost group)
        {
            ServiceResponse res = new ServiceResponse();
            GroupPost groupDb = await _db.GroupPosts.FindAsync(group.Id);
            if (groupDb == null)
            {
                res.Message = SysMessage.NotFound;
                res.ErrorCode = 404;
                res.Success = false;
            }
            var listOldMember = await _db.UserGroupPosts.Where(_ => _.GroupPostId == group.Id).ToListAsync();
            _db.UserGroupPosts.RemoveRange(listOldMember);
            int countMember = 0;
            if (group.list_user != null && group.list_user.Count > 0)
            {
                foreach (var user in group.list_user)
                {
                    //Thêm thành viên
                    UserGroupPost user_group = new UserGroupPost();
                    user_group.Id = Guid.NewGuid();
                    user_group.GroupPostId = group.Id;
                    user_group.UserId = user.Id;
                    user_group.RoleNumber = 0;
                    _db.UserGroupPosts.Add(user_group);
                    countMember++;
                }
            }
            //Thêm thằng admin
            UserGroupPost admin_group = new UserGroupPost();
            admin_group.Id = Guid.NewGuid();
            admin_group.GroupPostId = group.Id;
            admin_group.UserId = group.UserId;
            admin_group.RoleNumber = 1;// 1 là admin
            _db.UserGroupPosts.Add(admin_group);
            groupDb.MemberCount = countMember + 1;
            await _db.SaveChangesAsync();
            res.Success = true;
            res.Message = SysMessage.Success;
            res.Data = groupDb;
            return res;
        }

        // DELETE api/<GroupPostsController>/5
        [HttpDelete("remove")]
        public async Task<ServiceResponse> Delete(Guid? id)
        {
            ServiceResponse res = new ServiceResponse();
            var group_db = await _db.GroupPosts.FindAsync(id);
            if (group_db != null)
            {
                res.Message = SysMessage.NotFound;
                res.ErrorCode = 404;
                res.Success = false;
            }
            _db.GroupPosts.Remove(group_db);
            //delete ở các bảng liên quan
            var lst_group_user = await _db.UserGroupPosts.Where(_ => _.GroupPostId == id).ToListAsync();
            _db.UserGroupPosts.RemoveRange(lst_group_user);
            await _db.SaveChangesAsync();
            res.Success = true;
            res.Data = group_db;
            return res;
        }
    }
}
