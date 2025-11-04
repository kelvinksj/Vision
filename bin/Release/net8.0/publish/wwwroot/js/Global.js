window.blazor_setExitCheck = function (dotNetHelper, set) {
    if (set) {
        window.addEventListener("beforeunload", blazor_spaExit);
        blazorDotNetExitHelper = dotNetHelper;
    } else {
        window.removeEventListener("beforeunload", blazor_spaExit);
        blazorDotNetExitHelper = null;
    }
};

var blazorDotNetExitHelper;

window.blazor_spaExit = function (event) {
    event.preventDefault();
    blazorDotNetExitHelper.invokeMethodAsync("SpaExit");
};

async function checkServerStatus() {
    try {
        const response = await fetch('/api/keepalive');
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        console.log('Server is alive');
    } catch (error) {
        console.error('Failed to connect to server:', error);
    }
    setTimeout(checkServerStatus, 5000); // Retry after 5 seconds
};

// Start checking server status
//checkServerStatus();

window.connectionStatus = {
    initialize: function (dotNetObject) {
        function updateStatus() {
            const isOnline = navigator.onLine;
            console.log('Blazor javascript connection check triggered');
            console.error('Blazor javascript connection check triggered');
        }

        window.addEventListener('online', updateStatus);
        window.addEventListener('offline', updateStatus);

        updateStatus();
    }
};

// Auto-trigger initialize when the page loads
//document.addEventListener('DOMContentLoaded', () => {
//    console.log('Blazor javascript connection check triggered');
//    const dotNetObject = DotNet.invokeMethodAsync('TaskManagerWeb', 'GetDotNetObjectReference');
//    window.connectionStatus.initialize(dotNetObject);
//});

window.getClientIpAddress = async () => {
    const response = await fetch('https://api.ipify.org?format=json');
    const data = await response.json();
    return data.ip;
};

export function getFormattedDateTime() {
    const date = new Date();

    const yy = String(date.getFullYear()).slice(-2);
    const mm = String(date.getMonth() + 1).padStart(2, '0'); // Months are zero-based
    const dd = String(date.getDate()).padStart(2, '0');
    const hh = String(date.getHours()).padStart(2, '0');
    const min = String(date.getMinutes()).padStart(2, '0');
    const ss = String(date.getSeconds()).padStart(2, '0');

    return `${yy}${mm}${dd}${hh}${min}${ss}`;
}

window.dotnetconsole = async (message) => {
    DotNet.invokeMethodAsync(
        'TaskManagerWeb',
        'MessageFromJS',
        message
    );
};