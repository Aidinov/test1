using System.Diagnostics;

namespace DesignDocs.Api.Services;

public class GitCliProvider
{
    private readonly string _cacheDir;
    private readonly string _repoUrl;
    private readonly string _defaultBranch;

    public GitCliProvider(IConfiguration config)
    {
        _cacheDir = config["GIT_LOCAL_CACHE"] ?? "/tmp/git-cache";
        _repoUrl = config["GITLAB_PROJECT_HTTP_URL"] ?? string.Empty;
        _defaultBranch = config["GIT_DEFAULT_BRANCH"] ?? "main";
    }

    private ProcessStartInfo Psi(string args) => new("git") { ArgumentList = { } };

    public async Task<string> ExecAsync(string workDir, string args)
    {
        var psi = new ProcessStartInfo("git", args)
        {
            WorkingDirectory = workDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        var proc = Process.Start(psi)!;
        var output = await proc.StandardOutput.ReadToEndAsync();
        var err = await proc.StandardError.ReadToEndAsync();
        proc.WaitForExit();
        if (proc.ExitCode != 0) throw new Exception($"git {args} failed: {err}");
        return output;
    }

    public async Task<string> EnsureRepoAsync()
    {
        if (!Directory.Exists(_cacheDir))
            Directory.CreateDirectory(_cacheDir);
        var repoPath = Path.Combine(_cacheDir, "repo");
        if (!Directory.Exists(Path.Combine(repoPath, ".git")))
        {
            await ExecAsync(_cacheDir, $"clone {_repoUrl} repo");
        }
        await ExecAsync(repoPath, "fetch --all");
        await ExecAsync(repoPath, $"checkout {_defaultBranch}");
        await ExecAsync(repoPath, "pull");
        return repoPath;
    }
}
