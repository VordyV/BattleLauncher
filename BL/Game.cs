namespace BL;

public class Game
{
    public string Id { get; }
    public string Name { get;  }
    public string ShortName { get; }

    protected Launcher Launcher;

    public Game(Launcher launcher, string id, string name, string shortName)
    {
        this.Launcher = launcher;
        this.Id = id;
        this.Name = name;
        this.ShortName = shortName;
    }

    public async Task<bool> IsInstall()
    {
        
        //Console.WriteLine(await this.Launcher.Configurator.HasSection(this.Id));
        return false;
    }
}