let callId = 0;
const pendingCalls = {};

export function callNative(Namespace, Method, Paramters) {
  return new Promise((resolve, reject) => {
    const Id = ++callId;
    pendingCalls[Id] = resolve;

    const message = JSON.stringify({ Id, Method, Paramters, Namespace });
    window.webkit.messageHandlers.native.postMessage(message);
  });
}

window.__resolveNativeCall = (id, result) => {
  if (pendingCalls[id]) {
    pendingCalls[id](result);
    delete pendingCalls[id];
  }
};
