using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSAChat_UI.Server
{
    public class ChatHub : Hub
    {
        private static readonly Dictionary<string, string> connectionPublicKey = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> connectionUsername = new Dictionary<string, string>();

        public async Task SendMessage(string recipient, byte[] message)
        {
            var username = connectionUsername[Context.ConnectionId];

            var isPM = recipient != null;

            if (isPM == false)
            {
                await Clients.All.SendAsync("ReceiveMessage", username, message, isPM);
            }
            else
            {
                var recipientId = connectionUsername.First(i => i.Value == recipient).Key;
                await Clients.Clients(recipientId, Context.ConnectionId).SendAsync("ReceiveMessage", username, message, true);
            }
        }

        public async Task RegisterUser(string username, string publicKey)
        {
            connectionUsername.Add(Context.ConnectionId, username);
            connectionPublicKey.Add(Context.ConnectionId, publicKey);
            await Clients.All.SendAsync("NewConnection", connectionUsername[Context.ConnectionId], publicKey);
        }

        public override async Task OnConnectedAsync()
        {
            var usernamesKeys = connectionPublicKey.ToDictionary(i => connectionUsername[i.Key], i => i.Value);

            await Clients.Client(Context.ConnectionId).SendAsync("GetParticipants", usernamesKeys);
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
