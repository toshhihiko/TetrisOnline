using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDebugLog : MonoBehaviour
{
    [SerializeField]Text _window;

    private void Awake()
    {
        Application.logMessageReceived += OnLogMessage;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived += OnLogMessage;
    }

    // Display log messages on Text.UI object
    private void OnLogMessage(string logText, string stackTrace, LogType type)
    {
        if (string.IsNullOrEmpty(logText)) 
            return;

        // Limit total amount of log messages.
        if (_window.text.Length > 5000)
            _window.text = _window.text.Substring(0, 1000);

        _window.text = logText + stackTrace + System.Environment.NewLine + _window.text;
    }
}
