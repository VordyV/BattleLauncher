using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BL;

public class Launcher
{
    public static string[] InvalidClientFileExtensions { get; } = new[] {".lnk"};
    public static string UnknownClientId { get; } = "unknown";
    public static string UnknownClientName { get; } = "Unknown";
    public static string ClientsDir { get; } = ".clients";
    public static string ClientManifestFilename { get; } = ".manifest";
    
    public Configurator Registry;
    
    protected Dictionary<string, Game> Games;
    
    public Launcher(string pathConfig)
    {
        this.Games = new();
        this.Registry = new Configurator(pathConfig);
    }

    public void RegisterGame(string gameId, string name, string shortName, string[] determinants)
    {
        if (this.Games.ContainsKey(gameId)) throw new Exception($"A game with this ID '{gameId}' is already registered");
        this.Games.Add(gameId, new Game(launcher: this, id: gameId, name: name, shortName: shortName, determinants: determinants));
    }

    public void CheckGame(string gameId)
    {
        if (!this.Games.ContainsKey(gameId)) throw new Exception($"A game with this ID '{gameId}' is not registered");
    }

    public Game GetGame(string gameId)
    {
        this.CheckGame(gameId);
        return this.Games[gameId];
    }

    public List<string> GetGames() => this.Games.Keys.ToList();

    public void AddGameRegistry(string gameId, string path)
    {
        Game game = this.GetGame(gameId);
        if (game.IsInstall) throw new Exception($"Game '{gameId}' is already in the registry");
        this.Registry.AddSection(gameId);
        this.Registry.Set(gameId, "path", path);
    }
    
    public string? GetPath(string gameId)
    {
        this.CheckGame(gameId);
        if (!this.Registry.HasSection(gameId)) return null;
        return this.Registry.Get<string?>(gameId, "path", null);
    }

    public void SetReferenceClient(string gameId, string cliendId)
    {
        this.CheckGame(gameId);
        this.Registry.Set(gameId, "referenceClient", cliendId);
    }

    public string? GetReferenceClient(string gameId)
    {
        this.CheckGame(gameId);
        if (!this.Registry.HasSection(gameId)) return null;
        return this.Registry.Get<string?>(gameId, "referenceClient", null);
    }
    
    public void SetCurrentClient(string gameId, string cliendId)
    {
        this.CheckGame(gameId);
        this.Registry.Set(gameId, "client", cliendId);
    }

    public string? GetCurrentClient(string gameId)
    {
        this.CheckGame(gameId);
        if (!this.Registry.HasSection(gameId)) return null;
        return this.Registry.Get<string?>(gameId, "client", null);
    }

    public async Task GenerateDefaultClient(string gameId)
    {
        this.CheckGame(gameId);
        string? pathGame = this.GetPath(gameId);
        if (pathGame == null) return;

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
        
        string txtMF = JsonSerializer.Serialize(new ClientData() {ID = Launcher.UnknownClientId, Name = Launcher.UnknownClientName, Files = files.ToArray()}, new JsonSerializerOptions { WriteIndented = true});

        await File.WriteAllTextAsync(Path.Combine(pathNewClient, Launcher.ClientManifestFilename), txtMF);
    }

    public async Task<ClientData[]> GetClients(string gameId)
    {
        Game game = this.GetGame(gameId);
        string? path = game.GetPath();
        if (path == null || !Directory.Exists(Path.Combine(path, Launcher.ClientsDir))) return Array.Empty<ClientData>();

        List<ClientData> clients = new();
        
        string data;
        ClientData? client;
        foreach (var dir in Directory.GetDirectories(Path.Combine(path, Launcher.ClientsDir)))
        {
            if (!File.Exists(Path.Combine(dir, Launcher.ClientManifestFilename))) continue;

            data = await File.ReadAllTextAsync(Path.Combine(dir, Launcher.ClientManifestFilename));

            try
            {
                client = JsonSerializer.Deserialize<ClientData>(data);
                if (client == null) throw new Exception();
                clients.Add(client);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to read client manifest '{Path.GetDirectoryName(dir)}'. The file is corrupted.");
            }
        }

        return clients.ToArray();
    }
}