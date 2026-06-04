namespace BL;

public class Game
{
    public string Id { get; }
    public string Name { get; }
    public string ShortName { get; }
    public GameStatus Status { get; protected set; } = GameStatus.NotInstalled;
    public bool IsInstall { get; protected set; }
    public string[] Determinants { get; }

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
        await this.Launcher.Configurator.WaitReadiness();
        bool isInstall = this.Launcher.Configurator.HasSection(this.Id);
        this.IsInstall = isInstall;
        if (isInstall) await this.SetStatus(GameStatus.NotRunning);
        else await this.SetStatus(GameStatus.NotInstalled);
        return isInstall;
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
}