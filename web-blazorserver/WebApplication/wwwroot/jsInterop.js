class DeferredPromise {
    constructor() {
        this._promise = new Promise((resolve, reject) => {
            this.resolve = resolve;
            this.reject = reject;
        });
        // bind `then` and `catch` to implement the same interface as Promise
        this.then = this._promise.then.bind(this._promise);
        this.catch = this._promise.catch.bind(this._promise);
        this[Symbol.toStringTag] = 'Promise';
    }
}

class CustomError extends Error {
    constructor(message) {
        super(message);
        this.name = "CustomError";
    }
}

let deferredPromises = {};

let createUUID = () => {
    let fakeUUID = Math.floor(Math.random() * 0x100000000);
    return fakeUUID.toString(8);
};

let isDefinedAndroidJavascriptInterface = () => {
    if(typeof Android !== "undefined" && Android !== null) {
        return true;
    }
    return false;
}

let sendMessage = (message) => {
    if(typeof window.external !== "undefined" 
            && typeof window.external.sendMessage !== "undefined"
            && window.external.sendMessage !== null) {
        return window.external.sendMessage(message)
    } else if(typeof window.webkit !== "undefined"
            && typeof window.webkit.messageHandlers !== "undefined"
            && typeof window.webkit.messageHandlers.jsHandler !== "undefined"
            && typeof window.webkit.messageHandlers.jsHandler.postMessage !== "undefined"
            ) {
        return window.webkit.messageHandlers.jsHandler.postMessage(message)
    }
    throw Error("Unknown framework")
}

let sendReceiveCommand = async (message) => {
    let msgId = createUUID();
    let result;
    let promise = new DeferredPromise();

    deferredPromises[msgId] = promise;
    message.id = msgId;
    message = JSON.stringify(message);
    promise.then( v => { result = v.result; });
    sendMessage(message);
    await promise;
    delete deferredPromises[msgId];
    return result;
};

let throwException = async () => {
    throw new CustomError("Throw me");
};

window.javaScriptDoThrow = async () => {
    await throwException();
};

window.applicationDoThrow = async() => {
    if(typeof Android != "undefined" && Android !== null) {
        // TODO
    } else if(typeof window.external !== "undefined" && typeof window.external.sendMessage !== "undefined" && window.external.sendMessage !== null) {
        return await sendReceiveCommand({action: "applicationDoThrow"});
    }
    alert("Not viewing in webview");
    return "-1";
};

window.goLangDoPanic = async () => {
    if(typeof Android != "undefined" && Android !== null) {
        // TODO
    } else if(typeof window.external !== "undefined" && typeof window.external.sendMessage !== "undefined" && window.external.sendMessage !== null) {
        return await sendReceiveCommand({action: "goLangDoPanic"});
    }
    alert("Not viewing in webview");
    return "-1";
};

window.goLangDoThrow = async () => {
    if(typeof Android != "undefined" && Android !== null) {
        // TODO
    } else if(typeof window.external !== "undefined" && typeof window.external.sendMessage !== "undefined" && window.external.sendMessage !== null) {
        return await sendReceiveCommand({action: "goLangDoThrow"});
    }
    alert("Not viewing in webview");
    return "-1";
};

window.add = async (x, y) => {
    try {
        if (isDefinedAndroidJavascriptInterface()) {
            return Android.add(x, y);
        }
        return await sendReceiveCommand({action: "add", x:x, y:y});
    } catch (e) {
        alert(e);
    }
    return 0;
};

window.cosine = async (x) => {
    try {
        if (isDefinedAndroidJavascriptInterface()) {
            return Android.cosine(x); 
        }
        return await sendReceiveCommand({action: "cosine", x:x});
    } catch (e) {
        alert(e);
    }
    return 0;
};

window.sort = async (intArray) => {
    try {
        if (isDefinedAndroidJavascriptInterface()) {
            return JSON.parse(Android.sort(intArray));
        }
        return await sendReceiveCommand({action: "sort", x:intArray});
    } catch (e) {
        alert(e);
        return [];
    }
};

window.golog = async (msg) => {
    try {
        if (isDefinedAndroidJavascriptInterface()) {
            Android.golog(msg);
            return
        }
        await sendReceiveCommand({action: "golog", msg:msg});
    } catch (e) {
        alert(e);
    }
};

window.startCountDown = async () => {
    if(typeof Android !== "undefined" && Android !== null) {
        alert("Not implemented!");
    } else if(typeof window.external !== "undefined" && typeof window.external.sendMessage !== "undefined" && window.external.sendMessage !== null) {
        try {
            await sendReceiveCommand({action: "startCountDown", msg:{}});
            return;
        } catch (e) {
            alert(e);
            return;
        }
    }
    alert("Not viewing in webview");
};

CountDown = (data) => {
    DotNet.invokeMethodAsync('WebApplication', 'CountDown', JSON.stringify(data.message));
};

window.getOrganizationAsJson = async (organizationId) => {
    if(typeof Android !== "undefined" && Android !== null) {
        alert("Not implemented!");
    } else if(typeof window.external !== "undefined" && typeof window.external.sendMessage !== "undefined" && window.external.sendMessage !== null) {
        try {
            return await sendReceiveCommand({action: "getOrganizationAsJson", organizationId: organizationId});
        } catch (e) {
            alert(e);
            return;
        }
    }
    alert("Not viewing in webview");
};

window.getOrganizationAsCtype = async (organizationId) => {
    if(typeof Android !== "undefined" && Android !== null) {
        alert("Not implemented!");
    } else if(typeof window.external !== "undefined" && typeof window.external.sendMessage !== "undefined" && window.external.sendMessage !== null) {
        try {
            return await sendReceiveCommand({action: "getOrganizationAsCtype", organizationId: organizationId});
        } catch (e) {
            alert(e);
            return;
        }
    }
    alert("Not viewing in webview");
};

let invokeCommand = (message) => {
    switch(message.method) {
        case "CountDown":
            CountDown(message.data);
    }
}

/**
 * The response receiver
 * 
 * @param messageJson  a json structure
 *
 *  {
 *    id: <id>
 *    action": <action type>    // invokeCommand, response
 *    data: <a data structure>  // an arbitrary data structure
 *    error: {                  // ok if error == null
 *      stacktrace: <string>
 *      message: <string>
 *      type: <type of exception>
 *      innerexception: <nested exception>
 *    }
 *  }
 */
let receiveMessage = (messageJson) => {
    let message = JSON.parse(messageJson);

    // Action invokeCommand
    if (message.action === "invokeCommand") {
        invokeCommand(message)
        return;
    }

    // Action response
    let promise = deferredPromises[message.id];
    if (promise) {
        if (message.error == null) {
            promise.resolve(message.data);
            return;
        }
        try {
            promise.reject(JSON.stringify(message.error));
        } catch (e) {
            alert(`Unhandled error: ${e}`);
        }
    }
};

if (typeof window.external !== 'undefined') {
    window.external.receiveMessage(receiveMessage)
}
