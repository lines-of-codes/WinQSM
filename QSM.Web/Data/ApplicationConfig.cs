using System.Runtime.InteropServices;
using System.Text.Json;

namespace QSM.Web.Data;

public class ApplicationConfig
{
	public const string HttpDownloadClient = "HttpDownload";

	public static string AppFolder { get; private set; } = GetDefaultAppDataFolder();
	public static string DownloadsFolder => Path.Join(AppFolder, "downloads");
	
	public List<string> JavaInstalls { get; set; } = [];
	public string? DefaultJavaInstall { get; set; }
	public int ConcurrentDownloads { get; set; } = 5;
	
	private static string GetDefaultAppDataFolder()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return "/var/lib/qsm-web/";

		if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
			return "/usr/local/etc/qsm-web/";

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return @"C:\ProgramData\QSMWeb\";

		return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "./" : string.Empty;
	}
	
	public static string BackupSystemPath()
	{
		string installPath = Path.Join(GetDefaultAppDataFolder(), "qbs/qbsgo");

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			installPath += ".exe";
		}
		
		return installPath;
	}

	public static ApplicationConfig? LoadConfig()
	{
		var path = Path.Combine(GetDefaultAppDataFolder(), "config.json");

		if (!File.Exists(path)) return null;
		
		using var stream = File.OpenRead(path);
		return (ApplicationConfig?)JsonSerializer.Deserialize(stream, typeof(ApplicationConfig), ApplicationConfigContext.Default);
	}

	public void SaveConfig()
	{
		string path = Path.Combine(GetDefaultAppDataFolder(), "config.json");

		using var stream = File.OpenWrite(path);
		JsonSerializer.Serialize(stream, this, typeof(ApplicationConfig), ApplicationConfigContext.Default);
	}

	public static void EnsureFolderExists(string? appFolder)
	{
		if (!string.IsNullOrWhiteSpace(appFolder))
		{
			AppFolder = appFolder;
		}

		Directory.CreateDirectory(Path.Join(AppFolder, "downloads"));
	}
}