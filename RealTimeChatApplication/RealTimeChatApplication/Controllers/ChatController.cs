using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealTimeChatApplication.Data;
using RealTimeChatApplication.Hubs;
using RealTimeChatApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeChatApplication.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        private readonly IHubContext<ChatHub> chat;
        public ChatController(IHubContext<ChatHub> chat)
        {
            this.chat = chat;
        }
        [HttpPost("[action]/{connectionId}/{roomId}")]
        public async Task<IActionResult> JoinRoom(string connectionId, string roomId)
        {
            await chat.Groups.AddToGroupAsync(connectionId, roomId);
            return Ok();
        }
        [HttpPost("[action]/{connectionId}/{roomId}")]
        public async Task<IActionResult> LeaveRoom(string connectionId, string roomId)
        {
            await chat.Groups.RemoveFromGroupAsync(connectionId, roomId);
            return Ok();
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> SendMessage(string msg, int roomId,[FromServices] ApplicationDbContext ctx)
        {
            var mess = new Message
            {
                ChatId = roomId,
                Text = msg,
                Name = User.Identity.Name,
                Timestamp = DateTime.Now
            };
            ctx.Messages.Add(mess);
            await ctx.SaveChangesAsync();
            await chat.Clients.Group(roomId.ToString()).SendAsync("RecieveMessage", new { 
                    Text = mess.Text,
                    Name = mess.Name,
                    Timestamp = mess.Timestamp.ToString("dd/MM/yyy hh:mm:ss")
            });
            return Ok();
        }
    }
}
