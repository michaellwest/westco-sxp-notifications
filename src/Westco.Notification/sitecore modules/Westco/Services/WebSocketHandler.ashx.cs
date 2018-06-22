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

namespace Westco.Notification.sitecore_modules.Westco.Services
{
    public class WebSocketHandler : IHttpHandler
    {
        private static bool _isInitialized;
        private static readonly ConcurrentDictionary<string, WebSocket> Sockets = new ConcurrentDictionary<string, WebSocket>();
        private static readonly ConcurrentDictionary<string, string> SubscribedUsers = new ConcurrentDictionary<string, string>();

        public WebSocketHandler()
        {
            if (!_isInitialized)
            {
                SubscribeUser();
                NotifyUser();
                _isInitialized = true;
            }
        }

        public void SubscribeUser()
        {
            var handler = new EventHandler((sender, args) =>
            {
                if (Sitecore.Events.Event.ExtractParameter(args, 0) is SubscriptionEventArgs subscriptionEventArgs)
                {
                    SubscribedUsers.TryAdd(subscriptionEventArgs.SessionId, subscriptionEventArgs.Username);
                }
            });

            Sitecore.Events.Event.Subscribe("westcosocket:subscribe", handler);
        }

        public void NotifyUser()
        {
            var handler = new EventHandler((sender, args) =>
            {
                var message = Sitecore.Events.Event.ExtractParameter(args, 0) as dynamic;

                var canBroadcast = message.CanBroadcast;
                var targetSessionId = message.SessionId;

                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                };

                foreach (var sessionId in Sockets.Keys)
                {
                    if(!canBroadcast && targetSessionId != sessionId) continue;

                    var cancellationToken = new CancellationToken();

                    var socket = Sockets[sessionId];

                    var dataString = JsonConvert.SerializeObject(message.Payload, serializerSettings);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(dataString);

                    socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
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
            var sessionId = "";

            //Checks WebSocket state.
            while (webSocket.State == WebSocketState.Open)
            {
                var webSocketReceiveResult =
                  await webSocket.ReceiveAsync(receivedDataBuffer, cancellationToken);

                //If input frame is cancelation frame, send close command.
                if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                      string.Empty, cancellationToken);
                }
                else
                {
                    sessionId = webSocketContext.CookieCollection["ASP.NET_SessionId"]?.Value;
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        Sockets.TryAdd(sessionId, webSocket);                 
                    }
                }
            }

            if (!string.IsNullOrEmpty(sessionId))
            {
                Sockets.TryRemove(sessionId, out _);
            }
        }
    }
}