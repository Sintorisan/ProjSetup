using Enums;

namespace Models;

public class Node
{
    public string Name { get; set; } = string.Empty;
    public ProjectType Type { get; set; }
    public LayerType? Layer { get; set; }
    public List<string> Folders { get; set; } = new();
}