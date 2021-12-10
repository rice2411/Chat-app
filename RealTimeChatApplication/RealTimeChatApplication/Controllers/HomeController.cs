using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealTimeChatApplication.Data;
using RealTimeChatApplication.Models;

namespace RealTimeChatApplication.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext ctx;

        public HomeController(ApplicationDbContext ctx)
        {
            this.ctx = ctx;
        }
        [HttpPost]
        public async Task<IActionResult> CreateRoom(string name)
        {
            var chat = new Chat
            {
                Name = name,
                Type = ChatType.Room
            };
            chat.Users.Add(new ChatUser
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                Role = UserRole.Admin
            });
            ctx.Add(chat);
            await ctx.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpGet("{id}")]
        public IActionResult Chat(int id)
        {
            var chat = ctx.Chats
                .Include(x=> x.Messages)
                .FirstOrDefault(x => x.Id == id);
            return View(chat);
        }
     
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int ChatId, string message)
        {
            var msg = new Message
            {
                ChatId = ChatId,
                Text = message,
                Name = User.Identity.Name,
                Timestamp = DateTime.Now
            };
            ctx.Messages.Add(msg);
            await ctx.SaveChangesAsync();
            return RedirectToAction("Chat", new { id = ChatId });
        }
        public IActionResult Index()
        {
            var chats = ctx.Chats
                .Include(x=>x.Users)
                .Where(x=> !x.Users
                .Any(y=>y.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value))
                .ToList();
            return View(chats);
        }
        public IActionResult Find()
        {
            var users = ctx.Users
                .Where(x => x.Id != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                .ToList();
            return View(users);
        }
    
        public async Task<IActionResult> CreatePrivateRoom(string userId)
        {

            var chat = new Chat
            {
                Type = ChatType.Private
            };
            chat.Users.Add(new ChatUser
            {
                UserId = userId
            });
            chat.Users.Add(new ChatUser
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value
            });
            ctx.Chats.Add(chat);
            await ctx.SaveChangesAsync();
            return RedirectToAction("Chat",new { id=chat.Id});
        }
        public IActionResult Private()
        {
            var chat = ctx.Chats
                .Include(x => x.Users)
                .ThenInclude(x=>x.user)
                .Where(x => x.Type == ChatType.Private && x.Users
                .Any(y => y.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value))
                .ToList();
            return View(chat);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public async Task<IActionResult> JoinRoom(int id)
        {
            var chatUser =  new ChatUser
            {
                ChatId = id,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                Role = UserRole.Member
            };
            ctx.Add(chatUser);
            await ctx.SaveChangesAsync();
            return RedirectToAction("Chat","Home", new { id = id});
        }
    }
}
