const pendingCalls = {};

export function callNative(Namespace, Method, Paramters) {
  return new Promise((resolve, _) => {
    const Id = crypto.randomUUID();
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
