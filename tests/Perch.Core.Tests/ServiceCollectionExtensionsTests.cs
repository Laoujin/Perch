using Microsoft.Extensions.DependencyInjection;
using Perch.Core;

namespace Perch.Core.Tests;

[TestFixture]
public sealed class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddPerchCore_ReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddPerchCore();

        Assert.That(result, Is.SameAs(services));
    }
}
