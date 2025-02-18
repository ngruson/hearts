using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

internal class AutoNSubstituteDataAttribute() : AutoDataAttribute(() => CreateFixture())
{
    private static IFixture CreateFixture()
    {
        IFixture fixture = new Fixture()
            .Customize(new AutoNSubstituteCustomization());

        fixture.Behaviors
           .OfType<ThrowingRecursionBehavior>()
           .ToList()
           .ForEach(b => fixture.Behaviors.Remove(b));

        //fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }
}
