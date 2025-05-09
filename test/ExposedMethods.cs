using System;

namespace test;

public class ExposedMethods
{
    public string SendMessageThroughCSharp(string message)
    {
        Console.WriteLine(message);
        return message;
    }
}
