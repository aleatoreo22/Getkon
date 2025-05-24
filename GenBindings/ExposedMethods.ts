// ExposedMethods.ts - gerado automaticamente
export async function SendMessageThroughCSharp(message: string): Promise<string> {
  return await callNative("Getkon.ExposedMethods", "SendMessageThroughCSharp", { message });
}

export async function TesteInt(): Promise<number> {
  return await callNative("Getkon.ExposedMethods", "TesteInt", {  });
}

