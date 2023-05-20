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
    public Dictionary<string, Operation> Operations { get; } = new();

    public List<Element> Elements = new();
    
    public void Read(string xml) => Read(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

    public void Read(Stream xsd)
    {
        using var reader = XmlReader.Create(xsd);
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.XmlDeclaration)
                continue;

            if (reader.NodeType != XmlNodeType.Element) continue;
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

    public void ResolveAll()
    {
        if (Root.Data.Is(WsdlGlossary.Definitions))
            ResolveNamespaces(Root.Data);

        foreach (var element in Root.Children)
        {
            if (element.Data.Is(WsdlGlossary.Types))
                ResolveTypes(element);
            if (element.Data.Is(WsdlGlossary.Message))
                ResolveMessage(element);
            if (element.Data.Is(WsdlGlossary.PortType))
                ResolvePortType(element);
            if (element.Data.Is(WsdlGlossary.Binding))
                ResolveBinding(element);
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
            string element = partNode.Data.FindAttribute(WsdlGlossary.Element);
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
            var operation = Operation.Create(operationName);
            foreach (var operationChild in operationNode.Children)
            {
                var message = operationChild.Data.FindAttribute(WsdlGlossary.Message);
                var action = operationChild.Data.FindAttribute(WsdlGlossary.WsawAction, WsdlGlossary.WsamAction);

                if (operationChild.Data.Is(WsdlGlossary.OperationInput))
                    operation.SetInput(action, message);

                if (operationChild.Data.Is(WsdlGlossary.OperationOutput))
                    operation.SetOutput(action, message);
            }

            Operations.Add(operation.Name, operation);
        }
    }

    private void ResolveBinding(TreeNode<NodeElement> node)
    {
        foreach (var operationNode in node.Children)
        {
            if (operationNode.Data.Is(WsdlGlossary.Operation))
            {
                var operationName = operationNode.Data.FindNameAttribute();
                var soapOperation = operationNode.FindChildByName(SoapGlossary.Operation);

                if (Operations.TryGetValue(operationName, out var operation))
                {
                    var soapAction = soapOperation.FindAttribute(SoapGlossary.Action);
                    operation.SetSoapAction(soapAction);
                }
            }
        }
    }

    private void ResolveTypes(TreeNode<NodeElement> node)
    {
        foreach (var element in node.Children) //Schema
            ResolveSchema(element);
    }

    private void ResolveSchema(TreeNode<NodeElement> node)
    {
        foreach (var nodeElement in node.Children)
        {
            if (!nodeElement.Data.Is(WsdlGlossary.Element)) continue;
            var name = nodeElement.Data.FindNameAttribute();
            var type = nodeElement.Data.FindTypeAttribute();

            if (string.IsNullOrEmpty(type)) continue;
            
            var isNillable = nodeElement.Data.FindNillableAttribute();

            var element = Element.Create(name, isNillable, type);
            Elements.Add(element);
        }
    }
}