self.addEventListener('notificationclick', function (event) {
    if (!event.action) {
        // Was a normal notification click
        console.log('Notification Click.');
        return;
    }

    const data = event.notification.data;

    switch (event.action) {
        case 'open-window':
            console.log('Opening window.');
            const promiseChain = clients.openWindow(data.url);
            event.waitUntil(promiseChain);
            break;
        default:
            console.log(`Unknown action clicked: '${event.action}'`);
            break;
    }
});