using System;
using System.Threading.Tasks;

class Program
{
    private static DirectoryMover directoryMover;

    static async Task Main(string[] args)
    {
        var sourceDirectory = @"C:\source"; // Specify the source directory path
        var destinationDirectory = @"C:\destination"; // Specify the destination directory path

        try
        {
            // Call the method to start directory monitoring
            await StartDirectoryMonitoringAsync(sourceDirectory, destinationDirectory);

            // Call the method to cleanup directories after monitoring
            await CleanupAsync();
        }
        catch (Exception ex)
        {
            // Handle the exception (log, retry, or inform the user)
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Method to handle starting the directory monitoring process.
    /// Accepts source and destination directories as parameters.
    /// </summary>
    /// <param name="sourceDirectory">The source directory to monitor</param>
    /// <param name="destinationDirectory">The destination directory to move subdirectories to</param>
    private static async Task StartDirectoryMonitoringAsync(string sourceDirectory, string destinationDirectory)
    {
        // Instantiate DirectoryMover with the given source and destination directories
        directoryMover = new DirectoryMover(sourceDirectory, destinationDirectory);

        // Start monitoring asynchronously
        var monitorTask = directoryMover.StartMonitoringAsync();

        // Simulate some work (e.g., run monitoring for 20 seconds)
        await Task.Delay(20000);

        // Stop monitoring
        directoryMover.StopMonitoring();

        // Wait for the monitoring task to fully complete
        await monitorTask;
    }

    /// <summary>
    /// Method to handle cleaning up any remaining directories and deleting the source directory
    /// </summary>
    private static async Task CleanupAsync()
    {
        // Perform cleanup asynchronously
        await directoryMover.CleanUpAsync();
    }
}
