(function () {
    function registerServiceWorker() {
        return navigator.serviceWorker.register("/sitecore modules/westco/scripts/service-worker.js")
            .then(function (registration) {
                console.log("[Westco] Service worker successfully registered.");
                registration.update();
                return registration;
            })
            .catch(function (err) {
                console.error("[Westco] Unable to register service worker.", err);
            });
    }

    function askPermission() {
        return new Promise(function (resolve, reject) {
            const permissionResult = Notification.requestPermission(function (result) {
                resolve(result);
            });

            if (permissionResult) {
                permissionResult.then(resolve, reject);
            }
        })
            .then(function (permissionResult) {
                if (permissionResult !== "granted") {
                    throw new Error("We weren't granted permission.");
                }
            });
    }

    var webSocket;

    //The address of our HTTP-handler
    var handlerUrl = "wss://" + window.location.hostname + "/sitecore modules/westco/services/WebSocketHandler.ashx";
    
    function guid() {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
                .toString(16)
                .substring(1);
        }
        return s4() + s4() + "-" + s4() + "-" + s4() + "-" + s4() + "-" + s4() + s4() + s4();
    }

    var clientId = guid();

    function sendData() {

        //Initialize WebSocket.
        initWebSocket();

        //Send data if WebSocket is opened.
        if (webSocket.OPEN && webSocket.readyState === 1)
            webSocket.send(clientId);

        //If WebSocket is closed, show message.
        if (webSocket.readyState === 2 || webSocket.readyState === 3)
            console.log("[Westco] WebSocket closed, the data can't be sent.");
    }

    function initWebSocket() {

        if (webSocket == undefined) {
            webSocket = new WebSocket(handlerUrl);

            askPermission().then(function () {
                registerServiceWorker().then(function (registration) {

                    //Open connection  handler.
                    webSocket.onopen = function () {
                        console.log("[Westco] WebSocket opened.");
                        webSocket.send(clientId);
                    };

                    //Message data handler.
                    webSocket.onmessage = function (e) {
                        const message = JSON.parse(e.data);
                        console.log(`[Westco] ${message}`);
                        registration.showNotification(message.title, message.options);
                    };

                    //Close event handler.
                    webSocket.onclose = function () {
                        console.log("[Westco] WebSocket closed.");
                    };

                    //Error event handler.
                    webSocket.onerror = function (e) {
                        console.log(`[Westco] ${e.message}`);
                    }

                    if (webSocket.OPEN && webSocket.readyState === 1)
                        webSocket.send(clientId);

                    console.log("[Westco] Completing registration.");
                });
            });
        }
    }

    sendData();
})();