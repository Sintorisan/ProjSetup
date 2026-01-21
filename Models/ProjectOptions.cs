using Enums;

namespace Models
{
    public class ProjectOptions
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectPath { get; set; } = string.Empty;

        public ApiType? ApiType { get; set; }
        public List<Node> Structure { get; set; } = new();

        public void Build()
        {

        }
    }
}