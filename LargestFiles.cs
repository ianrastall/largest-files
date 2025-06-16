/*
 * LargestFilesFinder.cs – Modern Edition
 * --------------------------------------
 * Build (requires .NET 8 SDK):
 * dotnet build -c Release
 * Run:
 * LargestFilesFinder.exe [--json]
 *
 * Description
 * ==========
 * Enumerates every fixed or removable drive that is currently mounted and
 * reports the 50 largest regular files found on each drive. Results are
 * written next to the executable in either human-readable text or JSON
 * (if --json is supplied).
 *
 * Modern and Secure Design
 * ========================
 * • **Modern APIs** Uses high-performance .NET APIs like DirectoryInfo.EnumerateFiles
 * and PriorityQueue, which are more efficient and safer than raw Win32 calls.
 * • **Long Path Support** Automatically supports Windows long file paths
 * (> 260 chars) without needing special prefixes, ensuring all files are found.
 * • **Error Hygiene** Gracefully handles and skips directories that cannot be
 * accessed, preventing a single permission issue from halting the entire scan.
 * • **Efficient Collections** Employs a PriorityQueue to maintain the "Top N"
 * list with optimal performance, minimizing memory and CPU usage.
 * • **Dependencies** Pure BCL only; no external NuGet packages.
 * • **Compiler Hardening** Nullable reference types ON, deterministic build,
 * TreatWarningsAsErrors, and C# latest.
 */

#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed record FileEntry(string Path, ulong Size);

internal static class LargestFilesFinder
{
    /* ======= Public tunables ================= */
    private const int TopN = 50;
    private const string TextFile = "largest_files.txt";
    private const string JsonFile = "largest_files.json";

    /* ======= Records and helpers ============= */
    private static readonly string _baseDir = AppContext.BaseDirectory;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = AppJsonSerializerContext.Default
    };

    /* ======= Entry point ===================== */
    internal static int Main(string[] args)
    {
        bool asJson = args.Length > 0 && args[0].Equals("--json", StringComparison.OrdinalIgnoreCase);
        string outPath = Path.Combine(_baseDir, asJson ? JsonFile : TextFile);

        try
        {
            File.WriteAllText(outPath, string.Empty);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Cannot create output file: {e.Message}");
            return 1;
        }

        Console.WriteLine("[LargestFilesFinder – Modern Edition]\n");
        var finalResults = new Dictionary<string, List<FileEntry>>(StringComparer.OrdinalIgnoreCase);

        foreach (string drive in EnumerateEligibleDrives())
        {
            Console.WriteLine($"Scanning {drive} …");
            var sw = Stopwatch.StartNew();

            List<FileEntry> largest = GetLargestFilesOnDrive(drive);
            finalResults[drive[..2]] = largest;

            sw.Stop();
            Console.WriteLine($"  {largest.Count} items collected in {sw.Elapsed.TotalSeconds:F1}s\n");
        }

        WriteResults(outPath, finalResults, asJson);
        Console.WriteLine($"Done. Output => {outPath}");
        return 0;
    }

    /* ======= Core logic ====================== */
    private static IEnumerable<string> EnumerateEligibleDrives()
    {
        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable))
            {
                yield return drive.Name;
            }
        }
    }

    private static List<FileEntry> GetLargestFilesOnDrive(string root)
    {
        var largestFiles = new PriorityQueue<FileEntry, ulong>(TopN + 1);

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,
            IgnoreInaccessible = true
        };

        try
        {
            // FIX: Use DirectoryInfo.EnumerateFiles(), which is the correct public API
            // for creating a file enumerator. This resolves the CS0144 error.
            var files = new DirectoryInfo(root).EnumerateFiles("*", options);

            foreach (var file in files)
            {
                var current = new FileEntry(file.FullName, (ulong)file.Length);

                if (largestFiles.Count < TopN)
                {
                    largestFiles.Enqueue(current, current.Size);
                }
                else if (current.Size > largestFiles.Peek().Size)
                {
                    largestFiles.Dequeue();
                    largestFiles.Enqueue(current, current.Size);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"! Failed to fully scan {root}. Reason: {ex.Message}");
        }

        return largestFiles.UnorderedItems.OrderByDescending(e => e.Element.Size).Select(e => e.Element).ToList();
    }

    /* ======= Output ========================== */
    private static void WriteResults(string outPath, IReadOnlyDictionary<string, List<FileEntry>> data, bool asJson)
    {
        try
        {
            using StreamWriter w = new(outPath);
            if (asJson)
            {
                string json = JsonSerializer.Serialize(data, _jsonOptions);
                w.Write(json);
            }
            else
            {
                foreach (var (drive, files) in data)
                {
                    w.WriteLine($"Largest {files.Count} files on {drive}:");
                    if (files.Any())
                    {
                        foreach (var f in files)
                            w.WriteLine($"  {FormatBytes(f.Size),9} – {f.Path}");
                    }
                    else
                    {
                        w.WriteLine("  (No files found)");
                    }
                    w.WriteLine();
                }
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to write output file: {e.Message}");
        }
    }

    private static string FormatBytes(ulong bytes)
    {
        const int scale = 1024;
        string[] units = { "B", "KB", "MB", "GB", "TB", "PB" };
        int unitIndex = 0;
        double value = bytes;

        while (value >= scale && unitIndex < units.Length - 1)
        {
            value /= scale;
            unitIndex++;
        }
        return $"{value:0.##} {units[unitIndex]}";
    }
}

[JsonSerializable(typeof(IReadOnlyDictionary<string, List<FileEntry>>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}