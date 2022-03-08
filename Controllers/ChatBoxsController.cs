using BTL.API.Model;
using Microsoft.AspNetCore.Mvc;
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
    public class ChatBoxsController : ControllerBase
    {
        private readonly Social_NetworkContext _db;

        public ChatBoxsController(Social_NetworkContext context)
        {
            _db = context;
        }
        [HttpGet]
        public async Task<ActionResult<PagingData>> GetChatBoxByUserAsync(Guid? id, int? page = 1, int? record = 20)
        {
            var pagingData = new PagingData();
            List<ChatBox> records = await _db.ChatBoxes.OrderByDescending(x => x.ModifiedDate).ToListAsync();
            //Tổng số bản ghi
            pagingData.TotalRecord = records.Count();
            //Tổng số trangalue
            pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value));
            //Dữ liệu của từng trang
            pagingData.Data = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList();
            return pagingData;
        }

        [HttpPost]
        public async Task<ServiceResponse> ChatBoxPost(ChatBox chatbox)
        {
            ServiceResponse res = new ServiceResponse();
            chatbox.Id = Guid.NewGuid();
            chatbox.CreateDate = DateTime.Now;
            chatbox.ModifiedDate = DateTime.Now;
            await _db.ChatBoxes.AddAsync(chatbox);
            await _db.SaveChangesAsync();
            res.Success = true;
            res.Data = chatbox;
            return res;
        }

        [HttpGet("detail")]
        public async Task<ActionResult<PagingData>> GetChatBoxDetail(Guid? chat_box_id, int? page = 1, int? record = 20)
        {
            var pagingData = new PagingData();
            List<Message> records = await _db.Messages.Where(_ => _.ChatBoxId == chat_box_id).OrderBy(x => x.CreateDate).ToListAsync();
            //Tổng số bản ghi
            pagingData.TotalRecord = records.Count();
            //Tổng số trangalue
            pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value));
            //Dữ liệu của từng trang
            pagingData.Data = records.Skip(records.Count()-(page.Value  * record.Value)).Take(record.Value).ToList();
            return pagingData;
        }
        [HttpPost("add_message")]
        public async Task<ServiceResponse> MessagePost(Message mess)
        {
            ServiceResponse res = new ServiceResponse();
            mess.Id = Guid.NewGuid();
            mess.CreateDate = DateTime.Now;
            await _db.Messages.AddAsync(mess);
            await _db.SaveChangesAsync();
            res.Success = true;
            res.Data = mess;
            return res;
        }
        /**
         * Hàm truyền vào 2 userId và lấy về chat id
         * Nếu có r thì trả về chatbox tương ứng.
         * Nếu chưa có thì thêm 1 chatbox vào r chả về chatbox đó
         */
        [HttpGet("chatbox_id")]
        public async Task<ServiceResponse> GetChatBoxId(Guid? userId1, Guid? userId2)
        {
            ServiceResponse res = new ServiceResponse();
            try
            {
                var chatbox = _db.ChatBoxes.FirstOrDefault(e => e.UserId1 == userId1 && e.UserId2 == userId2 ||
                e.UserId1 == userId2 && e.UserId2 == userId1);

                if (chatbox == null)
                {
                    ChatBox cb = new ChatBox();
                    cb.Id = Guid.NewGuid();
                    cb.UserId1 = userId1;
                    cb.UserName1 = _db.UserTbs.Find(userId1).FullName;
                    cb.UserId2 = userId2;
                    cb.UserName2 = _db.UserTbs.Find(userId2).FullName;
                    cb.CreateDate = DateTime.Now;
                    cb.ModifiedDate = DateTime.Now;
                    await _db.ChatBoxes.AddAsync(cb);
                    await _db.SaveChangesAsync();
                    res.Data = cb;
                }
                else
                {
                    res.Data = chatbox;
                }
                res.Success = true;
            }
            catch
            {
                res.Success = false;
                res.Message = "Có lỗi sảy ra";
                res.ErrorCode = 400;
            }

            return res;
        }

    }
}
