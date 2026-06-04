namespace BL;

public class Launcher
{
    public Configurator Configurator;
    
    protected Dictionary<string, Game> Games;
    
    public Launcher(string pathConfig)
    {
        this.Games = new();
        this.Configurator = new Configurator(pathConfig);
    }

    public void RegisterGame(string gameId, string name, string shortName, string[] determinants)
    {
        if (this.Games.ContainsKey(gameId)) throw new Exception($"A game with this ID '{gameId}' is already registered");
        this.Games.Add(gameId, new Game(launcher: this, id: gameId, name: name, shortName: shortName, determinants: determinants));
    }

    public Game GetGame(string gameId)
    {
        if (!this.Games.ContainsKey(gameId)) throw new Exception($"A game with this ID '{gameId}' is not registered");
        return this.Games[gameId];
    }

    public List<string> GetGames() => this.Games.Keys.ToList();

    public void AddGameRegistry(string gameId, string path)
    {
        Game game = this.GetGame(gameId);
        if (game.IsInstall) throw new Exception($"Game '{gameId}' is already in the registry");
        this.Configurator.AddSection(gameId);
        this.Configurator.Set(gameId, "path", path);
    }
}