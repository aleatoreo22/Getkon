interface WebkitMessageHandlers {
  native: {
    postMessage: (message: any) => void;
  };
}

interface MyWindow extends Window {
  __resolveNativeCall: (id: string, result: any) => void;
  webkit: {
    messageHandlers: WebkitMessageHandlers;
  };
}

declare let window: MyWindow;

const pendingCalls: Record<string, (result: any) => void> = {};

export function callNative(
  Namespace: string,
  Method: string,
  Paramters: any
): any {
  return new Promise((resolve, _) => {
    const Id = crypto.randomUUID();
    pendingCalls[Id] = resolve;

    const message = JSON.stringify({ Id, Method, Paramters, Namespace });
    window.webkit.messageHandlers.native.postMessage(message);
  });
}

window.__resolveNativeCall = (id: string, result: any) => {
  if (pendingCalls[id]) {
    pendingCalls[id](result);
    delete pendingCalls[id];
  }
};
