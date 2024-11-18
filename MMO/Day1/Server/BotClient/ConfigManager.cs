using System;
using System.Collections.Generic;
using System.IO;

public class ConfigManager
{
    private static ConfigManager _instance;
    private Dictionary<string, string> _config;
    private string _configFile;
    private string _configFilePath;

    private ConfigManager(string configFile)
    {
        _configFile = configFile;
        _configFilePath = Path.GetFullPath(_configFile);
        _config = new Dictionary<string, string>();
        LoadConfig();
    }

    public static ConfigManager Instance(string configFile = "config.txt")
    {
        if (_instance == null)
        {
            _instance = new ConfigManager(configFile);
        }
        return _instance;
    }

    private void LoadConfig()
    {
        if (File.Exists(_configFilePath))
        {
            foreach (string line in File.ReadAllLines(_configFilePath))
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    _config[parts[0].Trim()] = parts[1].Trim();
                }
            }
            Console.WriteLine($"Config file loaded successfully: {_configFilePath}");
        }
        else
        {
            Console.WriteLine($"Config file not found: {_configFilePath}");
        }
    }

    public string GetValue(string key, string defaultValue)
    {
        return _config.TryGetValue(key, out string value) ? value : defaultValue;
    }

    public int GetIntValue(string key, int defaultValue)
    {
        if (_config.TryGetValue(key, out string value) && int.TryParse(value, out int result))
        {
            return result;
        }
        return defaultValue;
    }
}