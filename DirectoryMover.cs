using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace util;

public class DirectoryMover
{
    private readonly string _sourceDirectory;
    private readonly string _destinationDirectory;
    private readonly int _interval;

    private readonly int _numberofDirectoriesToMove = 3;
    private CancellationTokenSource _cancellationTokenSource;

    private Task _processingTask;

    public DirectoryMover(string sourceDirectory, string destinationDirectory, int intervalSeconds = 10)
    {
        _sourceDirectory = sourceDirectory ?? throw new ArgumentNullException(nameof(sourceDirectory));
        _destinationDirectory = destinationDirectory ?? throw new ArgumentNullException(nameof(destinationDirectory));
        _interval = TimeSpan.FromSeconds(intervalSeconds);
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
                _processingTask = CheckAndMoveDirectoriesAsync();

                await _processingTask;

                await Task.Delay(_interval, _cancellationTokenSource.Token);
            }
        }
        catch (TaskCanceledException)
        {
            // Do nothing; this is expected when stopping monitoring
        }
    }

    // Stop monitoring when requested
    public async Task StopMonitoringAsync()
    {
         _cancellationTokenSource.Cancel();

        if (_processingTask != null)
        {
            // Cancel the ongoing task
            try
            {
                // Wait for the task to complete
                await _processingTask;
            }
            catch (OperationCanceledException)
            {
                // Handle the task cancellation if needed
                Console.WriteLine("Monitoring task was canceled.");
            }
            finally
            {
                _processingTask = null;
            }
        }

        _cancellationTokenSource.Dispose();



    }

    private async Task CheckAndMoveDirectoriesAsync()
    {
        if (!Directory.Exists(_sourceDirectory))
            return;

        var directories = new DirectoryInfo(_sourceDirectory)
            .GetDirectories()
            .Where(d => !d.Name.Contains("Error", StringComparison.OrdinalIgnoreCase))
            .OrderBy(d => d.CreationTime)
            .ToList();

        if (directories.Count > _numberofDirectoriesToMove)
        {
            var directoriesToMove = directories.Take(_numberofDirectoriesToMove).ToList();
            foreach (var dir in directoriesToMove)
            {
                // Check for cancellation before starting the move
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                string destinationPath = Path.Combine(_destinationDirectory, dir.Name);

                if (!Directory.Exists(_destinationDirectory))
                    Directory.CreateDirectory(_destinationDirectory);

                try
                {
                    MoveDirectory(dir.FullName, destinationPath);
                }
                catch (OperationCanceledException)
                {
                    // Handle cancellation here by removing the partially copied directory
                    if (Directory.Exists(destinationPath))
                    {
                        try
                        {
                            Directory.Delete(destinationPath, true); // True indicates recursive delete
                        }
                        catch (Exception deleteEx)
                        {
                            // do a throw here
                            throw new Exception($"Error deleting partial directory '{destinationPath}': {deleteEx.Message}");

                        }
                    }

                    // Re-throw the cancellation exception to propagate it
                    throw;
                }
                catch (IOException ex)
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
                MoveDirectory(dir.FullName, destinationPath);
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

    private static void MoveDirectory(string sourcePath, string targetPath)
    {
        if(sourcePath.IndexOf("PDF23") > 0)
        {
            string hold;

            hold = "bob";
        }            

        DirectoryInfo source = new DirectoryInfo(sourcePath);
        DirectoryInfo target = new DirectoryInfo(targetPath);

        MoveDirectory(source,target);
        
    }
    private static void MoveDirectory(DirectoryInfo source, DirectoryInfo target)
    {
        // Ensure target directory exists
        if (!Directory.Exists(target.FullName))
        {
            Directory.CreateDirectory(target.FullName);
        }

        try
        {
            // Copy each file with retry logic to handle network drive errors
            foreach (FileInfo file in source.GetFiles())
            {
                CopyFileWithRetry(file, target.FullName);
            }

            // Recursively copy subdirectories with retry logic
            foreach (DirectoryInfo subdir in source.GetDirectories())
            {
                DirectoryInfo targetSubdir = target.CreateSubdirectory(subdir.Name);
                MoveDirectory(subdir, targetSubdir);
            }
        }
        catch (Exception ex)
        {
            throw new IOException($"Error copying directory '{source.FullName}' to '{target.FullName}': {ex.Message}", ex);
        }

        // After all files and directories are copied, delete the source directory
        try
        {
            source.Delete(true);
        }
        catch (Exception ex)
        {
            throw new IOException($"Error deleting source directory '{source.FullName}': {ex.Message}", ex);
        }
    }

    // Retry logic for file copy (useful for network drives)
    private static void CopyFileWithRetry(FileInfo file, string destinationDirectory, int maxRetries = 3, int delayMilliseconds = 1000)
    {
        string destinationPath = Path.Combine(destinationDirectory, file.Name);
        int attempt = 0;
        bool success = false;

        while (attempt < maxRetries && !success)
        {
            try
            {
                file.CopyTo(destinationPath, true);
                success = true;
            }
            catch (IOException ioEx) when (IsNetworkError(ioEx))
            {
                attempt++;
                if (attempt >= maxRetries)
                {
                    throw new IOException($"Failed to copy file '{file.FullName}' to '{destinationPath}' after {maxRetries} attempts: {ioEx.Message}", ioEx);
                }
                Thread.Sleep(delayMilliseconds); // Wait before retrying
            }
        }
    }

    // A method to detect network-related exceptions, adjust as needed for your use case
    private static bool IsNetworkError(IOException ex)
    {
        // Simplified check for network-related IO exceptions
        return ex.Message.Contains("network") || ex.Message.Contains("unreachable") || ex.Message.Contains("path");
    }

}
