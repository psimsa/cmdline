using System.Diagnostics;
using System.Reflection;

var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "'unknown'";
Console.WriteLine($"{Assembly.GetExecutingAssembly().GetName().Name} version {version}");

string globalSwitch = "";
switch (args.Length)
{
    case 0:
        globalSwitch = "";
        break;
    case 1 when args[0] == "--global" || args[0] == "-g":
        globalSwitch = "-g";
        break;
    default:
        Console.WriteLine("Usage: dotnet updatetools [-g|--global]");
        Environment.Exit(1);
        break;
}

var outputLines = new List<string>();

var getToolsListProcess = new Process()
{
    StartInfo =
    {
        FileName = "dotnet",
        Arguments = $"tool list {globalSwitch}",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = false,
        CreateNoWindow = true
    }
};

getToolsListProcess.OutputDataReceived += (sender, e) =>
{
    if (e.Data != null)
    {
        outputLines.Add(e.Data);
    }
};

getToolsListProcess.Start();
getToolsListProcess.BeginOutputReadLine();
getToolsListProcess.WaitForExit();

Parallel.ForEach(
    outputLines.Skip(2).Select(l => l.Split(' ', 2)[0]).OrderBy(t => t),
    new ParallelOptions() {MaxDegreeOfParallelism = 4},
    tool =>
    {
        var updateProcess = new Process()
        {
            StartInfo =
            {
                FileName = "dotnet",
                Arguments = $"tool update {tool} {globalSwitch}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        updateProcess.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine(e.Data);
            }
        };

        updateProcess.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine($"Error updating {tool}:{Environment.NewLine}\t{e.Data}");
            }
        };
        updateProcess.Start();
        updateProcess.BeginOutputReadLine();
        updateProcess.BeginErrorReadLine();

        var exited = updateProcess.WaitForExit(60_000);
        if (!exited) updateProcess.Kill(true);
    });