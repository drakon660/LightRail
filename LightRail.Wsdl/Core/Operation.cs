namespace LightRail.Wsdl.Core;

public sealed class Operation
{
    public string Name { get; }
    public string SoapAction { get; private set; }
    public OperationInput Input { get; private set; }
    public OperationOutput Output { get; private set; }

    private Operation(string name)
    {
        Name = name;
    }

    public static Operation Create(string name) => new (name);

    public void SetSoapAction(string actionName)
    {
        SoapAction = actionName;
    }

    public void SetInput(string action, string message)
    {
        Input = new OperationInput(action, message);
    }

    public void SetOutput(string action, string message)
    {
        Output = new OperationOutput(action, message);
    }
}

public record OperationInput(string Action, string Message);

public record OperationOutput(string Action, string Message);