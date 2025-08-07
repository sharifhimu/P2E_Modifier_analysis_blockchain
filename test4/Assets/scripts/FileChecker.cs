using UnityEngine;
using System;
using System.IO;

public class FileChecker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static bool AlreadySavedToday(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length < 2)
            return false;

        string lastLine = lines[^1];
        string[] parts = lastLine.Split(',');

        if (DateTime.TryParse(parts[0], out DateTime lastSavedTime))
        {
            // Compare using local date
            return lastSavedTime.Date == DateTime.Now.Date;
        }

        return false;
    }

}
