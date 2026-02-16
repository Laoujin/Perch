using System.Collections.Immutable;
using Perch.Core.Modules;

namespace Perch.Core.Registry;

public interface IRegistryCaptureService
{
    RegistryCaptureResult Capture(ImmutableArray<RegistryEntryDefinition> entries);
}
