using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealTimeChatApplication.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealTimeChatApplication.ViewCompenents
{
    public class RoomViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext ctx;

        public RoomViewComponent(ApplicationDbContext ctx)
        {
            this.ctx = ctx;
        }
        public IViewComponentResult Invoke()
        {
          
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var chats = ctx.ChatUsers
                .Include(x=>x.Chat)
                .Where(x=> x.UserId == userId && x.Chat.Type == Models.ChatType.Room    )
                .Select(x=>x.Chat)
                .ToList();
            return View(chats);
        }
    }
}
