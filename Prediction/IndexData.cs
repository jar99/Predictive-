using System.Text.Json.Serialization;

namespace Prediction;

public struct ActionData
{
    [JsonPropertyName("took")] public int Took { get; set; }
    [JsonPropertyName("errors")] public bool Errors { get; set; }

    [JsonPropertyName("items")] public ICollection<ActionDocumentData<DocumentData>> Items { get; set; }
}

public enum Action
{
    [JsonPropertyName("create")] Create,
    [JsonPropertyName("delete")] Delete,
    [JsonPropertyName("index")] Index,
    [JsonPropertyName("update")] Update
}

public struct QueryData
{
    [JsonPropertyName("query")] public string Query { get; set; }
}

public enum Modifier
{
}

public struct FieldValueFactor
{
}

public struct ActionDocumentData<TType> where TType : struct
{
    [JsonIgnore] public string DocumentId { get; set; }
    [JsonIgnore] public string DocumentIndex { get; set; }

    [JsonIgnore]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Action ActionType { get; private set; }

    [JsonIgnore] public TType Document { get; private set; }

    public void SetDocument(Action actionType, TType? document)
    {
        ActionType = actionType;
        Document = document ?? default;
    }

    public TType? GetDocument(Action actionType)
    {
        return ActionType == actionType ? Document : null;
    }

    [JsonPropertyName("create")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TType? Create
    {
        get => GetDocument(Action.Create);
        set => SetDocument(Action.Create, value);
    }

    [JsonPropertyName("delete")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TType? Delete
    {
        get => GetDocument(Action.Delete);
        set => SetDocument(Action.Delete, value);
    }

    [JsonPropertyName("index")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TType? Index
    {
        get => GetDocument(Action.Index);
        set => SetDocument(Action.Index, value);
    }

    [JsonPropertyName("update")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TType? Update
    {
        get => GetDocument(Action.Update);
        set => SetDocument(Action.Update, value);
    }
}

public interface IIndexData
{
    [JsonPropertyName("_index")] public string Index { get; set; }

    // [JsonPropertyName("_type")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    // public string? Type { get; set; }

    [JsonPropertyName("_id")] public string Id { get; set; }

    // [JsonPropertyName("_version")]
    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    // public int? Version { get; set; }
}

public struct IndexData : IIndexData
{
    [JsonPropertyName("_index")] public string Index { get; set; }

    [JsonPropertyName("_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }

    [JsonPropertyName("_id")] public string Id { get; set; }

    [JsonPropertyName("_version")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Version { get; set; }
}

public struct DocumentData : IIndexData
{
    [JsonPropertyName("_index")] public string Index { get; set; }

    [JsonPropertyName("_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }

    [JsonPropertyName("_id")] public string Id { get; set; }

    [JsonPropertyName("_version")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Version { get; set; }

    [JsonPropertyName("result")] public string Result { get; set; }

    [JsonPropertyName("_shards")] public ShardData Shards { get; set; }

    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Status { get; set; }

    [JsonPropertyName("_seq_no")] public int SeqNo { get; set; }

    [JsonPropertyName("_primary_term")] public int PrimaryTerm { get; set; }
}

public struct ShardData
{
    [JsonPropertyName("total")] public int Total { get; set; }

    [JsonPropertyName("successful")] public int Successful { get; set; }

    [JsonPropertyName("skipped")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Skipped { get; set; }

    [JsonPropertyName("failed")] public int Failed { get; set; }
}

public struct ClusterInfoData
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("cluster_name")] public string ClusterName { get; set; }

    [JsonPropertyName("cluster_uuid")] public string ClusterUuid { get; set; }

    [JsonPropertyName("version")] public ClusterVersionData Version { get; set; }

    [JsonPropertyName("tagline")] public string Tagline { get; set; }
}

public struct ClusterVersionData
{
    [JsonPropertyName("distribution")] public string Distribution { get; set; }

    [JsonPropertyName("number")]
    [JsonConverter(typeof(VersionJsonConverter))]
    public Version Number { get; set; }

    [JsonPropertyName("build_type")] public string BuildType { get; set; }

    [JsonPropertyName("build_hash")] public string BuildHash { get; set; }

    [JsonPropertyName("build_date")] public DateTime BuildDate { get; set; }

    [JsonPropertyName("build_snapshot")] public bool BuildSnapshot { get; set; }

    [JsonPropertyName("lucene_version")]
    [JsonConverter(typeof(VersionJsonConverter))]
    public Version LuceneVersion { get; set; }

    [JsonPropertyName("minimum_wire_compatibility_version")]
    [JsonConverter(typeof(VersionJsonConverter))]
    public Version MinimumWireCompatibilityVersion { get; set; }

    [JsonPropertyName("minimum_index_compatibility_version")]
    [JsonConverter(typeof(VersionJsonConverter))]
    public Version MinimumIndexCompatibilityVersion { get; set; }
}