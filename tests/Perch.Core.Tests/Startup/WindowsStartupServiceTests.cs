using System.Runtime.Versioning;

using NSubstitute;

using Perch.Core.Registry;
using Perch.Core.Startup;

namespace Perch.Core.Tests.Startup;

[TestFixture]
[Platform("Win")]
[SupportedOSPlatform("windows")]
public sealed class WindowsStartupServiceTests
{
    private const string HkcuRunKey = @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string HklmRunKey = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string HkcuApprovedKey = @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
    private const string HklmApprovedKey = @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

    private IRegistryProvider _registry = null!;
    private WindowsStartupService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _registry = Substitute.For<IRegistryProvider>();
        _registry.EnumerateValues(Arg.Any<string>()).Returns([]);
        _service = new WindowsStartupService(_registry,
            userStartupFolder: Path.Combine(Path.GetTempPath(), "perch-test-startup-nonexistent"),
            allUsersStartupFolder: Path.Combine(Path.GetTempPath(), "perch-test-allusers-nonexistent"));
    }

    [Test]
    public async Task GetAll_EnumeratesHkcuAndHklmRunKeys()
    {
        _registry.EnumerateValues(HkcuRunKey).Returns(new List<RegistryValueEntry>
        {
            new("Spotify", @"""C:\Program Files\Spotify\Spotify.exe""", RegistryValueType.String),
        });
        _registry.EnumerateValues(HklmRunKey).Returns(new List<RegistryValueEntry>
        {
            new("SecurityHealth", @"""C:\Windows\System32\SecurityHealthSystray.exe""", RegistryValueType.String),
        });

        var result = await _service.GetAllAsync();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Name, Is.EqualTo("Spotify"));
        Assert.That(result[0].Source, Is.EqualTo(StartupSource.RegistryCurrentUser));
        Assert.That(result[1].Name, Is.EqualTo("SecurityHealth"));
        Assert.That(result[1].Source, Is.EqualTo(StartupSource.RegistryLocalMachine));
    }

    [Test]
    public async Task GetAll_MergesStartupApprovedDisabledState()
    {
        _registry.EnumerateValues(HkcuRunKey).Returns(new List<RegistryValueEntry>
        {
            new("EnabledApp", @"C:\app.exe", RegistryValueType.String),
            new("DisabledApp", @"C:\other.exe", RegistryValueType.String),
        });

        var enabledBytes = new byte[12];
        enabledBytes[0] = 0x02;
        var disabledBytes = new byte[12];
        disabledBytes[0] = 0x03;

        _registry.EnumerateValues(HkcuApprovedKey).Returns(new List<RegistryValueEntry>
        {
            new("EnabledApp", enabledBytes, RegistryValueType.Binary),
            new("DisabledApp", disabledBytes, RegistryValueType.Binary),
        });

        var result = await _service.GetAllAsync();

        var enabled = result.First(e => e.Name == "EnabledApp");
        var disabled = result.First(e => e.Name == "DisabledApp");
        Assert.Multiple(() =>
        {
            Assert.That(enabled.IsEnabled, Is.True);
            Assert.That(disabled.IsEnabled, Is.False);
        });
    }

    [Test]
    public async Task GetAll_NoApprovedEntry_DefaultsToEnabled()
    {
        _registry.EnumerateValues(HkcuRunKey).Returns(new List<RegistryValueEntry>
        {
            new("NewApp", @"C:\new.exe", RegistryValueType.String),
        });

        var result = await _service.GetAllAsync();

        Assert.That(result[0].IsEnabled, Is.True);
    }

    [Test]
    public async Task SetEnabled_WritesCorrectBinaryToApprovedKey()
    {
        var entry = new StartupEntry("RegistryCurrentUser:TestApp", "TestApp", @"C:\test.exe", @"C:\test.exe",
            StartupSource.RegistryCurrentUser, true);

        await _service.SetEnabledAsync(entry, false);

        _registry.Received(1).SetValue(HkcuApprovedKey, "TestApp",
            Arg.Is<byte[]>(b => b.Length == 12 && b[0] == 0x03),
            RegistryValueType.Binary);
    }

    [Test]
    public async Task SetEnabled_Enable_WritesByte02()
    {
        var entry = new StartupEntry("RegistryCurrentUser:TestApp", "TestApp", @"C:\test.exe", @"C:\test.exe",
            StartupSource.RegistryCurrentUser, false);

        await _service.SetEnabledAsync(entry, true);

        _registry.Received(1).SetValue(HkcuApprovedKey, "TestApp",
            Arg.Is<byte[]>(b => b[0] == 0x02),
            RegistryValueType.Binary);
    }

    [Test]
    public async Task SetEnabled_HklmSource_UsesHklmApprovedKey()
    {
        var entry = new StartupEntry("RegistryLocalMachine:TestApp", "TestApp", @"C:\test.exe", @"C:\test.exe",
            StartupSource.RegistryLocalMachine, true);

        await _service.SetEnabledAsync(entry, false);

        _registry.Received(1).SetValue(HklmApprovedKey, "TestApp",
            Arg.Any<byte[]>(), RegistryValueType.Binary);
    }

    [Test]
    public async Task Remove_RegistryCurrentUser_DeletesRunAndApprovedValues()
    {
        var entry = new StartupEntry("RegistryCurrentUser:TestApp", "TestApp", @"C:\test.exe", @"C:\test.exe",
            StartupSource.RegistryCurrentUser, true);

        await _service.RemoveAsync(entry);

        _registry.Received(1).DeleteValue(HkcuRunKey, "TestApp");
        _registry.Received(1).DeleteValue(HkcuApprovedKey, "TestApp");
    }

    [Test]
    public async Task Remove_RegistryLocalMachine_DeletesRunAndApprovedValues()
    {
        var entry = new StartupEntry("RegistryLocalMachine:TestApp", "TestApp", @"C:\test.exe", @"C:\test.exe",
            StartupSource.RegistryLocalMachine, true);

        await _service.RemoveAsync(entry);

        _registry.Received(1).DeleteValue(HklmRunKey, "TestApp");
        _registry.Received(1).DeleteValue(HklmApprovedKey, "TestApp");
    }

    [Test]
    public async Task Add_RegistryCurrentUser_WritesToHkcuRunKey()
    {
        await _service.AddAsync("MyApp", @"C:\myapp.exe", StartupSource.RegistryCurrentUser);

        _registry.Received(1).SetValue(HkcuRunKey, "MyApp", @"C:\myapp.exe", RegistryValueType.String);
    }

    [Test]
    public async Task Add_RegistryLocalMachine_WritesToHklmRunKey()
    {
        await _service.AddAsync("MyApp", @"C:\myapp.exe", StartupSource.RegistryLocalMachine);

        _registry.Received(1).SetValue(HklmRunKey, "MyApp", @"C:\myapp.exe", RegistryValueType.String);
    }

    [TestCase(@"""C:\Program Files\App\app.exe"" --arg", @"C:\Program Files\App\app.exe")]
    [TestCase(@"C:\App\app.exe", @"C:\App\app.exe")]
    [TestCase(@"""C:\App\app.exe""", @"C:\App\app.exe")]
    [TestCase("", null)]
    [TestCase("   ", null)]
    public void ExtractImagePath_ExtractsCorrectly(string command, string? expected)
    {
        Assert.That(WindowsStartupService.ExtractImagePath(command), Is.EqualTo(expected));
    }

    [Test]
    public async Task GetAll_StartupFolderUser_EnumeratesLnkFiles()
    {
        string userFolder = Path.Combine(Path.GetTempPath(), $"perch-startup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(userFolder);
        try
        {
            File.WriteAllText(Path.Combine(userFolder, "Discord.lnk"), "shortcut");

            var svc = new WindowsStartupService(_registry, userFolder,
                Path.Combine(Path.GetTempPath(), "perch-noexist"));

            var result = await svc.GetAllAsync();

            var folderEntries = result.Where(e => e.Source == StartupSource.StartupFolderUser).ToList();
            Assert.That(folderEntries, Has.Count.EqualTo(1));
            Assert.That(folderEntries[0].Name, Is.EqualTo("Discord.lnk"));
        }
        finally
        {
            Directory.Delete(userFolder, true);
        }
    }

    [Test]
    public async Task GetAll_StartupFolderUser_MergesApprovedState()
    {
        string userFolder = Path.Combine(Path.GetTempPath(), $"perch-startup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(userFolder);
        try
        {
            File.WriteAllText(Path.Combine(userFolder, "Disabled.lnk"), "shortcut");

            var disabledBytes = new byte[12];
            disabledBytes[0] = 0x03;
            _registry.EnumerateValues(@"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder")
                .Returns(new List<RegistryValueEntry>
                {
                    new("Disabled.lnk", disabledBytes, RegistryValueType.Binary),
                });

            var svc = new WindowsStartupService(_registry, userFolder,
                Path.Combine(Path.GetTempPath(), "perch-noexist"));

            var result = await svc.GetAllAsync();

            var entry = result.First(e => e.Name == "Disabled.lnk");
            Assert.That(entry.IsEnabled, Is.False);
        }
        finally
        {
            Directory.Delete(userFolder, true);
        }
    }

    [Test]
    public async Task GetAll_StartupFolderAllUsers_NoApprovedKey_DefaultsEnabled()
    {
        string allUsersFolder = Path.Combine(Path.GetTempPath(), $"perch-startup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(allUsersFolder);
        try
        {
            File.WriteAllText(Path.Combine(allUsersFolder, "Startup.lnk"), "shortcut");

            var svc = new WindowsStartupService(_registry,
                Path.Combine(Path.GetTempPath(), "perch-noexist"), allUsersFolder);

            var result = await svc.GetAllAsync();

            var entry = result.First(e => e.Source == StartupSource.StartupFolderAllUsers);
            Assert.That(entry.IsEnabled, Is.True);
        }
        finally
        {
            Directory.Delete(allUsersFolder, true);
        }
    }

    [Test]
    public async Task SetEnabled_StartupFolderUser_WritesToFolderApprovedKey()
    {
        var entry = new StartupEntry("StartupFolderUser:Discord.lnk", "Discord.lnk", @"C:\shortcut.lnk",
            null, StartupSource.StartupFolderUser, true);

        await _service.SetEnabledAsync(entry, false);

        _registry.Received(1).SetValue(
            @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder",
            "Discord.lnk",
            Arg.Is<byte[]>(b => b[0] == 0x03),
            RegistryValueType.Binary);
    }

    [Test]
    public async Task SetEnabled_StartupFolderAllUsers_IsNoOp()
    {
        var entry = new StartupEntry("StartupFolderAllUsers:App.lnk", "App.lnk", @"C:\shortcut.lnk",
            null, StartupSource.StartupFolderAllUsers, true);

        await _service.SetEnabledAsync(entry, false);

        _registry.DidNotReceive().SetValue(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object>(), Arg.Any<RegistryValueType>());
    }

    [Test]
    public async Task Remove_StartupFolderUser_DeletesFileAndApprovedEntry()
    {
        string userFolder = Path.Combine(Path.GetTempPath(), $"perch-startup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(userFolder);
        try
        {
            var lnkPath = Path.Combine(userFolder, "Discord.lnk");
            File.WriteAllText(lnkPath, "shortcut");

            var svc = new WindowsStartupService(_registry, userFolder,
                Path.Combine(Path.GetTempPath(), "perch-noexist"));
            var entry = new StartupEntry("StartupFolderUser:Discord.lnk", "Discord.lnk", lnkPath,
                null, StartupSource.StartupFolderUser, true);

            await svc.RemoveAsync(entry);

            Assert.That(File.Exists(lnkPath), Is.False);
            _registry.Received(1).DeleteValue(
                @"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder",
                "Discord.lnk");
        }
        finally
        {
            if (Directory.Exists(userFolder))
                Directory.Delete(userFolder, true);
        }
    }

    [Test]
    public async Task Remove_StartupFolderAllUsers_DeletesFile()
    {
        string allUsersFolder = Path.Combine(Path.GetTempPath(), $"perch-startup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(allUsersFolder);
        try
        {
            var lnkPath = Path.Combine(allUsersFolder, "App.lnk");
            File.WriteAllText(lnkPath, "shortcut");

            var svc = new WindowsStartupService(_registry,
                Path.Combine(Path.GetTempPath(), "perch-noexist"), allUsersFolder);
            var entry = new StartupEntry("StartupFolderAllUsers:App.lnk", "App.lnk", lnkPath,
                null, StartupSource.StartupFolderAllUsers, true);

            await svc.RemoveAsync(entry);

            Assert.That(File.Exists(lnkPath), Is.False);
        }
        finally
        {
            if (Directory.Exists(allUsersFolder))
                Directory.Delete(allUsersFolder, true);
        }
    }

    [Test]
    public async Task Add_StartupFolderUser_IsNoOp()
    {
        await _service.AddAsync("App", @"C:\app.exe", StartupSource.StartupFolderUser);

        _registry.DidNotReceive().SetValue(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object>(), Arg.Any<RegistryValueType>());
    }

    [Test]
    public async Task GetAll_NullCommandValue_DefaultsToEmpty()
    {
        _registry.EnumerateValues(HkcuRunKey).Returns(new List<RegistryValueEntry>
        {
            new("NullApp", null!, RegistryValueType.String),
        });

        var result = await _service.GetAllAsync();

        var entry = result.First(e => e.Name == "NullApp");
        Assert.That(entry.Command, Is.EqualTo(string.Empty));
    }
}
