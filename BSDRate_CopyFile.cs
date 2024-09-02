using System;
using System.IO;

public void Main()
{
    // Retrieve the variables from SSIS package
    string sSource = Dts.Variables["User::sSource"].Value.ToString();
    string sDestination = Dts.Variables["User::sDestination"].Value.ToString();
    
    // Validate that source file exists
    if (!File.Exists(sSource))
    {
        Dts.Events.FireError(0, "File Move Script", "Source file does not exist: " + sSource, string.Empty, 0);
        Dts.TaskResult = (int)ScriptResults.Failure;
        return;
    }
    
    // Extract the directory part of the destination path
    string destinationDirectory = Path.GetDirectoryName(sDestination);
    
    // Validate that destination directory exists
    if (!Directory.Exists(destinationDirectory))
    {
        Dts.Events.FireError(0, "File Move Script", "Destination directory does not exist: " + destinationDirectory, string.Empty, 0);
        Dts.TaskResult = (int)ScriptResults.Failure;
        return;
    }
    
    // Append the current date to the destination file name
    string fileName = Path.GetFileNameWithoutExtension(sDestination);
    string extension = Path.GetExtension(sDestination);
    string dateSuffix = DateTime.Now.ToString("yyyyMMdd");
    string destinationFileName = $"{fileName}_{dateSuffix}{extension}";
    string destinationFilePath = Path.Combine(destinationDirectory, destinationFileName);
    
    try
    {
        // Move the file
        File.Move(sSource, destinationFilePath);
        Dts.TaskResult = (int)ScriptResults.Success;
    }
    catch (Exception ex)
    {
        Dts.Events.FireError(0, "File Move Script", "An error occurred while moving the file: " + ex.Message, string.Empty, 0);
        Dts.TaskResult = (int)ScriptResults.Failure;
    }
}
