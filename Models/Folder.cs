namespace Models;

public class Folder
{
    public string Name { get; set; } = string.Empty;
    public List<string> Files { get; set; } = new();
}