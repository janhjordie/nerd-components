using System.Text;

namespace TheNerdCollective.Helpers;

/// <summary>
/// Stream and byte array conversion helper methods.
/// </summary>
public abstract class StreamByteHelpers
{
    /// <summary>
    /// Converts string to Stream.
    /// </summary>
    public static Stream StringToStream(string input)
    {
        var byteArray = Encoding.UTF8.GetBytes(input);
        return new MemoryStream(byteArray);
    }

    /// <summary>
    /// Converts Stream to string.
    /// </summary>
    public static string StreamToString(Stream input)
    {
        using var reader = new StreamReader(input, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Converts Stream to byte array.
    /// </summary>
    public static byte[] StreamToByteArray(Stream input)
    {
        using var memoryStream = new MemoryStream();
        input.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Converts byte array to Stream.
    /// </summary>
    public static Stream ByteArrayToStream(byte[] input)
    {
        return new MemoryStream(input);
    }

    /// <summary>
    /// Converts string to byte array.
    /// </summary>
    public static byte[] StringToByteArray(string input)
    {
        return Encoding.UTF8.GetBytes(input);
    }

    /// <summary>
    /// Converts byte array to string.
    /// </summary>
    public static string ByteArrayToString(byte[] input)
    {
        return Encoding.UTF8.GetString(input);
    }

    /// <summary>
    /// Converts string to MemoryStream.
    /// </summary>
    public static MemoryStream StringToMemoryStream(string input)
    {
        var byteArray = Encoding.UTF8.GetBytes(input);
        return new MemoryStream(byteArray);
    }

    /// <summary>
    /// Converts MemoryStream to string.
    /// </summary>
    public static string MemoryStreamToString(MemoryStream input)
    {
        input.Position = 0;
        using var reader = new StreamReader(input, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Converts MemoryStream to byte array.
    /// </summary>
    public static byte[] MemoryStreamToByteArray(MemoryStream input)
    {
        return input.ToArray();
    }

    /// <summary>
    /// Converts byte array to MemoryStream.
    /// </summary>
    public static MemoryStream ByteArrayToMemoryStream(byte[] byteArray)
    {
        return new MemoryStream(byteArray);
    }
}
