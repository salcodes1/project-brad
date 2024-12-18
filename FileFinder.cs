using Godot;
using System.IO;

public static class FileFinder
{
    public static string FindFileUpwards(string fileName)
    {
        // Get the full path to the currently running executable
        string execPath = OS.GetExecutablePath();
        // Extract the directory portion from the executable path
        string dirPath = Path.GetDirectoryName(execPath);

        while (!string.IsNullOrEmpty(dirPath))
        {
            string candidate = Path.Combine(dirPath, fileName);
            if (File.Exists(candidate))
                return candidate;

            // Move one level up
            string parent = Directory.GetParent(dirPath)?.FullName;
            // If there's no parent or we didn't actually move up, break
            if (parent == null || parent == dirPath)
                break;

            dirPath = parent;
        }

        // File not found
        return string.Empty;
    }

    public static string FindDirectoryUpwards(string directoryName)
    {
        // Get the full path to the currently running executable
        string execPath = OS.GetExecutablePath();
        // Extract the directory portion from the executable path
        string dirPath = Path.GetDirectoryName(execPath);

        while (!string.IsNullOrEmpty(dirPath))
        {
            string candidate = Path.Combine(dirPath, directoryName);
            if (Directory.Exists(candidate))
                return candidate;

            // Move one level up
            string parent = Directory.GetParent(dirPath)?.FullName;
            // If there's no parent or we didn't actually move up, break
            if (parent == null || parent == dirPath)
                break;

            dirPath = parent;
        }

        // Directory not found
        return string.Empty;
    }
}