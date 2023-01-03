namespace DevToys.Api;

public static class ExtensionOrderer
{
    public static IEnumerable<Lazy<TExtension, TMetadata>> Order<TExtension, TMetadata>(
        IEnumerable<Lazy<TExtension, TMetadata>> extensions)
        where TMetadata : IOrderableMetadata
    {
        var graph = new Graph<TExtension, TMetadata>(extensions);
        return graph.TopologicalSort();
    }

    public static void CheckForCycles<TExtension, TMetadata>(
        IEnumerable<Lazy<TExtension, TMetadata>> extensions)
        where TMetadata : IOrderableMetadata
    {
        var graph = new Graph<TExtension, TMetadata>(extensions);
        graph.CheckForCycles();
    }

    private sealed class Node<TExtension, TMetadata> where TMetadata : IOrderableMetadata
    {
        internal string Name => Extension.Metadata.InternalComponentName;

        internal HashSet<Node<TExtension, TMetadata>> NodesBefore { get; }

        internal Lazy<TExtension, TMetadata> Extension { get; }

        internal Node(Lazy<TExtension, TMetadata> extension)
        {
            Extension = extension;
            NodesBefore = new HashSet<Node<TExtension, TMetadata>>();
        }

        internal void CheckForCycles()
        {
            CheckForCycles(new HashSet<Node<TExtension, TMetadata>>());
        }

        internal void Visit(List<Lazy<TExtension, TMetadata>> result, HashSet<Node<TExtension, TMetadata>> seenNodes)
        {
            if (!seenNodes.Add(this))
            {
                return;
            }

            foreach (Node<TExtension, TMetadata> before in NodesBefore)
            {
                before.Visit(result, seenNodes);
            }

            result.Add(Extension);
        }

        private void CheckForCycles(HashSet<Node<TExtension, TMetadata>> seenNodes)
        {
            if (!seenNodes.Add(this))
            {
                throw new ArgumentException($"Cycle detected in extensions. Extension Name: '{Name}'");
            }

            foreach (Node<TExtension, TMetadata> before in NodesBefore)
            {
                before.CheckForCycles(seenNodes);
            }

            seenNodes.Remove(this);
        }
    }

    private sealed class Graph<TExtension, TMetadata> where TMetadata : IOrderableMetadata
    {
        private readonly Dictionary<string, Node<TExtension, TMetadata>> _nodes = new();

        internal Graph(IEnumerable<Lazy<TExtension, TMetadata>> extensions)
        {
            foreach (Lazy<TExtension, TMetadata> extension in extensions)
            {
                var node = new Node<TExtension, TMetadata>(extension);
                _nodes.Add(node.Name, node);
            }

            foreach (Node<TExtension, TMetadata> node in _nodes.Values)
            {

                foreach (string before in node.Extension.Metadata.Before)
                {
                    Node<TExtension, TMetadata> nodeAfter = _nodes[before];
                    nodeAfter.NodesBefore.Add(node);
                }

                foreach (string after in node.Extension.Metadata.After)
                {
                    Node<TExtension, TMetadata> nodeBefore = _nodes[after];
                    node.NodesBefore.Add(nodeBefore);
                }
            }
        }

        internal IList<Lazy<TExtension, TMetadata>> TopologicalSort()
        {
            CheckForCycles();

            var result = new List<Lazy<TExtension, TMetadata>>();
            var seenNodes = new HashSet<Node<TExtension, TMetadata>>();

            foreach (Node<TExtension, TMetadata> node in _nodes.Values)
            {
                node.Visit(result, seenNodes);
            }

            return result;
        }

        internal void CheckForCycles()
        {
            foreach (Node<TExtension, TMetadata> node in _nodes.Values)
            {
                node.CheckForCycles();
            }
        }
    }
}
