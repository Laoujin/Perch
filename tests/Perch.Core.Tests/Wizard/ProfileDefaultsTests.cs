using Perch.Core.Wizard;

namespace Perch.Core.Tests.Wizard;

[TestFixture]
public sealed class ProfileDefaultsTests
{
    [Test]
    public void GetDotfileGroupsFor_Developer_IncludesGitShellEditorsNode()
    {
        var groups = ProfileDefaults.GetDotfileGroupsFor(UserProfile.Developer);

        Assert.Multiple(() =>
        {
            Assert.That(groups, Does.Contain("Git"));
            Assert.That(groups, Does.Contain("Shell"));
            Assert.That(groups, Does.Contain("Editors"));
            Assert.That(groups, Does.Contain("Node.js"));
        });
    }

    [Test]
    public void GetDotfileGroupsFor_Minimal_ReturnsEmpty()
    {
        var groups = ProfileDefaults.GetDotfileGroupsFor(UserProfile.Minimal);

        Assert.That(groups, Is.Empty);
    }

    [Test]
    public void GetDotfileGroupsFor_MultipleProfiles_UnionsGroups()
    {
        var groups = ProfileDefaults.GetDotfileGroupsFor(UserProfile.Developer | UserProfile.PowerUser);

        Assert.That(groups, Does.Contain("WSL"));
        Assert.That(groups, Does.Contain("Git"));
    }

    [Test]
    public void GetDotfileGroupsFor_None_ReturnsEmpty()
    {
        var groups = ProfileDefaults.GetDotfileGroupsFor(UserProfile.None);

        Assert.That(groups, Is.Empty);
    }

    [Test]
    public void GetTweakProfilesFor_Developer_IncludesDeveloper()
    {
        var profiles = ProfileDefaults.GetTweakProfilesFor(UserProfile.Developer);

        Assert.That(profiles, Does.Contain("developer"));
    }

    [Test]
    public void GetTweakProfilesFor_PowerUser_IncludesBoth()
    {
        var profiles = ProfileDefaults.GetTweakProfilesFor(UserProfile.PowerUser);

        Assert.Multiple(() =>
        {
            Assert.That(profiles, Does.Contain("developer"));
            Assert.That(profiles, Does.Contain("power-user"));
        });
    }

    [Test]
    public void GetTweakProfilesFor_Gamer_IncludesGamer()
    {
        var profiles = ProfileDefaults.GetTweakProfilesFor(UserProfile.Gamer);

        Assert.That(profiles, Does.Contain("gamer"));
    }
}
