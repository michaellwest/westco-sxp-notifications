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
        private static bool IsInitialized = false;
        private static readonly ConcurrentDictionary<string, WebSocket> Sockets = new ConcurrentDictionary<string, WebSocket>();
        private static readonly ConcurrentDictionary<string, string> SubscribedUsers = new ConcurrentDictionary<string, string>();

        public WebSocketHandler()
        {
            if (!IsInitialized)
            {
                SubscribeUser();
                NotifyUser();
                IsInitialized = true;
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

                    //Sends data back.
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
            //Gets the current WebSocket object.
            var webSocket = webSocketContext.WebSocket;

            /*We define a certain constant which will represent
            size of received data. It is established by us and 
            we can set any value. We know that in this case the size of the sent
            data is very small.
            */
            const int maxMessageSize = 1024;

            //Buffer for received bits.
            var receivedDataBuffer = new ArraySegment<byte>(new byte[maxMessageSize]);

            var cancellationToken = new CancellationToken();
            var socketId = Guid.Empty;
            var sessionId = "";

            //Checks WebSocket state.
            while (webSocket.State == WebSocketState.Open)
            {
                //Reads data.
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
                    /*
                    var payloadData = receivedDataBuffer.Array.Where(b => b != 0).ToArray();

                    //Because we know that is a string, we convert it.
                    var receiveString = System.Text.Encoding.UTF8.GetString(payloadData, 0, payloadData.Length);
                    */
                    sessionId = webSocketContext.CookieCollection["ASP.NET_SessionId"]?.Value;
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        Sockets.TryAdd(sessionId, webSocket);
                        
                        /*
                        //Converts string to byte array.
                        var newString = $"Registered at {DateTime.Now}";
                        var bytes = System.Text.Encoding.UTF8.GetBytes(newString);

                        //Sends data back.
                        await webSocket.SendAsync(new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text, true, cancellationToken);
                        */
                    }
                    else
                    {
                        /*
                        //Converts string to byte array.
                        var newString = $"Hello, {receiveString}! Time {DateTime.Now}";
                        var bytes = System.Text.Encoding.UTF8.GetBytes(newString);

                        //Sends data back.
                        await webSocket.SendAsync(new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text, true, cancellationToken);
                        */
                    }
                }
            }

            Sockets.TryRemove(sessionId, out _);
        }
    }
}