using System.Text.Json.Serialization;

namespace Document;

public struct WordData
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("word")] public string Word { get; set; }
    [JsonPropertyName("frequency")] public uint Frequency { get; set; }
    
    [JsonPropertyName("norm_frequency")]
    public double NormFrequency { get; set; }
}