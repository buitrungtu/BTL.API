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
using System.Net;
using System.Net.Mail;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SocialNetwork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly Social_NetworkContext _db;
        public static IWebHostEnvironment _environment;


        public UsersController(Social_NetworkContext context, IWebHostEnvironment environment)
        {
            _db = context;
            _environment = environment;
        }
        [HttpPost("login")]
        public async Task<ServiceResponse> Login([FromForm] UserTb user)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var userDb = await _db.UserTbs.Where(_ => _.UserName == user.UserName && _.Password == user.Password).ToListAsync();
                if (userDb == null || userDb.Count < 1)
                {
                    res.Message = SysMessage.LoginErr;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                    return res;
                }
                Dictionary<string, object> result = new Dictionary<string, object>();
                result.Add("user_data", userDb);
                res.Data = result;
                res.Success = true;
            }
            catch(Exception ex)
            {
                res.Success = false;
                res.Message = ex.ToString();
                res.Data = null;
                return res;
            }
            return res;
        }

        
        [HttpPost("change_password")]
        public async Task<ServiceResponse> ChangePassWord(UserTb user)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var userDb = await _db.UserTbs.FirstOrDefaultAsync(_ => _.Id == user.Id && _.Password == user.Password);
                userDb.Password = user.NewPassword;
                await _db.SaveChangesAsync();
                res.Message = SysMessage.Success;
                res.Success = true;
            }
            catch
            {
                res.Message = SysMessage.ErrorMsg;
                res.ErrorCode = 404;
                res.Success = false;
                res.Data = null;
            }

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
                string sql_search = "select * from dbo.user_tb where CHARINDEX(@txtSeach,user_name) > 0 or CHARINDEX(@txtSeach,full_name) > 0";
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
        public async Task<ServiceResponse> UserPost([FromForm] UserTb user)
        {
            ServiceResponse res = new ServiceResponse();
            var userDb = await _db.UserTbs.Where(_ => _.UserName == user.UserName).ToListAsync();
            if(userDb != null && userDb.Count > 0)
            {
                res.Success = false;
                res.Message = "Tên đăng nhập đã tồn tại";
                return res;
            }
            user.Id = Guid.NewGuid();
            if (user.objFile != null)
            {
                if (!Directory.Exists(_environment.WebRootPath + "\\Upload\\"))
                {
                    Directory.CreateDirectory(_environment.WebRootPath + "\\Upload\\");
                }
                if (user.objFile.Length > 0)
                {
                    string fileName = user.Id + "_" + System.IO.Path.GetExtension(user.objFile.FileName);
                    using (FileStream fileStream = System.IO.File.Create(_environment.WebRootPath + "\\Upload\\" + fileName))
                    {
                        user.objFile.CopyTo(fileStream);
                        fileStream.Flush();
                    }
                }
            }
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
            userDb.Birthday = user.Birthday;
            userDb.Gender = user.Gender;
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

        [HttpGet("verify_email")]
        public async Task<ServiceResponse> VerifyGmail(string userName,string email)
        {
            ServiceResponse res = new ServiceResponse();

            try
            {
                var userDb = await _db.UserTbs.FirstOrDefaultAsync(_ => _.UserName == userName && _.Email == email);
                if(userDb == null)
                {
                    res.Message = SysMessage.ErrorMsg;
                    res.ErrorCode = 404;
                    res.Success = false;
                    res.Data = null;
                }
                else
                {
                    Random rd = new Random();
                    int num = rd.Next(100000,999999);
                    Dictionary<string, object> result = new Dictionary<string, object>();
                    result.Add("user_data", userDb);
                    result.Add("verify_code", num);

                    //Send email
                    string smtpAddress = "smtp.gmail.com";
                    int portNumber = 587;
                    bool enableSSL = true;
                    string emailFromAddress = Constant.EmailFromAddress; //Sender Email Address  
                    string password = Constant.Password; //Sender Password  
                    string emailToAddress = email; //Receiver Email Address  
                    string subject = "Verify Number";
                    string body = "Hello, Your verify number is " + num;

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(emailFromAddress);
                        mail.To.Add(emailToAddress);
                        mail.Subject = subject;
                        mail.Body = body;
                        mail.IsBodyHtml = true;
                        //mail.Attachments.Add(new Attachment("D:\\TestFile.txt"));//--Uncomment this to send any attachment  
                        using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                        {
                            smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                            smtp.EnableSsl = enableSSL;
                            smtp.Send(mail);
                        }
                    }

                    res.Message = SysMessage.Success;
                    res.Data = result;
                    res.Success = true;
                }
            }
            catch(Exception ex)
            {
                res.Message = SysMessage.ErrorMsg;
                res.ErrorCode = 404;
                res.Success = false;
                res.Data = null;
            }

            return res;


            //string smtpAddress = "smtp.gmail.com";
            //int portNumber = 587;
            //bool enableSSL = true;
            //string emailFromAddress = "longkieuck4@gmail.com"; //Sender Email Address  
            //string password = "long27112000"; //Sender Password  
            //string emailToAddress = "longkieuck@gmail.com"; //Receiver Email Address  
            //string subject = "Hello";
            //string body = "Hello, This is Email sending test using gmail.";

            //using (MailMessage mail = new MailMessage())
            //{
            //    mail.From = new MailAddress(emailFromAddress);
            //    mail.To.Add(emailToAddress);
            //    mail.Subject = subject;
            //    mail.Body = body;
            //    mail.IsBodyHtml = true;
            //    //mail.Attachments.Add(new Attachment("D:\\TestFile.txt"));//--Uncomment this to send any attachment  
            //    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
            //    {
            //        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
            //        smtp.EnableSsl = enableSSL;
            //        smtp.Send(mail);
            //    }
            //}

        }
    }
}
