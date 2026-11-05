#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

internal static class Program
{
    public static void Main(string[] args)
    {
        string fileName = args.Length > 0 ? args[0] : "sample";
        if (args.Length < 2 || !int.TryParse(args[1], out int n))
        {
            n = 10;
        }
        var json = File.ReadAllBytes($"{fileName}.json").AsSpan();
        GeoData data = JsonSerializer.Deserialize(json, MyJsonContext.Default.GeoData);
        PrintHash(data);
        GeoData[] array = new GeoData[n];
        for (int i = 0; i < n; i++)
        {
            array[i] = JsonSerializer.Deserialize(json, MyJsonContext.Default.GeoData);
        }
        PrintHash(array);
    }
    private static void PrintHash<T>(T value)
    {
        using var md5 = MD5.Create();
        using (var cs = new CryptoStream(Stream.Null, md5, CryptoStreamMode.Write))
            JsonSerializer.Serialize(cs, value, typeof(T), MyJsonContext.Default);
        Console.WriteLine(Convert.ToHexStringLower(md5.Hash));
    }
}
internal sealed class GeoData
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("features")]
    public List<Feature> Features { get; set; }
}

internal sealed class Feature
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("properties")]
    public Properties Properties { get; set; }

    [JsonPropertyName("geometry")]
    public Geometry Geometry { get; set; }
}

internal sealed class Properties
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

internal sealed class Geometry
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("coordinates")]
    public List<List<Coord>> Coordinates { get; set; }
}

[JsonSerializable(typeof(GeoData))]
[JsonSerializable(typeof(GeoData[]))]
[JsonSourceGenerationOptions(DefaultBufferSize = 1 << 10)]
internal sealed partial class MyJsonContext : JsonSerializerContext { }

[JsonConverter(typeof(CoordConverter))]
internal readonly record struct Coord(double X, double Y);

internal sealed class CoordConverter : JsonConverter<Coord>
{
    public override Coord Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
    {
        r.Read(); double x = r.GetDouble();
        r.Read(); double y = r.GetDouble();
        r.Read(); // EndArray
        return new Coord(x, y);
    }
    public override void Write(Utf8JsonWriter w, Coord c, JsonSerializerOptions o)
    {
        w.WriteStartArray(); w.WriteNumberValue(c.X); w.WriteNumberValue(c.Y); w.WriteEndArray();
    }
}
