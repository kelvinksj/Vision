import { getFormattedDateTime } from '/js/Global.js';

window.workerManager = {
    workers: {},
    startWorker: function () {
        if (typeof (Worker) !== "undefined") {
            try {
                const workerNo = getFormattedDateTime();
                const worker = new Worker('js/Worker.js');
                worker.onmessage = function (event) {
                    // DotNet.invokeMethodAsync(
                    //     'TaskManagerWeb',
                    //     'MessageFromJS',
                    //     event.data
                    // );
                };
                worker.postMessage({ command: 'start', workerNo: workerNo });
                this.workers[workerNo] = worker;
                DotNet.invokeMethodAsync(
                    'TaskManagerWeb',
                    'MessageFromJS',
                    `Worker ${workerNo} started`
                );

                return workerNo;
            }
            catch (ex) {
                DotNet.invokeMethodAsync(
                    'TaskManagerWeb',
                    'MessageFromJS',
                    `workerManager-startWorker err:[${ex.name}] ${ex.message}`
                );

                return "";
            }
        } else {
            DotNet.invokeMethodAsync(
                'TaskManagerWeb',
                'MessageFromJS',
                'Web Workers are not supported in this browser'
            );

            return "";
        }
    },
    stopWorker: function (workerNo) {
        DotNet.invokeMethodAsync(
            'TaskManagerWeb',
            'MessageFromJS',
            `stopWorker=${workerNo}`
        );
        try {
            if (this.workers[workerNo]) {
                this.workers[workerNo].postMessage({ command: 'stop', workerNo: workerNo });
                setTimeout(() => {
                    this.workers[workerNo].terminate();
                    delete this.workers[workerNo];
                    DotNet.invokeMethodAsync(
                        'TaskManagerWeb',
                        'MessageFromJS',
                        `Worker ${workerNo} terminated`
                    );
                }, 100);
            }
        }
        catch (ex) {
            DotNet.invokeMethodAsync(
                'TaskManagerWeb',
                'MessageFromJS',
                `workerManager-stopWorker err:[${ex.name}] ${ex.message}`
            );
        }
    }
};