// Teste.ts - gerado automaticamente
export async function SendMessageThroughCSharp(message: string): Promise<string> {
  return await callNative("Getkon.Teste", "SendMessageThroughCSharp", { message });
}

export async function TesteInt(): Promise<number> {
  return await callNative("Getkon.Teste", "TesteInt", {  });
}

