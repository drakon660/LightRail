using System.Text;
using System.Xml;
using LightRail.Wsdl.Core;

namespace LightRail.Wsdl;

public class WsdlReader
{
    public string ServiceName { get; private set; } = "";
    public string SoapActionNamespace { get; private set; }
    public string TargetNamespace { get; private set; }
    public string WsdlNamespace { get; private set; }
    public string SoapNamespace { get; private set; }
    public string XMLSchemaNamespace { get; private set; }

    private const char Separator = ':';

    public TreeNode<NodeElement> Root { get; private set; }

    public Dictionary<string, IReadOnlyList<Part>> Messages { get; } = new();
    public Dictionary<string, Operation> Operations { get; } = new ();
    public void Read(string xml) => Read(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

    public void Read(Stream xsd)
    {
        using var reader = XmlReader.Create(xsd);
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.XmlDeclaration)
                continue;

            if (reader.NodeType == XmlNodeType.Element)
            {
                var attributes = new Dictionary<string, string>();

                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                        attributes.Add(reader.Name, reader.Value);

                    // Move the reader back to the element
                    reader.MoveToElement();
                }

                var nodeElement = NodeElement.Create(reader.Name, reader.LocalName, attributes);

                if (Root != null)
                {
                    var currentTreeNode = TreeNode<NodeElement>.Create(nodeElement, reader.Depth);
                    Root.AddChildAtLevel(currentTreeNode.Level, currentTreeNode);
                }
                else
                {
                    Root = TreeNode<NodeElement>.Create(nodeElement, reader.Depth);
                }
            }
        }
    }

    public void ResolveAll()
    {
        if (Root.Data.LocalName == WsdlGlossary.Definitions)
            ResolveNamespaces(Root.Data);

        foreach (var element in Root.Children)
        {
            if (element.Data.Is(WsdlGlossary.Message))
                ResolveMessage(element);
            if (element.Data.Is(WsdlGlossary.PortType))
                ResolvePortType(element);
        }
    }

    private void ResolveNamespaces(NodeElement element)
    {
        ServiceName = element.FindNameAttribute();
        TargetNamespace = element.FindAttribute(WsdlGlossary.TargetNamespace);
        var attribute =
            element.Attributes.First(kv => kv.Value == TargetNamespace && kv.Key != WsdlGlossary.TargetNamespace);

        SoapActionNamespace = attribute.Key.Split(Separator)[1];
    }

    private void ResolveMessage(TreeNode<NodeElement> node)
    {
        var name = node.Data.FindNameAttribute();
        var parts = new List<Part>();
        foreach (var partNode in node.Children)
        {
            string partName = partNode.Data.FindNameAttribute();
            string element = partNode.Data.FindAttribute("element");
            var part = new Part(partName, element);
            parts.Add(part);
        }

        Messages.Add(name, parts);
    }

    private void ResolvePortType(TreeNode<NodeElement> node)
    {
        foreach (var operationNode in node.Children)
        {
            var operationName = operationNode.Data.FindNameAttribute();
            Operation operation = Operation.Create(operationName);
            foreach (var operationChild in operationNode.Children)
            {
                var message = operationChild.Data.FindAttribute(WsdlGlossary.Message);
                var action = operationChild.Data.FindAttribute(WsdlGlossary.WsawAction, WsdlGlossary.WsamAction);
                
                if (operationChild.Data.Is(WsdlGlossary.OperationInput))
                    operation.SetInput(action, message);
                
                if (operationChild.Data.Is( WsdlGlossary.OperationOutput))
                    operation.SetOutput(action, message);
            }
            Operations.Add(operation.Name, operation);
        }
    }

    private void ResolveBinding(TreeNode<NodeElement> node)
    {
        foreach (var operationNode in node.Children)
        {
            
        }
    }
}