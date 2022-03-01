// See https://aka.ms/new-console-template for more information

using Document;
using Prediction;

Console.WriteLine("Hello, World!");
var max = uint.MinValue;
var min = uint.MaxValue;
var sum = (uint)0;
var path = "en_full.txt";
var words = new Dictionary<string, uint>();
await using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
await using (BufferedStream bs = new BufferedStream(fs))
using (StreamReader sr = new StreamReader(bs))
{
    while (sr.ReadLine() is { } line)
    {
        var split = line.Split(' ');
        if (split.Length != 2) continue;
        var word = split[0];
        var count = uint.Parse(split[1]);

        if (count > max) max = count;
        if (count < min) min = count;
        sum += count;
        words[word] = count;
    }
}
Console.WriteLine($"Found: {words.Count} max: {max} min: {min} count: {sum}");


var configuration = new ElasticClientConfiguration
{
    BaseAddress = new Uri("https://172.31.86.23:9200")
};
await using var client = new ElasticClient(configuration);
await client.GetClient();

var i = 0;
var doc = new List<ActionDocumentData<WordData>>();
var d =  SoftMax.WordsPerMillion(words);
foreach (var keys in words.Keys)
{
    var word = new WordData
    {
        Id = i,
        Word = keys,
        Frequency = words[keys],
        NormFrequency = d[keys]
    };
    doc.Add(new ActionDocumentData<WordData>
    {
        DocumentIndex = "words",
        DocumentId = word.Id.ToString(),
        Index = word
    });
    i++;
}


var groups = doc.Split(20);
foreach (var gGroup in groups)
{
    var res = await client.Bulk(gGroup);
    Console.WriteLine($"took: {res.Took} to do {res.Items.Count} Errors: {res.Errors}");
}

Console.WriteLine("Done...");