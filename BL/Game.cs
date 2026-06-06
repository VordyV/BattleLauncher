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

    public event Func<GameStatus, Task> OnChangeStatus; 

    protected Launcher Launcher;

    public Game(Launcher launcher, string id, string name, string shortName, string[] determinants)
    {
        this.Launcher = launcher;
        this.Id = id;
        this.Name = name;
        this.ShortName = shortName;
        this.Determinants = determinants;
    }

    public async Task<bool> CheckInstall()
    {
        await this.Launcher.Registry.WaitReadiness();
        bool isInstall = this.Launcher.Registry.HasSection(this.Id);
        string? path = this.GetPath();
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

    public async Task GenerateDefaultClient()
    {
        await this.Launcher.GenerateDefaultClient(this.Id);
    }

    public string? GetPath()
    {
        return this.Launcher.GetPath(this.Id);
    }

    public void SetReferenceClient(string? clientId)
    {
        this.ReferenceClient = clientId;
        this.Launcher.SetReferenceClient(this.Id, clientId);
    }
    
    public string? GetReferenceClient()
    {
        string? clientId = this.Launcher.GetReferenceClient(this.Id);
        if (clientId == null) throw new Exception("Reference client has not been set");
        this.ReferenceClient = clientId;
        return clientId;
    }
    
    public void SetCurrentClient(string? clientId)
    {
        this.Client = clientId;
        this.Launcher.SetCurrentClient(this.Id, clientId);
    }
    
    public string? GetCurrentClient()
    {
        string? clientId = this.Launcher.GetCurrentClient(this.Id);
        if (clientId == null) throw new Exception("Current client has not been set");
        this.Client = clientId;
        return clientId;
    }

    public async Task<ClientData[]> GetClients()
    {
        return await this.Launcher.GetClients(this.Id);
    }
}