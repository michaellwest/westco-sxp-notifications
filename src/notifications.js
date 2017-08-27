(function ($) {
    Notification.requestPermission();

    function notifyMe() {
        if (!Notification) {
            alert('Desktop notifications not available in your browser. Try Chromium.');
            return;
        }

        if (Notification.permission !== "granted")
            Notification.requestPermission();
    }

    notifyMe();

    //$(function () {
        var notificationHub = $.connection.loggedInHub;

        notificationHub.client.ShowLoggedInUserInfo = function (data) {
            const notify = JSON.parse(data);
            var notification = new Notification(notify.title, {
                icon: notify.icon,
                body: notify.body
            });
        };

        notificationHub.client.NotifyConnectionTime = function(time) {
            console.log(time);
        };

        $.connection.hub.start().done(function () {
            notificationHub.server.clientBroadcastTime();
        });
    //});
}(jQuery));