using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace chatServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        public static Dictionary<string, string> users = new Dictionary<string, string>();
        public static List<Message> messages = new List<Message>();
        public static Dictionary<string, string> connectedUsers = new Dictionary<string, string>();


        [HttpPost("~/SignIn")]
        public void SignIn(string id, string name)
        {
            users.Add(id, name);
            connectedUsers.Add(id, name);
        }


        [HttpPost("~/Logout")]
        public void Logout(string id)
        {
            if (connectedUsers.ContainsKey(id))
            {
                connectedUsers.Remove(id);
            }
        }

        [HttpGet("~/GetUserList")]
        public List<User> GetUserList()
        {
            List<User> usersList=new List<User>();
            foreach (var u in users)
            {
                User use=new User();
                use.UserId = u.Key;
                use.UserName = u.Value;
                usersList.Add(use); 
            }
           return usersList;
        }

        public static void AddMessageToUser(string receiverId, string sender, string text)
        {
            messages.Add(new Message() { ReceiverId = receiverId, Sender = sender, Text = text });
        }

        private static List<Message> GetMessagesForUser(string userId)
        {
            List<Message> messagesForUser = new List<Message>();
            foreach (var m in messages)
            {
                if (m.ReceiverId == userId)
                {
                    messagesForUser.Add(m);
                }
            }
            return messagesForUser;
        }

        [HubName("chat")]
        public class ChatHub : Hub
        {
            //?
            public async Task SendMessage(string sender, string receiverId, string text)
            {
                if (connectedUsers.ContainsKey(receiverId))
                {
                    await Clients.Client(connectedUsers[receiverId]).SendAsync("addMessage", sender, text);
                }
                AddMessageToUser(receiverId, sender, text);
            }

            // POST api/user/login
            [SwaggerResponse((int)HttpStatusCode.OK)]

            public void Login(string userId, string userName)
            {
                if (!(users.ContainsKey(userId)))
                {
                    // TODO: Add authentication logic here
                    // For now, just add the user to the connected users list
                    users.Add(userId, userName);
                }
                if (!connectedUsers.ContainsKey(userId))
                {
                    connectedUsers.Add(userId, Context.ConnectionId);
                }
            }

            public List<Message> GetMessages(string userId)
            {
                return GetMessagesForUser(userId);
            }
        }

    }
    public class User
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
    }

    public class Message
    {
        public string ReceiverId { get; set; }
        public string Sender { get; set; }
        public string Text { get; set; }
    }
}