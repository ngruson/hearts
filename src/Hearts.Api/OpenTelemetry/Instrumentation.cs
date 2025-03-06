using System.Diagnostics;

namespace Hearts.Api.OpenTelemetry;

public class Instrumentation
{
    public ActivitySource ActivitySource => new("Hearts.Api");

    public virtual Activity? StartActivity(string name, ActivityKind kind, ActivityContext parentContext)
    {
        return this.ActivitySource.StartActivity(name, kind, parentContext);
    }
}
