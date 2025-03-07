using System.Diagnostics;
using Hearts.Api.OpenTelemetry;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace Hearts.Api.UnitTests.OpenTelemetry;

public class InstrumentationUnitTests
{
    [Theory, AutoNSubstituteData]
    internal void start_activity_given_name(
        Instrumentation sut,
        string name)
    {
        // Arrange

        using TracerProvider tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Hearts.Api")
            .Build();

        // Act

        Activity? activity = sut.StartActivity(name);

        // Assert

        Assert.Equal(name, activity?.DisplayName);
    }

    [Theory, AutoNSubstituteData]
    internal void start_activity_given_name_kind_context(
        Instrumentation sut,
        string name,
        ActivityKind activityKind,
        ActivityContext activityContext)
    {
        // Arrange

        using TracerProvider tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Hearts.Api")
            .Build();

        // Act

        Activity? activity = sut.StartActivity(name, activityKind, activityContext);

        // Assert

        Assert.Equal(name, activity?.DisplayName);
        Assert.Equal(activityKind, activity?.Kind);
        Assert.Equal(activityContext.TraceId, activity?.TraceId);
        Assert.Equal(activityContext.SpanId, activity?.ParentSpanId);
    }
}
