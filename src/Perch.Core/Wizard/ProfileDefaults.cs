using System.Collections.Immutable;

namespace Perch.Core.Wizard;

public static class ProfileDefaults
{
    private static readonly ImmutableDictionary<UserProfile, ImmutableHashSet<string>> DotfileGroups =
        new Dictionary<UserProfile, ImmutableHashSet<string>>
        {
            [UserProfile.Developer] = ["Git", "Shell", "Editors", "Node.js"],
            [UserProfile.Creative] = ["Editors"],
            [UserProfile.PowerUser] = ["Git", "Shell", "Editors", "Node.js", "WSL"],
            [UserProfile.Gamer] = ImmutableHashSet<string>.Empty,
            [UserProfile.Minimal] = ImmutableHashSet<string>.Empty,
        }.ToImmutableDictionary();

    private static readonly ImmutableDictionary<UserProfile, ImmutableHashSet<string>> TweakProfiles =
        new Dictionary<UserProfile, ImmutableHashSet<string>>
        {
            [UserProfile.Developer] = ["developer"],
            [UserProfile.Creative] = ImmutableHashSet<string>.Empty,
            [UserProfile.PowerUser] = ["developer", "power-user"],
            [UserProfile.Gamer] = ["gamer"],
            [UserProfile.Minimal] = ImmutableHashSet<string>.Empty,
        }.ToImmutableDictionary();

    public static ImmutableHashSet<string> GetDotfileGroupsFor(UserProfile profiles)
    {
        var groups = ImmutableHashSet<string>.Empty;
        foreach (UserProfile profile in Enum.GetValues<UserProfile>())
        {
            if (profile != UserProfile.None && profiles.HasFlag(profile) && DotfileGroups.TryGetValue(profile, out var profileGroups))
            {
                groups = groups.Union(profileGroups);
            }
        }

        return groups;
    }

    public static ImmutableHashSet<string> GetTweakProfilesFor(UserProfile profiles)
    {
        var result = ImmutableHashSet<string>.Empty;
        foreach (UserProfile profile in Enum.GetValues<UserProfile>())
        {
            if (profile != UserProfile.None && profiles.HasFlag(profile) && TweakProfiles.TryGetValue(profile, out var tweakProfiles))
            {
                result = result.Union(tweakProfiles);
            }
        }

        return result;
    }
}
