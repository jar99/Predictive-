using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Prediction;

public class ElasticClientConfiguration
{
    public Uri BaseAddress { get; init; } = new("https://localhost:9200");
    public string Credentials { get; init; } = "admin:admin";

    public bool DoNotVerifySsl { get; init; } = true;
}

public class ElasticClient : IDisposable, IAsyncDisposable
{
    private readonly HttpClient _client;
    private bool _disposed;

    public ElasticClient(ElasticClientConfiguration configuration)
    {
        var byteArray = Encoding.ASCII.GetBytes(configuration.Credentials);


        var handler = new HttpClientHandler();
        if (configuration.DoNotVerifySsl)
        {
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (_, _, _, _) => true;
        }

        _client = new HttpClient(handler)
        {
            BaseAddress = configuration.BaseAddress
        };
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
    }

    public async Task<ActionData> Bulk<TType>(IEnumerable<ActionDocumentData<TType>> data, string? index = null)
        where TType : struct
    {
        var stringBuilder = new StringBuilder();
        foreach (var d in data)
        {
            var actionType = d.ActionType;

            var acton = new IndexData
            {
                Id = d.DocumentId,
                Index = d.DocumentIndex
            };
            var ac = new ActionDocumentData<IndexData>();
            ac.SetDocument(actionType, acton);

            var actonLine = JsonSerializer.Serialize(ac);
            stringBuilder.AppendLine(actonLine);
            if (actionType == Action.Delete) continue;
            var line = JsonSerializer.Serialize(d.Document);
            stringBuilder.AppendLine(line);
        }

        var body = stringBuilder.ToString();
        var url = GetUrl("_bulk", index: index);

        var content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
        var result = await _client.PostAsync(url, content);
        result.EnsureSuccessStatusCode();

        var stream = await result.Content.ReadAsStreamAsync();
        var respBody = await JsonSerializer.DeserializeAsync<ActionData>(stream);
        return respBody;
    }

    public async Task<DocumentData> PostDocument<TType>(TType data, string? id = null, string? index = null)
    {
        var body = JsonSerializer.Serialize(data);
        var url = GetUrl("_doc", id, index);

        var content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
        var result = await _client.PostAsync(url, content);
        result.EnsureSuccessStatusCode();

        var stream = await result.Content.ReadAsStreamAsync();
        var respBody = await JsonSerializer.DeserializeAsync<DocumentData>(stream);
        return respBody;
    }

    public async Task<string> PostSearch(string? index = null)
    {
        var data = new QueryData
        {
        };
        var body = JsonSerializer.Serialize(data);
        var url = GetUrl("_search", index: index);

        var content = new StringContent(body, Encoding.UTF8, MediaTypeNames.Application.Json);
        var result = await _client.PostAsync(url, content);
        result.EnsureSuccessStatusCode();

        var stream = await result.Content.ReadAsStringAsync();
        Console.WriteLine(stream);
        return stream;
    }

    private string GetUrl(string action, string? id = null, string? index = null)
    {
        return index is null ? id is null ? $"/{action}" : $"/{action}/{id}" :
            id is null ? $"/{index}/{action}" : $"/{index}/{action}/{id}";
    }

    private string JoinIndex(char del = '-', params string[] keys)
    {
        return JoinIndexPath(keys, del: del);
    }

    private string JoinIndexPath(string[] keys, bool wildCard = false, char del = '-')
    {
        if (del <= 0) throw new ArgumentOutOfRangeException(nameof(del));
        return wildCard ? string.Join(del, keys, "*") : string.Join(del, keys);
    }


    public async Task<ClusterInfoData> GetClient()
    {
        var result = await _client.GetAsync("/");
        result.EnsureSuccessStatusCode();
        var stream = await result.Content.ReadAsStreamAsync();
        var respBody = await JsonSerializer.DeserializeAsync<ClusterInfoData>(stream);
        return respBody;
    }


    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _client?.Dispose();
        }

        _disposed = true;
    }

    protected virtual ValueTask DisposeAsyncCore()
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }
}