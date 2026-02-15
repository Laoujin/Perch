namespace Perch.Core.Wizard;

[Flags]
public enum UserProfile
{
    None = 0,
    Developer = 1,
    Creative = 2,
    PowerUser = 4,
    Gamer = 8,
    Minimal = 16,
}
