using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using QSM.Core.ServerSettings;
using QSM.Core.ServerSoftware;
using QSM.Web.Data;
using QSM.Web.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace QSM.Web.Components.Create.Pages;

[UsedImplicitly]
public partial class CreateServer : ComponentBase
{
	private Dictionary<ServerSoftwares, InfoFetcher> _infoFetchers = null!;
	
	[SupplyParameterFromForm] private NewServerModel? Model { get; set; }

	private const long _maxFileSize = 100 * 1024 * 1024; // 100 MiB
	private bool _isProcessing;
	private string _processingMessage = string.Empty;
	private string _targetFolderPreview = string.Empty;
	private string _errorMessage = string.Empty;
	private IBrowserFile? _selectedFile;
	private string[] _minecraftVersions = [];
	private string[] _availableBuilds = [];

	protected override void OnInitialized()
	{
		_infoFetchers = new Dictionary<ServerSoftwares, InfoFetcher>
		{
			{ ServerSoftwares.Paper, new PaperMCFetcher("paper", HttpClientFactory) },
			{ ServerSoftwares.Purpur, new PurpurFetcher(HttpClientFactory) },
			{ ServerSoftwares.Vanilla, new VanillaFetcher(HttpClientFactory) },
			{ ServerSoftwares.Fabric, new FabricFetcher(HttpClientFactory) },
			{ ServerSoftwares.NeoForge, new NeoForgeFetcher(HttpClientFactory) },
			{ ServerSoftwares.Velocity, new PaperMCFetcher("velocity", HttpClientFactory) },
			{ ServerSoftwares.Folia, new PaperMCFetcher("folia", HttpClientFactory) },
			{ ServerSoftwares.Forge, new ForgeFetcher(HttpClientFactory) },
			{ ServerSoftwares.Quilt, new QuiltFetcher(HttpClientFactory) }
		};
		Model ??= new NewServerModel();
		Model.Path = GetDefaultServerPath();
		_targetFolderPreview = Model.Path;
	}

	private static string GetDefaultServerPath()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			return "/var/lib/qsm-web/servers/";
		}

		if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
		{
			return "/usr/local/etc/qsm-web/servers/";
		}

		return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\ProgramData\QSMWeb\Servers\" : "./";
	}

	protected override async Task OnInitializedAsync()
	{
		try
		{
			await FetchMinecraftVersions();
			await FetchAvailableBuilds(_minecraftVersions[0]);
		}
		catch (Exception ex)
		{
			_errorMessage = ex.Message;
			Logger.LogError(ex, "An error occurred while fetching software information");
		}
	}

	private async Task OnValidSubmit()
	{
		_isProcessing = true;

		try
		{
			await Create();
		}
		catch (Exception ex)
		{
			_isProcessing = false;
			_errorMessage = ex.Message;
		}
	}

	private async Task Create()
	{
		_processingMessage = "Creating folder...";

		Directory.CreateDirectory(_targetFolderPreview);

		bool custom = Model!.Software == ServerSoftwares.Custom;
		if (custom)
		{
			await GetCustomServerFile();
		}
		else
		{
			await DownloadServerFile();
		}

		_processingMessage = "Registering server...";
		await using (ApplicationDbContext ctx = await DbFactory.CreateDbContextAsync())
		{
			ServerInstance newServer = new()
			{
				MinecraftVersion = custom ? null : Model.MinecraftVersion ?? _minecraftVersions[0],
				Software = Model.Software,
				Name = Model.Name,
				Running = false,
				ServerPath = _targetFolderPreview,
				ServerVersion = custom ? null : Model.ServerBuild ?? _availableBuilds[0]
			};
			ctx.Servers.Add(newServer);
			await ctx.SaveChangesAsync();
			
			_processingMessage = "Creating configuration file...";
			await new ServerSettings().SaveJsonAsync(newServer.ConfigPath);
		}

		_isProcessing = false;
		NavigationManager.NavigateTo("/");
	}

	private async Task GetCustomServerFile()
	{
		if (_selectedFile == null)
		{
			_errorMessage = "No file has been selected.";
			return;
		}

		_processingMessage = "Uploading server file...";
		await using FileStream fs = new(Path.Join(_targetFolderPreview, "server.jar"), FileMode.OpenOrCreate);
		await _selectedFile.OpenReadStream(_maxFileSize).CopyToAsync(fs);
	}

	private async Task DownloadServerFile()
	{
		_processingMessage = "Downloading server file...";
		string downloadUrl = await _infoFetchers[Model!.Software]
			.GetDownloadUrlAsync(Model.MinecraftVersion ?? _minecraftVersions[0],
				Model.ServerBuild ?? _availableBuilds[0]);

		using HttpClient client = HttpClientFactory.CreateClient();
		await using Stream stream = await client.GetStreamAsync(downloadUrl);
		await using FileStream fs = new(Path.Join(_targetFolderPreview, "server.jar"), FileMode.OpenOrCreate);

		await stream.CopyToAsync(fs);
	}

	private async Task FetchMinecraftVersions(ServerSoftwares software = ServerSoftwares.Paper)
	{
		_minecraftVersions = await _infoFetchers[software].FetchAvailableMinecraftVersionsAsync();
		_errorMessage = string.Empty;
	}

	private async Task FetchAvailableBuilds(string minecraftVersion)
	{
		ServerSoftwares software = Model!.Software;

		if (software == ServerSoftwares.Custom)
		{
			return;
		}

		_availableBuilds = await _infoFetchers[software].FetchAvailableBuildsAsync(minecraftVersion);
		_errorMessage = string.Empty;
	}

	private async Task SoftwareSelectionChanged(ChangeEventArgs args)
	{
		bool parseResult = Enum.TryParse(typeof(ServerSoftwares), (string)args.Value!, false, out object? software);

		if (!parseResult || (ServerSoftwares)software! == ServerSoftwares.Custom)
		{
			return;
		}

		try
		{
			await FetchMinecraftVersions((ServerSoftwares)software);
			await FetchAvailableBuilds(_minecraftVersions[0]);
		}
		catch (Exception ex)
		{
			_errorMessage = ex.Message;
			Logger.LogError(ex, "An error occurred while fetching Minecraft software version");
		}
	}

	private async Task MinecraftVersionChanged(ChangeEventArgs args)
	{
		string? version = (string?)args.Value;

		if (version == null)
		{
			return;
		}

		try
		{
			await FetchAvailableBuilds(version);
		}
		catch (Exception ex)
		{
			_errorMessage = ex.Message;
			Logger.LogError(ex, "An error occurred while fetching software build list");
		}
	}

	private void SetTargetFolderPreview(string path, string folderName)
	{
		_targetFolderPreview = Path.Join(path, StringUtility.SanitizeFileName(folderName));
	}

	private void NameInputChanged(ChangeEventArgs args)
	{
		string? newName = (string?)args.Value;

		if (newName == null)
		{
			return;
		}

		SetTargetFolderPreview(Model!.Path!, newName);
	}

	private void PathInputChanged(ChangeEventArgs args)
	{
		string? newPath = (string?)args.Value;

		if (newPath == null)
		{
			return;
		}

		SetTargetFolderPreview(newPath, Model!.Name!);
	}

	private async Task OnBeforeLeave(LocationChangingContext ctx)
	{
		if (!_isProcessing) return;
		
		bool confirm = await Js.InvokeAsync<bool>("window.confirm", "The installation isn't complete. Are you sure you want to leave this page?");

		if (!confirm) ctx.PreventNavigation();
	}

	private void HandleFileChange(InputFileChangeEventArgs e)
	{
		_selectedFile = e.File;
	}

	private sealed class NewServerModel
	{
		[Required] public string? Name { get; set; }

		[Required] public ServerSoftwares Software { get; set; } = ServerSoftwares.Paper;

		public string? MinecraftVersion { get; set; }

		public string? ServerBuild { get; set; }

		[Required] public string? Path { get; set; }
	}
}