namespace Models;

public class AddFileOptions
{
    public string Name { get; set; } = string.Empty;
    public Node? Layer { get; set; }
    public string Folder { get; set; } = string.Empty;
}