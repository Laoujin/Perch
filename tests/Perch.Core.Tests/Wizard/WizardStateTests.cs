using System.Collections.Immutable;

using Perch.Core.Wizard;

namespace Perch.Core.Tests.Wizard;

[TestFixture]
public sealed class WizardStateTests
{
    [Test]
    public void DefaultState_HasExpectedDefaults()
    {
        var state = new WizardState();

        Assert.Multiple(() =>
        {
            Assert.That(state.RepoMode, Is.EqualTo(RepoSetupMode.StartFresh));
            Assert.That(state.RepoPath, Is.Null);
            Assert.That(state.CloneUrl, Is.Null);
            Assert.That(state.SelectedProfiles, Is.EqualTo(UserProfile.None));
            Assert.That(state.MachineName, Is.Not.Empty);
            Assert.That(state.SelectedDotfiles, Is.Empty);
            Assert.That(state.AppsToInstall, Is.Empty);
            Assert.That(state.FontsToInstall, Is.Empty);
            Assert.That(state.ExtensionsToSync, Is.Empty);
            Assert.That(state.TweaksToApply, Is.Empty);
        });
    }

    [Test]
    public void SelectedDotfiles_CanBeModified()
    {
        var state = new WizardState();

        state.SelectedDotfiles = ["/home/.gitconfig", "/home/.bashrc"];

        Assert.That(state.SelectedDotfiles, Has.Count.EqualTo(2));
    }

    [Test]
    public void SelectedProfiles_CanCombineFlags()
    {
        var state = new WizardState
        {
            SelectedProfiles = UserProfile.Developer | UserProfile.PowerUser,
        };

        Assert.Multiple(() =>
        {
            Assert.That(state.SelectedProfiles.HasFlag(UserProfile.Developer), Is.True);
            Assert.That(state.SelectedProfiles.HasFlag(UserProfile.PowerUser), Is.True);
            Assert.That(state.SelectedProfiles.HasFlag(UserProfile.Gamer), Is.False);
        });
    }
}
