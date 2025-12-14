using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace TheNerdCollective.Helpers;

/// <summary>
/// CSV helper methods for reading and writing CSV data.
/// </summary>
public abstract class CsvHelpers
{
    private const string DataFolder = "Data";

    /// <summary>
    /// Loads CSV records from a MemoryStream.
    /// </summary>
    public static IEnumerable<T> LoadCsvMemoryStream<T>(MemoryStream? csvStream, string delimiter = ";")
    {
        if (csvStream == null)
            return [];

        csvStream.Position = 0;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter
        };

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, config);
        csv.Read();
        csv.ReadHeader();

        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// Loads CSV records from a byte array.
    /// </summary>
    public static IEnumerable<T> LoadCsvBytes<T>(byte[]? csvBytes, string delimiter = ";")
    {
        if (csvBytes == null) return [];

        var stream = StreamByteHelpers.ByteArrayToMemoryStream(csvBytes);
        var records = LoadCsvMemoryStream<T>(stream, delimiter);
        stream.Dispose();

        return records;
    }

    /// <summary>
    /// Loads CSV records from a string.
    /// </summary>
    public static IEnumerable<T> LoadCsvString<T>(string csvString, string delimiter = ";")
    {
        if (string.IsNullOrWhiteSpace(csvString)) return new List<T>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter
        };

        using var reader = new StringReader(csvString);
        using var csv = new CsvReader(reader, config);
        csv.Read();
        csv.ReadHeader();

        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// Loads CSV records from a file.
    /// </summary>
    public static IEnumerable<T> LoadCsvFile<T>(string csvFile, string delimiter = ";")
    {
        if (string.IsNullOrWhiteSpace(csvFile) || !File.Exists(csvFile)) 
            return new List<T>();

        using var reader = new StreamReader(csvFile);
        using var csv = new CsvReader(reader, new CsvConfiguration(cultureInfo: CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter
        });

        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// Writes CSV records to a file.
    /// </summary>
    public static void ToCsvFile<T>(IEnumerable<T> records, string csvFileName)
    {
        var fileWithPath = Path.Combine(DataFolder, csvFileName);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";"
        };

        using var writer = new StreamWriter(fileWithPath);
        using var csv = new CsvWriter(writer, config);
        csv.WriteRecords(records);
        csv.Flush();
        writer.Flush();
    }

    /// <summary>
    /// Converts records to a CSV string.
    /// </summary>
    public static string ToCsvString<T>(IEnumerable<T> records, string delimiter = ";")
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, new CsvConfiguration(cultureInfo: CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter
        });

        csv.WriteRecords(records);

        return writer.ToString();
    }

    /// <summary>
    /// Converts records to a CSV MemoryStream.
    /// </summary>
    public static MemoryStream ToCsvStream<T>(IEnumerable<T> records)
    {
        var str = new MemoryStream();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";"
        };

        using (var stream = new MemoryStream())
        using (var writer = new StreamWriter(stream))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteRecords(records);
            csv.Flush();
            writer.Flush();
            stream.Position = 0;
            stream.CopyTo(str);
        }

        str.Position = 0;
        return str;
    }

    /// <summary>
    /// Converts records to a CSV byte array.
    /// </summary>
    public static byte[] ToCsvByteArray<T>(IEnumerable<T> records)
    {
        var stream = ToCsvStream(records);
        stream.Position = 0;
        var bytes = StreamByteHelpers.MemoryStreamToByteArray(stream);
        stream.Dispose();

        return bytes;
    }
}
