namespace Hearts.Contracts;

public record Game(Guid Id, string WorkflowInstanceId, Player[] Players, PassingDirection PassingDirection = PassingDirection.None);
