window.wakeLock = {
    wakeLockObj: null,
    request: async function () {
        if ('wakeLock' in navigator) {
            try {
                this.wakeLockObj = await navigator.wakeLock.request('screen');
                this.wakeLockObj.addEventListener('release', () => {
                    DotNet.invokeMethodAsync(
                        'TaskManagerWeb',
                        'MessageFromJS',
                        'Screen wake lock released automatically'
                    );
                });
                DotNet.invokeMethodAsync(
                    'TaskManagerWeb',
                    'MessageFromJS',
                    'Screen Wake Lock acquired'
                );
            } catch (ex) {
                DotNet.invokeMethodAsync(
                    'TaskManagerWeb',
                    'MessageFromJS',
                    `wakeLock request err:[${ex.name}] ${ex.message}`
                );
            }
        } else {
            DotNet.invokeMethodAsync(
                'TaskManagerWeb',
                'MessageFromJS',
                'Screen Wake Lock API not supported in this browser'
            );
        }
    },
    release: async function () {
        if (this.wakeLockObj) {
            await this.wakeLockObj.release();
            this.wakeLockObj = null;
            DotNet.invokeMethodAsync(
                'TaskManagerWeb',
                'MessageFromJS',
                'Screen wake lock released manually'
            );
        }
    }
};