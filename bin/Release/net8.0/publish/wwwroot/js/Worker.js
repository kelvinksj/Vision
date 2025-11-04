let intervalId;

self.onmessage = function (event) {
    if (event.data.command === 'start') {
        intervalId = setInterval(() => {
            self.postMessage(`Worker ${event.data.workerNo} is running`);
        }, 5000);
    } else if (event.data.command === 'stop') {
        clearInterval(intervalId);
        self.postMessage(`Worker ${event.data.workerNo} has stopped`);
        self.close();
    }
};