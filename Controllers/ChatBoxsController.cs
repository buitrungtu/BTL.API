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
            List<Message> records = await _db.Messages.Where(_ => _.ChatBoxId == chat_box_id).OrderByDescending(x => x.CreateDate).ToListAsync();
            //Tổng số bản ghi
            pagingData.TotalRecord = records.Count();
            //Tổng số trangalue
            pagingData.TotalPage = Convert.ToInt32(Math.Ceiling((decimal)pagingData.TotalRecord / (decimal)record.Value));
            //Dữ liệu của từng trang
            pagingData.Data = records.Skip((page.Value - 1) * record.Value).Take(record.Value).ToList();
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

    }
}
