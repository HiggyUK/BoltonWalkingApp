namespace BoltonWalking.App.Models;

public class CommitteeMember
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Quote { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}
