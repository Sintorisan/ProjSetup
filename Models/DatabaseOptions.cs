using Enums;

namespace Models;

public class DatabaseOptions
{
    public DatabaseType DatabaseType { get; set; } = DatabaseType.None;
    public bool HasEfCore { get; set; } = false;
    public List<AddFileOptions> FilesToAdd { get; set; } = new();
    public bool UseIdentity { get; set; }
}