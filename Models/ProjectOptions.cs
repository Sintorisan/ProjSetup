using Enums;

namespace Models
{
    public class ProjectOptions
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectPath { get; set; } = string.Empty;

        public ApiType? ApiType { get; set; }
        public ProjectStructure? Structure { get; set; }
        public DatabaseOptions DbOptions { get; set; } = new();
    }
}