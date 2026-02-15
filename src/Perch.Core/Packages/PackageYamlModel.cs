namespace Perch.Core.Packages;

internal sealed class PackageYamlModel
{
    public List<PackageEntryYamlModel>? Packages { get; set; }
}

internal sealed class PackageEntryYamlModel
{
    public string? Name { get; set; }
    public string? Manager { get; set; }
}
