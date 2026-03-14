using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class PerfGetBenchmarks
{
    private Process? webApiProcess;
    private HttpClient? http;
    private Task<string>? webApiStdOutTask;
    private Task<string>? webApiStdErrTask;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        var port = GetFreeTcpPort();
        webApiProcess = StartWebApi(port);
        http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}") };

        await WaitUntilReadyAsync(http, webApiProcess, TimeSpan.FromSeconds(45));
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        http?.Dispose();
        http = null;

        if (webApiProcess is { HasExited: false })
        {
            try { webApiProcess.Kill(entireProcessTree: true); } catch { }
        }

        webApiProcess?.Dispose();
        webApiProcess = null;
        webApiStdOutTask = null;
        webApiStdErrTask = null;
    }

    [Benchmark]
    public Task<byte[]> Get_100() => http!.GetByteArrayAsync("/api/perf/items?count=100");

    [Benchmark]
    public Task<byte[]> Get_1000() => http!.GetByteArrayAsync("/api/perf/items?count=1000");

    [Benchmark]
    public Task<byte[]> Get_10000() => http!.GetByteArrayAsync("/api/perf/items?count=10000");

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    private Process StartWebApi(int port)
    {
        var repoRoot = FindRepoRoot(AppContext.BaseDirectory);
        var projectPath = Path.Combine(repoRoot, "WebApi", "WebApi.csproj");
        var dotnetHome = Path.Combine(repoRoot, ".dotnet-home");

        if (!File.Exists(projectPath))
        {
            throw new FileNotFoundException("WebApi.csproj not found.", projectPath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --no-build -c Release --project \"{projectPath}\" --urls http://127.0.0.1:{port}",
            WorkingDirectory = repoRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Benchmark";
        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Benchmark";
        startInfo.Environment["DOTNET_CLI_HOME"] = dotnetHome;
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start WebApi.");
        webApiStdOutTask = process.StandardOutput.ReadToEndAsync();
        webApiStdErrTask = process.StandardError.ReadToEndAsync();
        return process;
    }

    private async Task WaitUntilReadyAsync(HttpClient http, Process process, TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            if (process.HasExited)
            {
                await ThrowStartupFailureAsync(process);
            }

            try
            {
                using var resp = await http.GetAsync("/api/perf/items?count=1");
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
            }
            catch
            {
                // ignore while booting
            }

            await Task.Delay(150);
        }

        if (process.HasExited)
        {
            await ThrowStartupFailureAsync(process);
        }

        throw new TimeoutException("WebApi did not become ready in time.");
    }

    private static string FindRepoRoot(string startPath)
    {
        var dir = new DirectoryInfo(startPath);
        for (var i = 0; i < 12 && dir is not null; i++)
        {
            var webApiProject = Path.Combine(dir.FullName, "WebApi", "WebApi.csproj");
            if (File.Exists(webApiProject))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException($"Could not locate repo root starting from '{startPath}'.");
    }

    private async Task ThrowStartupFailureAsync(Process process)
    {
        var stdout = webApiStdOutTask is null ? string.Empty : await webApiStdOutTask;
        var stderr = webApiStdErrTask is null ? string.Empty : await webApiStdErrTask;

        throw new InvalidOperationException(
            $"WebApi exited during startup (ExitCode={process.ExitCode}).\nSTDERR:\n{Tail(stderr, 50)}\nSTDOUT:\n{Tail(stdout, 20)}");
    }

    private static string Tail(string text, int maxLines)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var lines = text.Replace("\r\n", "\n").Split('\n');
        var start = Math.Max(0, lines.Length - maxLines);
        return string.Join(Environment.NewLine, lines[start..]);
    }
}
