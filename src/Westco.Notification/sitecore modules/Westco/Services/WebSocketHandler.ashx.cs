using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sitecore.Web.Authentication;

namespace Westco.Notification.sitecore_modules.Westco.Services
{
    public class QueuedMessage
    {
        public string Username { get; set; }
        public byte[] Data { get; set; }
    }
    public class WebSocketHandler : IHttpHandler
    {
        private static bool _isInitialized;
        private static readonly ConcurrentDictionary<string, WebSocket> Subscriptions = new ConcurrentDictionary<string, WebSocket>();
        private static readonly ConcurrentBag<QueuedMessage> Messages = new ConcurrentBag<QueuedMessage>();

        public WebSocketHandler()
        {
            if (_isInitialized) return;

            SubscribeUser();
            NotifyUser();
            UnsubscribeUser();
            _isInitialized = true;
        }

        private static async Task ProcessMessages()
        {
            await Task.Delay(1000);
            var localMessages = Messages;
            foreach (var localMessage in localMessages)
            {
                var username = localMessage.Username;
                if (!Subscriptions.ContainsKey(username) || Subscriptions[username] == null) continue;

                if (!Messages.TryTake(out var message)) continue;

                var socket = Subscriptions[username];
                var cancellationToken = new CancellationToken();
                var bytes = message.Data;
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
                    cancellationToken);
            }
        }

        public void SubscribeUser()
        {
            var handler = new EventHandler((sender, args) =>
            {
                if (Sitecore.Events.Event.ExtractParameter(args, 0) is SubscriptionEventArgs subscriptionEventArgs)
                {
                    Subscriptions.TryAdd(subscriptionEventArgs.Username, null);
                }
            });

            Sitecore.Events.Event.Subscribe("westcosocket:subscribe", handler);
        }

        public void UnsubscribeUser()
        {
            var handler = new EventHandler((sender, args) =>
            {
                if (Sitecore.Events.Event.ExtractParameter(args, 0) is SubscriptionEventArgs subscriptionEventArgs)
                {
                    Subscriptions.TryRemove(subscriptionEventArgs.Username, out _);
                    //TODO: Should remove old messages?
                }
            });

            Sitecore.Events.Event.Subscribe("westcosocket:unsubscribe", handler);
        }

        public void NotifyUser()
        {
            var handler = new EventHandler((sender, args) =>
            {
                var message = Sitecore.Events.Event.ExtractParameter(args, 0) as dynamic;

                var canBroadcast = message.CanBroadcast;
                var targetUsername = message.Username;

                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                };

                foreach (var username in Subscriptions.Keys)
                {
                    if (!canBroadcast && targetUsername != username) continue;

                    var cancellationToken = new CancellationToken();

                    var socket = Subscriptions[username];

                    var dataString = JsonConvert.SerializeObject(message.Payload, serializerSettings);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(dataString);

                    if (socket != null)
                    {
                        socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true,
                           cancellationToken);
                    }
                    else
                    {
                        Messages.Add(new QueuedMessage()
                        {
                            Username = username,
                            Data = bytes
                        });
                    }
                }
            });

            Sitecore.Events.Event.Subscribe("westcosocket:notify", handler);
        }

        public void ProcessRequest(HttpContext context)
        {
            //Checks if the query is WebSocket request. 
            if (context.IsWebSocketRequest)
            {
                //If yes, we attach the asynchronous handler.
                context.AcceptWebSocketRequest(WebSocketRequestHandler);
            }
        }

        public bool IsReusable { get { return false; } }

        //Asynchronous request handler.
        public async Task WebSocketRequestHandler(AspNetWebSocketContext webSocketContext)
        {
            var webSocket = webSocketContext.WebSocket;
            const int maxMessageSize = 1024;

            var receivedDataBuffer = new ArraySegment<byte>(new byte[maxMessageSize]);

            var cancellationToken = new CancellationToken();

            //Checks WebSocket state.
            while (webSocket.State == WebSocketState.Open)
            {
                var username = "";
                var sessionId = webSocketContext.CookieCollection["ASP.NET_SessionId"]?.Value;
                if (!string.IsNullOrEmpty(sessionId))
                {
                    var session = DomainAccessGuard.Sessions.FirstOrDefault(s => s.SessionID == sessionId);
                    if (session != null)
                    {
                        username = session.UserName;
                        Subscriptions[username] = webSocket;
                        await ProcessMessages();
                    }
                }

                var webSocketReceiveResult =
                  await webSocket.ReceiveAsync(receivedDataBuffer, cancellationToken);

                //If input frame is cancelation frame, send close command.
                if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
                {
                    if (!string.IsNullOrEmpty(username))
                    {
                        Subscriptions[username] = null;
                    }

                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                      string.Empty, cancellationToken);
                }
                else
                {
                    await ProcessMessages();
                }
            }
        }
    }
}