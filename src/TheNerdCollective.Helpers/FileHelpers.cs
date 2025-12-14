using System.Text;

namespace TheNerdCollective.Helpers;

/// <summary>
/// File I/O helper methods for reading and writing files.
/// </summary>
public abstract class FileHelpers
{
    private const string DataFolder = "Data";

    /// <summary>
    /// Reads a file to a MemoryStream.
    /// </summary>
    public static MemoryStream? ReadFileToMemoryStream(string filePath)
    {
        var fileWithPath = Path.Combine(DataFolder, filePath);

        if (!File.Exists(fileWithPath)) return null;

        using var fileStream = new FileStream(fileWithPath, FileMode.Open, FileAccess.Read);
        var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }

    /// <summary>
    /// Reads file information.
    /// </summary>
    public static FileInfo? ReadFileInfo(string filePath)
    {
        var fileWithPath = Path.Combine(DataFolder, filePath);

        if (!File.Exists(fileWithPath)) return null;

        return new FileInfo(fileWithPath);
    }

    /// <summary>
    /// Reads a file to a byte array.
    /// </summary>
    public static byte[]? ReadFileToByteArray(string filePath)
    {
        var fileWithPath = Path.Combine(DataFolder, filePath);

        if (!File.Exists(fileWithPath)) return null;

        return File.ReadAllBytes(fileWithPath);
    }

    /// <summary>
    /// Reads a file to a string with optional encoding.
    /// </summary>
    public static string? ReadFileToString(string filePath, Encoding? encoding = null)
    {
        var fileWithPath = Path.Combine(DataFolder, filePath);

        if (!File.Exists(fileWithPath)) return null;

        encoding = encoding ?? Encoding.UTF8;
        using var reader = new StreamReader(fileWithPath, encoding);

        return reader.ReadToEnd();
    }

    /// <summary>
    /// Saves a MemoryStream to a file.
    /// </summary>
    public static void SaveMemoryStreamToFile(MemoryStream memoryStream, string filePath)
    {
        Directory.CreateDirectory(DataFolder);
        var fileWithPath = Path.Combine(DataFolder, filePath);

        using var fileStream = new FileStream(fileWithPath, FileMode.Create, FileAccess.Write);
        memoryStream.WriteTo(fileStream);
        fileStream.Flush();
    }

    /// <summary>
    /// Saves a byte array to a file.
    /// </summary>
    public static void SaveByteArrayToFile(byte[] byteArray, string filePath)
    {
        Directory.CreateDirectory(DataFolder);
        var fileWithPath = Path.Combine(DataFolder, filePath);

        File.WriteAllBytes(fileWithPath, byteArray);
    }

    /// <summary>
    /// Saves a string to a file with optional encoding.
    /// </summary>
    public static void SaveStringToFile(string content, string filePath, Encoding? encoding = null)
    {
        Directory.CreateDirectory(DataFolder);
        var fileWithPath = Path.Combine(DataFolder, filePath);

        encoding = encoding ?? Encoding.UTF8;

        File.WriteAllText(fileWithPath, content, encoding);
    }
}
