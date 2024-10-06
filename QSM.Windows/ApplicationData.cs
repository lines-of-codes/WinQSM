﻿using QSM.Core.ServerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AppData = Windows.Storage.ApplicationData;

namespace QSM.Windows;

internal class ApplicationData
{
    public static string ApplicationDataPath = Path.Combine(AppData.Current.LocalCacheFolder.Path, "Local", "QSM");
    public static string ServersFolderPath = Path.Combine(ApplicationDataPath, "Servers");
    public static string JavaInstallsPath = Path.Combine(ApplicationDataPath, "Java");
    public static string LogsFolderPath = Path.Combine(ApplicationDataPath, "Logs");
	public static string ConfigFile = Path.Combine(ApplicationDataPath, "config.json");
    public static ApplicationConfiguration Configuration { get; set; } = new();
    public static Dictionary<Guid, ServerSettings> ServerSettings { get; set; } = [];
    public static JsonSerializerOptions SerializerOptions = new()
	{
        IgnoreReadOnlyProperties = true
    };

	public static void EnsureDataFolderExists()
    {
        Directory.CreateDirectory(ApplicationDataPath);
        Directory.CreateDirectory(JavaInstallsPath);
        Directory.CreateDirectory(LogsFolderPath);
    }

    public static void LoadConfiguration()
    {
        if (!File.Exists(ConfigFile)) return;
        Configuration = JsonSerializer.Deserialize<ApplicationConfiguration>(File.ReadAllText(ConfigFile));
    }

    public static void SaveConfiguration()
    {
        string jsonStr = JsonSerializer.Serialize(Configuration, SerializerOptions);
        File.WriteAllText(ConfigFile, jsonStr);
    }
}
