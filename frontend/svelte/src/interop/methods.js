let callId = 0;
const pendingCalls = {};

// export async function SendMessageThroughCSharp(
//   message: string
// ): Promise<string> {
//   window.webkit.messageHandlers.native.postMessage("calcular");
//   // var a = await sendMessageAsync("Teste JS");

//   return "";
// }

export function callNative(method, args) {
  return new Promise((resolve, reject) => {
    const id = ++callId;
    pendingCalls[id] = resolve;

    const message = JSON.stringify({ id, method, args });
    window.webkit.messageHandlers.native.postMessage(message);
  });
}

// Esta função será chamada pelo C#
window.__resolveNativeCall = (id, result) => {
  if (pendingCalls[id]) {
    pendingCalls[id](result);
    delete pendingCalls[id];
  }
};
