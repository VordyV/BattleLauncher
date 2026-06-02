namespace BL;

public class Launcher
{
    protected Dictionary<string, Game> Games;
    
    public Launcher(string pathConfig)
    {
        
        this.Games = new();
    }

    public void RegisterGame(string gameId, string name, string shortName)
    {
        if (this.Games.ContainsKey(gameId)) throw new Exception($"A game with this ID '{gameId}' is already registered");
        this.Games.Add(gameId, new Game(launcher: this, id: gameId, name: name, shortName: shortName));
    }

    public Game GetGame(string gameId)
    {
        if (!this.Games.ContainsKey(gameId)) throw new Exception($"A game with this ID '{gameId}' is not registered");
        return this.Games[gameId];
    }

    public List<string> GetGames() => this.Games.Keys.ToList();
}