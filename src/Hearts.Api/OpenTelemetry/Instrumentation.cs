using System.Diagnostics;

namespace Hearts.Api.OpenTelemetry;

public class Instrumentation
{
    private readonly ActivitySource activitySource = new("Hearts.Api");

    public virtual Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return this.activitySource.StartActivity(name, kind);
    }

    public virtual Activity? StartActivity(string name, ActivityKind kind, ActivityContext parentContext)
    {
        return this.activitySource.StartActivity(name, kind, parentContext);
    }
}
