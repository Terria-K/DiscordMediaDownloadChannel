#!/usr/bin/env dotnet 

#:package System.CommandLine@2.0.1

using System.Text.Json;
using System.CommandLine;
using System.Text.Json.Serialization;

var jsonOption = new Option<string?>("--json", "--input", "-i");
var outputDirOption = new Option<string?>("--output", "-o");

var rootCommand = new RootCommand("Fetch Images")
{
    TreatUnmatchedTokensAsErrors = false
};

rootCommand.Add(jsonOption);

var result = rootCommand.Parse(args);

if (result.Errors.Count > 0)
{
    foreach (var error in result.Errors)
    {
        Console.Error.WriteLine(error.Message);
    }

    return;
}

var json = result.GetValue(jsonOption);
var output = result.GetValue(outputDirOption);

if (output is null)
{
    output = "Result";
}

if (json is null)
{
    Console.WriteLine("No '--input' provided.");
    return;
}

if (!Directory.Exists(output))
{
    Directory.CreateDirectory(output);
}

var read = File.ReadAllText(json);

var urls = JsonSerializer.Deserialize(read, SourceGenerationContext.Default.StringArray);

if (urls is not null) 
{
    var succeed = 0;
    var failed = 0;

    var total = urls.Length;
    await Parallel.ForEachAsync(urls, async (url, token) =>
    {
        var cutUrl = url.Replace("https://cdn.discordapp.com/attachments/", string.Empty);
        var index = cutUrl.IndexOf('/');
        var str = cutUrl[(index + 1)..];
        index = str.IndexOf('?');
        str = str[0..index];


        var str2 = str.Replace("/", "-");
        if (File.Exists($"Result/{str2}")) 
        {
            var current = Interlocked.Increment(ref succeed);
            Console.WriteLine($"{str2} is already done! {current}/{total}");
            return;
        }
        using var client = new HttpClient();
        using var res = await client.GetAsync(url, token);

        res.EnsureSuccessStatusCode();
        await using var inputStream = await res.Content.ReadAsStreamAsync(token);

        try 
        {
            MemoryStream stream = new MemoryStream();
            inputStream.CopyTo(stream);
            stream.Position = 0;

            await Write(new ImageData(stream, str2));
            var current = Interlocked.Increment(ref succeed);
            Console.WriteLine($"{str2} is done! {current}/{total}");
        }
        catch (Exception e) 
        {
            Interlocked.Increment(ref failed);
            Console.Error.WriteLine(e.ToString());
            Console.Error.WriteLine($"{str2} failed! {succeed}/{total}");
        }
    });

    Console.WriteLine($"{succeed}/{total} Succeed.");
    Console.WriteLine($"{failed}/{total} Failed.");
}

async Task Write(ImageData data)
{
    using var fs = new FileStream(Path.Combine(output, data.Name), FileMode.Create, FileAccess.Write);

    var stream = data.Image;
    stream.Position = 0;
    await stream.CopyToAsync(fs);
}

public readonly struct ImageData(MemoryStream image, string id)
{
    public readonly string Name = id;
    public readonly MemoryStream Image = image;
}


[JsonSerializable(typeof(string[]))]
internal partial class SourceGenerationContext : JsonSerializerContext {}