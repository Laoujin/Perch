using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Perch.Cli.Commands;

public sealed class CompletionCommand : AsyncCommand<CompletionCommand.Settings>
{
    private readonly IAnsiConsole _console;

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<shell>")]
        [Description("Shell type (bash, zsh, or powershell)")]
        public string Shell { get; init; } = "";
    }

    public CompletionCommand(IAnsiConsole console)
    {
        _console = console;
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        string script = settings.Shell.ToLowerInvariant() switch
        {
            "bash" => BashScript,
            "zsh" => ZshScript,
            "powershell" or "pwsh" => PowerShellScript,
            _ => "",
        };

        if (string.IsNullOrEmpty(script))
        {
            _console.MarkupLine($"[red]Error:[/] Unsupported shell '{settings.Shell.EscapeMarkup()}'. Use bash, zsh, or powershell.");
            return Task.FromResult(1);
        }

        _console.WriteLine(script);
        return Task.FromResult(0);
    }

    internal const string BashScript = """
        _perch_completions() {
            local cur="${COMP_WORDS[COMP_CWORD]}"
            local prev="${COMP_WORDS[COMP_CWORD-1]}"

            local commands="deploy status apps restore git diff completion"

            case "$prev" in
                perch)
                    COMPREPLY=($(compgen -W "$commands" -- "$cur"))
                    return
                    ;;
                git)
                    COMPREPLY=($(compgen -W "setup" -- "$cur"))
                    return
                    ;;
                diff)
                    COMPREPLY=($(compgen -W "start stop" -- "$cur"))
                    return
                    ;;
                restore)
                    COMPREPLY=($(compgen -W "list apply" -- "$cur"))
                    return
                    ;;
                completion)
                    COMPREPLY=($(compgen -W "bash zsh powershell" -- "$cur"))
                    return
                    ;;
                deploy)
                    COMPREPLY=($(compgen -W "--config-path --dry-run --output --interactive" -- "$cur"))
                    return
                    ;;
                status)
                    COMPREPLY=($(compgen -W "--config-path --output" -- "$cur"))
                    return
                    ;;
                apps)
                    COMPREPLY=($(compgen -W "--config-path --output --unmanaged" -- "$cur"))
                    return
                    ;;
                setup)
                    COMPREPLY=($(compgen -W "--config-path --dry-run" -- "$cur"))
                    return
                    ;;
                start|stop)
                    COMPREPLY=($(compgen -W "--config-path" -- "$cur"))
                    return
                    ;;
                list|apply)
                    COMPREPLY=($(compgen -W "--config-path" -- "$cur"))
                    return
                    ;;
            esac

            if [[ "$cur" == -* ]]; then
                COMPREPLY=($(compgen -W "--config-path --dry-run --output --interactive --unmanaged" -- "$cur"))
            else
                COMPREPLY=($(compgen -W "$commands" -- "$cur"))
            fi
        }

        complete -F _perch_completions perch
        """;

    internal const string ZshScript = """
        #compdef perch

        _perch() {
            local -a commands
            commands=(
                'deploy:Deploy managed configs by creating symlinks'
                'status:Check for drift between managed configs and deployed symlinks'
                'apps:Show installed apps and their config module status'
                'restore:Restore files from pre-deploy snapshots'
                'git:Git-related commands'
                'diff:Diff-related commands'
                'completion:Generate shell completion script'
            )

            local -a git_commands
            git_commands=('setup:Register git clean filters defined in module manifests')

            local -a diff_commands
            diff_commands=(
                'start:Capture a filesystem snapshot for change detection'
                'stop:Compare current state against the captured snapshot'
            )

            local -a restore_commands
            restore_commands=(
                'list:List available pre-deploy snapshots'
                'apply:Restore files from a pre-deploy snapshot'
            )

            _arguments -C \
                '1:command:->command' \
                '*::arg:->args'

            case "$state" in
                command)
                    _describe 'command' commands
                    ;;
                args)
                    case "${words[1]}" in
                        deploy)
                            _arguments \
                                '--config-path[Path to the config repository]:path:_files' \
                                '--dry-run[Preview changes without making them]' \
                                '--output[Output format (Pretty or Json)]:format:(Pretty Json)' \
                                '--interactive[Prompt before deploying each module]'
                            ;;
                        status)
                            _arguments \
                                '--config-path[Path to the config repository]:path:_files' \
                                '--output[Output format (Pretty or Json)]:format:(Pretty Json)'
                            ;;
                        apps)
                            _arguments \
                                '--config-path[Path to the config repository]:path:_files' \
                                '--output[Output format (Pretty or Json)]:format:(Pretty Json)' \
                                '--unmanaged[Show unmanaged apps]'
                            ;;
                        git)
                            _describe 'git command' git_commands
                            ;;
                        diff)
                            _describe 'diff command' diff_commands
                            ;;
                        restore)
                            _describe 'restore command' restore_commands
                            ;;
                        completion)
                            _arguments '1:shell:(bash zsh powershell)'
                            ;;
                    esac
                    ;;
            esac
        }

        _perch "$@"
        """;

    internal const string PowerShellScript = """
        Register-ArgumentCompleter -Native -CommandName perch -ScriptBlock {
            param($wordToComplete, $commandAst, $cursorPosition)

            $commands = @{
                'deploy'     = @('--config-path', '--dry-run', '--output', '--interactive')
                'status'     = @('--config-path', '--output')
                'apps'       = @('--config-path', '--output', '--unmanaged')
                'restore'    = @('list', 'apply')
                'git'        = @('setup')
                'diff'       = @('start', 'stop')
                'completion' = @('bash', 'zsh', 'powershell')
            }

            $subcommands = @{
                'setup' = @('--config-path', '--dry-run')
                'start' = @('--config-path')
                'stop'  = @('--config-path')
                'list'  = @('--config-path')
                'apply' = @('--config-path')
            }

            $tokens = $commandAst.ToString().Split(' ', [StringSplitOptions]::RemoveEmptyEntries)

            if ($tokens.Count -le 1 -or ($tokens.Count -eq 2 -and $wordToComplete)) {
                $commands.Keys | Where-Object { $_ -like "$wordToComplete*" } | ForEach-Object {
                    [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
                }
                return
            }

            $command = $tokens[1]

            if ($tokens.Count -eq 2 -and -not $wordToComplete) {
                $opts = if ($commands.ContainsKey($command)) { $commands[$command] } else { @() }
                $opts | Where-Object { $_ -like "$wordToComplete*" } | ForEach-Object {
                    [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
                }
                return
            }

            if ($tokens.Count -ge 3) {
                $sub = $tokens[2]
                if ($subcommands.ContainsKey($sub)) {
                    $subcommands[$sub] | Where-Object { $_ -like "$wordToComplete*" } | ForEach-Object {
                        [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
                    }
                    return
                }
            }

            $opts = if ($commands.ContainsKey($command)) { $commands[$command] } else { @() }
            $opts | Where-Object { $_ -like "$wordToComplete*" } | ForEach-Object {
                [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
            }
        }
        """;
}
