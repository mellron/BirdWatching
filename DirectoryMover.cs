using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class DirectoryMover
{
    private readonly string _sourceDirectory;
    private readonly string _destinationDirectory;
    private readonly int _interval;
    private CancellationTokenSource _cancellationTokenSource;

    public DirectoryMover(string sourceDirectory, string destinationDirectory, int intervalSeconds = 5)
    {
        _sourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory));
        _destinationDirectory = destinationDirectory ?? throw new ArgumentNullException(nameof(destinationDirectory));
        _interval = intervalSeconds * 1000; // Convert to milliseconds
        _cancellationTokenSource = new CancellationTokenSource();
    }

    // Start monitoring for directory changes
    public async Task StartMonitoringAsync()
    {
        if (!Directory.Exists(_sourceDirectory))
            throw new DirectoryNotFoundException($"Source directory '{_sourceDirectory}' does not exist.");

        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await CheckAndMoveDirectoriesAsync();
                await Task.Delay(_interval);
            }
        }
        catch (TaskCanceledException)
        {
            // Do nothing; this is expected when stopping monitoring
        }
    }

    // Stop monitoring when requested
    public void StopMonitoring()
    {
        _cancellationTokenSource.Cancel();
    }

    // Check and move the latest 3 subdirectories to the destination
    private async Task CheckAndMoveDirectoriesAsync()
    {
        if (!Directory.Exists(_sourceDirectory))
            throw new DirectoryNotFoundException($"Source directory '{_sourceDirectory}' does not exist.");

        // Get all subdirectories sorted by creation time (descending)
        var directories = new DirectoryInfo(_sourceDirectory)
            .GetDirectories()
            .OrderByDescending(d => d.CreationTime)
            .ToList();

        // Only move if there are more than 3 subdirectories
        if (directories.Count > 3)
        {
            // Select the latest 3 subdirectories
            var directoriesToMove = directories.Take(3).ToList();

            foreach (var dir in directoriesToMove)
            {
                string destinationPath = Path.Combine(_destinationDirectory, dir.Name);

                if (!Directory.Exists(_destinationDirectory))
                {
                    throw new DirectoryNotFoundException($"Destination directory '{_destinationDirectory}' does not exist.");
                }

                try
                {
                    // Move the directory
                    Directory.Move(dir.FullName, destinationPath);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Error moving directory '{dir.FullName}' to '{destinationPath}': {ex.Message}", ex);
                }
            }
        }

        await Task.CompletedTask;
    }

    // Clean-up method to move remaining directories and delete source
    public async Task CleanUpAsync()
    {
        if (!Directory.Exists(_sourceDirectory))
            throw new DirectoryNotFoundException($"Source directory '{_sourceDirectory}' does not exist.");

        // Get all remaining subdirectories
        var remainingDirectories = new DirectoryInfo(_sourceDirectory).GetDirectories();

        // Move all remaining subdirectories
        foreach (var dir in remainingDirectories)
        {
            string destinationPath = Path.Combine(_destinationDirectory, dir.Name);

            if (!Directory.Exists(_destinationDirectory))
            {
                throw new DirectoryNotFoundException($"Destination directory '{_destinationDirectory}' does not exist.");
            }

            try
            {
                Directory.Move(dir.FullName, destinationPath);
            }
            catch (Exception ex)
            {
                throw new IOException($"Error moving directory '{dir.FullName}' to '{destinationPath}': {ex.Message}", ex);
            }
        }

        // Delete the source directory if empty
        try
        {
            if (!Directory.EnumerateFileSystemEntries(_sourceDirectory).Any())
            {
                Directory.Delete(_sourceDirectory);
            }
        }
        catch (Exception ex)
        {
            throw new IOException($"Error deleting source directory '{_sourceDirectory}': {ex.Message}", ex);
        }

        await Task.CompletedTask;
    }
}
