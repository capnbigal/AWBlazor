namespace AWBlazorApp.Features.Processes.Timelines.Application;

public class ChainDefinitionNotFoundException : Exception
{
    public string ChainCode { get; }
    public ChainDefinitionNotFoundException(string chainCode)
        : base($"No active ProcessChainDefinition with code '{chainCode}'.") => ChainCode = chainCode;
}

public class ChainStepNotSupportedException : Exception
{
    public string ParentEntity { get; }
    public string ChildEntity { get; }
    public string ForeignKey { get; }
    public ChainStepNotSupportedException(string parent, string child, string fk)
        : base($"No IChainHopQuery registered for {parent}->{child} via {fk}.")
        => (ParentEntity, ChildEntity, ForeignKey) = (parent, child, fk);
}
