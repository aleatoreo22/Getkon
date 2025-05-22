const namespace = "Getkon.ExposedMethods";
import { callNative } from "../interop/interop";
export async function sendMessageThroughCSharp(
  message: string
): Promise<string> {
  return await callNative(namespace, "SendMessageThroughCSharp", {
    message,
  });
}
