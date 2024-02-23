using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using REALLY9.Models;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace REALLY9.Hubs
{
    public class ChatHub : Hub
    {

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
