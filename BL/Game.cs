using System.Text.Json;

namespace BL;

public class Game
{
    public string Id { get; }
    public string Name { get; }
    public string ShortName { get; }
    public GameStatus Status { get; protected set; } = GameStatus.NotInstalled;
    public bool IsInstall { get; protected set; }
    public string[] Determinants { get; }
    public string ReferenceClient { get; protected set; }
    public string Client { get; protected set; }
    public Dictionary<string, ClientData> Clients { get; protected set; } = new();

    public event Func<GameStatus, Task> OnChangeStatus; 

    protected Launcher Launcher;
    protected Configurator Registry;

    public Game(Launcher launcher, Configurator registry, string id, string name, string shortName, string[] determinants)
    {
        this.Launcher = launcher;
        this.Registry = registry;
        this.Id = id;
        this.Name = name;
        this.ShortName = shortName;
        this.Determinants = determinants;
    }

    public async Task<bool> CheckInstall()
    {
        await this.Launcher.Registry.WaitReadiness();
        bool isInstall = this.Launcher.Registry.HasSection(this.Id);
        string? path;
        this.TryGetPath(out path);
        this.IsInstall = isInstall && path != null;
        if (this.IsInstall) await this.SetStatus(GameStatus.NotRunning);
        else await this.SetStatus(GameStatus.NotInstalled);
        return isInstall;
    }

    public bool CheckPathInstall()
    {
        string? path = this.GetPath();
        return path != null && Directory.Exists(path);
    }

    public void AddGameRegistry(string path)
    {
        this.Launcher.AddGameRegistry(this.Id, path);
    }

    protected async Task SetStatus(GameStatus status)
    {
        this.Status = status;
        await this.OnChangeStatus(status);
    }

    public bool IdentifyGame(string path)
    {
        if (!Directory.Exists(path)) return false;

        string pf = "";
        foreach (string filename in this.Determinants)
        {
            pf = Path.Combine(path, filename);
            if (!File.Exists(pf)) return false;
        }

        return true;
    }

    public async Task<ClientData> GenerateDefaultClient()
    {
        string pathGame = this.GetPath();

        string pathDirClients = Path.Combine(pathGame, Launcher.ClientsDir);
        if (!Directory.Exists(pathDirClients)) Directory.CreateDirectory(pathDirClients);

        string pathNewClient = Path.Combine(pathDirClients, Launcher.UnknownClientId);
        if (Directory.Exists(pathDirClients)) Directory.Delete(pathNewClient, true);
        Directory.CreateDirectory(pathNewClient);

        List<string> files = new();
        string fn;
        string fe;
        foreach (var file in Directory.GetFiles(pathGame))
        {
            fe = Path.GetExtension(file);
            fn = Path.GetFileName(file);
            if (Launcher.InvalidClientFileExtensions.Contains(fe)) continue;
            files.Add(fn);
            File.Copy(file, Path.Combine(pathNewClient, fn));
        }
        
        ClientData client = new ClientData() {ID = Launcher.UnknownClientId, Name = Launcher.UnknownClientName, Files = files.ToArray()};
        string txtMF = JsonSerializer.Serialize(client, new JsonSerializerOptions { WriteIndented = true});

        await File.WriteAllTextAsync(Path.Combine(pathNewClient, Launcher.ClientManifestFilename), txtMF);

        return client;
    }

    protected bool HasGameInRegistry(string? option = null)
    {
        if (option == null && !this.Registry.HasSection(this.Id) || (option != null && !this.Registry.HasSection(this.Id) && !this.Registry.HasOption(this.Id, option)) || (option != null && this.Registry.HasSection(this.Id) && !this.Registry.HasOption(this.Id, option)))
            return false;
        return true;
    }

    protected void CheckGameInRegistry()
    {
        if (!this.HasGameInRegistry()) throw new GameNotSetRegistryException($"Game '{this.Id}' has not been added to the launcher registry");
    }

    public string GetPath()
    {
        if (!this.HasGameInRegistry("path")) throw new GamePathNotSetException($"Game '{this.Id}' path is not set");
        string path = this.Registry.Get<string>(this.Id, "path");
        if (!Directory.Exists(path)) throw new GamePathNotFoundException($"Path '{path}' of game '{this.Id}' not found");
        return path;
    }

    public bool TryGetPath(out string? data)
    {
        data = null;
        if (!this.HasGameInRegistry("path")) return false;
        string path = this.Registry.Get<string>(this.Id, "path");
        if (!Directory.Exists(path)) return false;
        data = path;
        return true;
    }

    public void SetReferenceClient(string clientId)
    {
        this.CheckGameInRegistry();
        this.Registry.Set(this.Id, "referenceClient", clientId);
        this.ReferenceClient = clientId;
    }
    
    public string GetReferenceClient()
    {
        if (!this.HasGameInRegistry(option: "referenceClient")) throw new GameNotSetRegistryException($"Game '{this.Id}' referenceClient is not set");
        string clientId = this.Registry.Get<string>(this.Id, "referenceClient");
        this.ReferenceClient = clientId;
        return clientId;
    }
    
    public void SetCurrentClient(string clientId)
    {
        this.CheckGameInRegistry();
        this.Registry.Set(this.Id, "client", clientId);
        this.Client = clientId;
    }
    
    public string GetCurrentClient()
    {
        if (!this.HasGameInRegistry(option: "client")) throw new GameNotSetRegistryException($"Game '{this.Id}' client is not set");
        string clientId = this.Registry.Get<string>(this.Id, "client");
        this.Client = clientId;
        return clientId;
    }

    public async Task<ClientData[]> GetClients()
    {
        ClientData[] clients = await this.Launcher.GetClients(this.Id);
        foreach (var client in clients)
        {
            this.Clients.Add(client.ID, client);
        }
        return clients;
    }

    public async Task ChangeClientGame(string clientId)
    {
        await this.Launcher.ChangeClientGame(this.Id, clientId);
    }

    public async Task SetStateSnapshot(StateSnapshot state)
    {
        await this.Launcher.SetStateSnapshot(this.Id, state);
    }
    
    public async Task<StateSnapshot?> GetStateSnapshot()
    {
        return await this.Launcher.GetStateSnapshot(this.Id);
    }
}