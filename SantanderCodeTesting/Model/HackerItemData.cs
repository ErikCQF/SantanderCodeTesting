namespace SantanderCodeTesting.Model;


public sealed class HackerItemData
{
    public int Id { get; set; }
    public string? Type { get; set; }
    public string? Title { get; set; }
    public string? Url { get; set; }
    public string? By { get; set; }
    public long Time { get; set; }
    public int Score { get; set; }
    public int Descendants { get; set; }
    public bool Dead { get; set; }
    public bool Deleted { get; set; }
}
