window.JsFunctions = {
    logKey: null,

    addKeyboardListenerEvent: function (name, guid) {
        // console.log('addKeyboardListenerEvent triggered');

        try {
            let serializeEvent = function (e) {
                if (e) {
                    return {
                        key: e.key,
                        code: e.keyCode.toString(),
                        location: e.location,
                        repeat: e.repeat,
                        ctrlKey: e.ctrlKey,
                        shiftKey: e.shiftKey,
                        altKey: e.altKey,
                        metaKey: e.metaKey,
                        type: e.type,
                        guid: guid
                    };
                }
            };

            window.JsFunctions.logKey = function (e) {
                // console.log('logKey triggered');

                try {
                    DotNet.invokeMethodAsync('TaskManagerWeb', name, serializeEvent(e));
                }
                catch (ex) {
                    console.log('logKey err: ' + ex.message);
                }
            };

            try {
                // console.log('addEventListener triggered');
                window.document.addEventListener('keydown', window.JsFunctions.logKey);
            }
            catch (ex) {
                console.log('addEventListener err: ' + ex.message);
            }

            // console.log('addKeyboardListenerEvent done');
        }
        catch (ex) {
            console.log('addKeyboardListenerEvent err: ' + ex.message);
        }
    },

    removeKeyboardListernerEvent: function () {
        // console.log('removeKeyboardListernerEvent triggered');

        try {
            window.document.removeEventListener('keydown', window.JsFunctions.logKey);
            window.JsFunctions.logKey = null;
            // console.log('removeKeyboardListernerEvent done');
        }
        catch (ex) {
            console.log('removeKeyboardListernerEvent err: ' + ex.message);
        }
    }
};