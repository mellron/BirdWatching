using System;

public void Main()
{
    // Retrieve variables from SSIS package
    string scheduledTime = Dts.Variables["User::ScheduledTime"].Value.ToString(); // Format: "HH:mm"
    string dayOfTheWeek = Dts.Variables["User::DayOfTheWeek"].Value.ToString();  // Values: "All", "Mo", "Tu", "We", "Th", "Fr", or combinations like "MoTu"

    // Parse the scheduled time in "HH:mm" format
    TimeSpan scheduledTimeSpan = TimeSpan.ParseExact(scheduledTime, "hh\\:mm", null);
    TimeSpan currentTimeSpan = DateTime.Now.TimeOfDay;

    // Get the current day of the week
    string currentDay = DateTime.Now.ToString("ddd"); // "Mon", "Tue", etc.
    currentDay = currentDay.Substring(0, 2); // "Mo", "Tu", etc.

    // Check if the current day is in the specified DayOfTheWeek or if it's set to "All"
    bool isDayMatch = dayOfTheWeek.Equals("All", StringComparison.OrdinalIgnoreCase) || dayOfTheWeek.Contains(currentDay);

    // Check if the current time is past or equal to the scheduled time
    bool isTimeMatch = currentTimeSpan >= scheduledTimeSpan;

    // Decide whether to execute the copy script or skip
    if (isDayMatch && isTimeMatch)
    {
        Dts.Variables["User::ShouldExecuteCopy"].Value = true;
        Dts.Events.FireInformation(0, "Check DateTime Script", "Conditions met. Proceeding with file copy.", string.Empty, 0);
    }
    else
    {
        Dts.Variables["User::ShouldExecuteCopy"].Value = false;
        Dts.Events.FireInformation(0, "Check DateTime Script", "Conditions not met. Skipping file copy.", string.Empty, 0);
    }

    Dts.TaskResult = (int)ScriptResults.Success;
}
