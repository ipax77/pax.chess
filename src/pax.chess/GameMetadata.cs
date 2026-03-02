namespace pax.chess;

public sealed class GameMetadata
{
    public string? Event { get; set; }
    public string? Site { get; set; }
    public DateTime? Date { get; set; }
    public string? Round { get; set; }
    public string? White { get; set; }
    public string? Black { get; set; }
    public string? Annotator { get; set; }

    public Dictionary<string, string> AdditionalTags { get; } = [];
}
