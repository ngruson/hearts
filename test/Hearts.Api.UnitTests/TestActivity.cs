using System.Diagnostics;

namespace Hearts.Api.UnitTests;

public class TestActivity(string operationName) : Activity(operationName)
{
    public virtual Activity AddException(Exception exception)
    {
        return this.AddException(exception, default, default);
    }
}
