namespace DataMiner;

public abstract class BaseVariableDicer
{
    public string Target { get; protected set; }
    public string RootAttribute { get; protected set; }
    public bool IsLogarithmic { get; protected set; }
    public bool IsDelta { get; protected set; }
    public bool IsVisit { get; protected set; }
    public string VariableName { get; protected set; }

    protected void ValidateVariableName(string variableName)
    {
        if (string.IsNullOrWhiteSpace(variableName))
            throw new ArgumentException("Variable name cannot be null or empty.", nameof(variableName));
    }
}