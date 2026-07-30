using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Domain.Models;

namespace StudyBuddy.Infrastructure.Evaluation;

/// <summary>
/// Disk-backed eval history store. Each run is a timestamped JSON file under
/// <c>eval-history/</c> so results survive backend restarts.
/// </summary>
public sealed class FileEvalResultStore : IEvalResultStore
{
    public const string HistoryFolderName = "eval-history";
    public const int DefaultHistoryCount = 20;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly object _gate = new();
    private readonly string _historyDirectory;

    public FileEvalResultStore(IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);
        _historyDirectory = DiskEvalReportWriter.ResolveRepoRelativePath(hostEnvironment, HistoryFolderName);
        Directory.CreateDirectory(_historyDirectory);
    }

    public void Save(EvalRunResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var fileName = $"eval-run-{result.RunAt.UtcDateTime:yyyyMMdd'T'HHmmssfff'Z'}.json";
        var path = Path.Combine(_historyDirectory, fileName);

        lock (_gate)
        {
            Directory.CreateDirectory(_historyDirectory);
            var json = JsonSerializer.Serialize(result, JsonOptions);
            File.WriteAllText(path, json);
        }
    }

    public EvalRunResult? GetLatest()
    {
        lock (_gate)
        {
            var latestPath = EnumerateResultFiles()
                .OrderByDescending(f => f, StringComparer.Ordinal)
                .FirstOrDefault();

            return latestPath is null ? null : ReadFile(latestPath);
        }
    }

    public IReadOnlyList<EvalRunResult> GetHistory(int count = DefaultHistoryCount)
    {
        if (count <= 0)
        {
            return [];
        }

        lock (_gate)
        {
            var results = new List<EvalRunResult>();
            foreach (var path in EnumerateResultFiles()
                .OrderByDescending(f => f, StringComparer.Ordinal)
                .Take(count))
            {
                var result = ReadFile(path);
                if (result is not null)
                {
                    results.Add(result);
                }
            }

            return results;
        }
    }

    private IEnumerable<string> EnumerateResultFiles()
    {
        if (!Directory.Exists(_historyDirectory))
        {
            return [];
        }

        return Directory.EnumerateFiles(_historyDirectory, "eval-run-*.json", SearchOption.TopDirectoryOnly);
    }

    private static EvalRunResult? ReadFile(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<EvalRunResult>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }
}
