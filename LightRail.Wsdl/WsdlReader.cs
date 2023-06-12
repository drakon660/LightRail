using System.Text;
using System.Xml;
using LightRail.Wsdl.Core;

namespace LightRail.Wsdl;

public class WsdlReader
{
    public string ServiceName { get; private set; } = "";
    public string SoapActionNamespace { get; private set; }
    public string TargetNamespace { get; private set; }

    private const char Separator = ':';

    public TreeNode<NodeElement> Root { get; private set; }
    public Dictionary<string, IReadOnlyList<Part>> Messages { get; } = new();
    public Dictionary<string, Operation> Operations { get; } = new();
    
    public Dictionary<string, Element> ComplexTypes { get; } = new();
    public List<Element> Elements { get; } = new();

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
        if (Root.Content.Is(WsdlGlossary.Definitions))
            ResolveNamespaces(Root.Content);

        foreach (var element in Root.Children)
        {
            if (element.Content.Is(WsdlGlossary.Types))
                ResolveTypes(element);
            if (element.Content.Is(WsdlGlossary.Message))
                ResolveMessage(element);
            if (element.Content.Is(WsdlGlossary.PortType))
                ResolvePortType(element);
            if (element.Content.Is(WsdlGlossary.Binding))
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
        var name = node.Content.FindNameAttribute();
        var parts = new List<Part>();
        foreach (var partNode in node.Children)
        {
            string partName = partNode.Content.FindNameAttribute();
            string element = partNode.Content.FindAttribute(WsdlGlossary.Element);
            var part = new Part(partName, element);
            parts.Add(part);
        }

        Messages.Add(name, parts);
    }

    private void ResolvePortType(TreeNode<NodeElement> node)
    {
        foreach (var operationNode in node.Children)
        {
            var operationName = operationNode.Content.FindNameAttribute();
            var operation = Operation.Create(operationName);
            foreach (var operationChild in operationNode.Children)
            {
                var message = operationChild.Content.FindAttribute(WsdlGlossary.Message);
                var action = operationChild.Content.FindAttribute(WsdlGlossary.WsawAction, WsdlGlossary.WsamAction);

                if (operationChild.Content.Is(WsdlGlossary.OperationInput))
                    operation.SetInput(action, message);

                if (operationChild.Content.Is(WsdlGlossary.OperationOutput))
                    operation.SetOutput(action, message);
            }

            Operations.Add(operation.Name, operation);
        }
    }

    private void ResolveBinding(TreeNode<NodeElement> node)
    {
        foreach (var operationNode in node.Children)
        {
            if (operationNode.Content.Is(WsdlGlossary.Operation))
            {
                var operationName = operationNode.Content.FindNameAttribute();
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
        foreach (var child in node.Children)
        {
            if (child.Content.Is(WsdlGlossary.Element) && child.HasChildren)
            {
                ResolveGlobalElement(child);
            }

            if (child.Content.Is(WsdlGlossary.Element) && !child.HasChildren)
            {
                var element = child.Content.ToXsdElement();
                Elements.Add(element);
            }

            if (child.Content.Is(WsdlGlossary.ComplexType))
            {
                var element = ResolveComplexType(child);
                ComplexTypes.Add(element.Name, element);
            }
        }
    }

    private Element ResolveComplexType(TreeNode<NodeElement> node)
    {
        var grandChild = node.FirstChild;

        var parent = node.Content.ToXsdElement();

        if (!parent.HasName)
            parent = node.Parent.Content.ToXsdElement();
        
        if (grandChild.Content.Is(WsdlGlossary.Sequence))
        {
            foreach (var grandChildChild in grandChild.Children)
            {
                if (!grandChildChild.Content.Is(WsdlGlossary.Element))
                    throw new Exception($"sequence child {grandChildChild.Content.Name} not recognized");

                var element = grandChildChild.Content.ToXsdElement();
                parent.AddChild(element);
            }
        }

        return parent;
    }
    
    private void ResolveGlobalElement(TreeNode<NodeElement> node)
    {
        var parentElement = node.Content.ToXsdElement();

        if (parentElement.HasTypeAttribute) //Type //Ref? element inside Types
        {
            //return;
        }
        
        var child = node.FirstChild;
        if (child != null)
        {
            if (child.Content.Is(WsdlGlossary.ComplexType))
            {
                var complexTypeElement = ResolveComplexType(child);
                Elements.Add(complexTypeElement);
            }
        }
    }

    public OperationParam BuildFromElement(Element element)
    {
        if (element.IsSimpleType)
        {
            return OperationParam.Create(element.Name ,element.Type);
        }

        var operationParam = OperationParam.Create(element.Name, element.Type);
            
        foreach (var childElement in element.Children)
        {
            var operationParamChild = BuildFromElement(childElement);
            operationParam.AddChild(operationParamChild);
        }   
            
        return operationParam;
    }
    

    public IReadOnlyDictionary<string, List<OperationParam>> Build()
    {
        Dictionary<string, List<OperationParam>> operationWithParams = new();

        foreach (var operation in Operations)
        {
            var inputMessage = operation.Value.Input.Message.Replace(SoapActionNamespace + ":", string.Empty);
            var outputMessage = operation.Value.Output.Message.Replace(SoapActionNamespace + ":", string.Empty);

            List<OperationParam> operationParameters = new List<OperationParam>();

            if (Messages.TryGetValue(inputMessage, out var inputMessageParts))
            {
                var part = inputMessageParts.First();
                var elementName = part.Element;

                var element = Elements.FirstOrDefault(x =>
                    x.Name == elementName.Replace(SoapActionNamespace + ":", string.Empty));

                if (element.Children.Any())
                {
                    foreach (var childElement in element.Children)
                    {
                        if (childElement.IsSimpleType)
                        {
                            OperationParam param = BuildFromElement(childElement);
                            operationParameters.Add(param);    
                        }
                        else
                        {
                            var complexType = ComplexTypes[childElement.TypeWithoutNamespace];
                            OperationParam param = BuildFromElement(complexType);
                            operationParameters.Add(param);    
                        }
                    }
                }
                
                // if (element.IsSimpleType)
                // {
                //     var operationParam = OperationParam.Create(element.Name);
                //     operationParameters.Add(operationParam);
                // }
                // else
                // {
                //     foreach (var childElement in element.Children)
                //     {
                //         if (childElement.IsSimpleType)
                //         {
                //             var operationParam = OperationParam.Create(childElement.Name);
                //             operationParameters.Add(operationParam);
                //         }
                //         else
                //         {
                //             
                //         }
                //     }
                // }
            }

            //TODO
            // if (Messages.TryGetValue(outputMessage, out var outputMessageParts))
            // {
            //     var part = outputMessageParts.First();
            //     var elementName = part.Element;
            //     var element = Elements.FirstOrDefault(x => x.Name == elementName.Replace(SoapActionNamespace + ":", string.Empty));
            //     
            //     if(element.IsSimpleType)
            //         operationParameters.Add(element.Name);
            //     else
            //     {
            //         foreach (var childElement in element.Children)
            //         {
            //             operationParameters.Add(childElement.Name);
            //         }
            //     }
            // }

            operationWithParams.Add(operation.Key, operationParameters);
        }

        return operationWithParams;
    }
}