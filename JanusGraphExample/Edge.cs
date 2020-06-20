using ExRam.Gremlinq.Core.GraphElements;

namespace Test
{
    public class Edge : IEdge
    {
        public object Id { get; set; }
        public string Label { get; set; }
    }
}
