/*using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class LogManager : MonoBehaviour
{
    private static string logPath;

    private void Start()
    {
        logPath = Application.dataPath + "/../latest.log";
    }

    void OnEnable()
    {
        //Clear Log
        File.Delete(logPath);
        
        Application.logMessageReceived += Log;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    private static void Log(string logEntry, string stackTrace, LogType type)
    {
        TextWriter writer = new StreamWriter(logPath, true);

        //Always include time and log type
        logEntry = System.DateTime.Now.ToShortTimeString() + "[" + type + "] " + logEntry;
        
        //If message isn't a log, include stack trace
        if (type != LogType.Log)
            logEntry = logEntry + Environment.NewLine + "stacktrace: " + stackTrace;

        writer.WriteLine(logEntry);
        writer.Close();
    }
}
*/