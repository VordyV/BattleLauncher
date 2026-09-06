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
    public static string StateSnapshotFilename { get; } = ".statesnapshot";
    
    public Configurator Registry { get; }
    
    protected Dictionary<string, Game> Games;
    
    public Launcher(string pathConfig)
    {
        this.Games = new();
        this.Registry = new Configurator(pathConfig);
    }

    public void RegisterGame(string gameId, string name, string shortName, string[] determinants, ILaunchParam[] launchParams)
    {
        if (!this.Games.TryAdd(gameId, new Game(launcher: this, registry: this.Registry, id: gameId, name: name, shortName: shortName, determinants: determinants, launchParams: launchParams))) 
            throw new GameAlreadyRegException($"A game with this ID '{gameId}' is already registered");
    }

    public Game GetGame(string gameId)
    {
        Game? game;
        if (!this.Games.TryGetValue(gameId, out game)) throw new GameNotFoundException($"A game with this ID '{gameId}' is not registered");
        return game;
    }

    public List<string> GetGames() => this.Games.Keys.ToList();

    public void AddGameRegistry(string gameId, string path)
    {
        Game game = this.GetGame(gameId);
        if (game.IsInstall) throw new GameInstallException($"Game '{gameId}' is already in the registry");
        this.Registry.AddSection(gameId);
        this.Registry.Set(gameId, "path", path);
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

    public async Task SetStateSnapshot(string gameId, StateSnapshot state)
    {
        Game game = this.GetGame(gameId);
        string path = game.GetPath();
        
        string data = JsonSerializer.Serialize(state);
        await File.WriteAllTextAsync(Path.Combine(path, Launcher.StateSnapshotFilename), data);
    }
    
    public async Task<StateSnapshot?> GetStateSnapshot(string gameId)
    {
        Game game = this.GetGame(gameId);
        string path = game.GetPath();

        string data = await File.ReadAllTextAsync(Path.Combine(path, Launcher.StateSnapshotFilename));
        return JsonSerializer.Deserialize<StateSnapshot>(data);
    }

    public async Task ChangeClientGame(string gameId, string clientId)
    {
        Game game = this.GetGame(gameId);
        string path = game.GetPath();

        StateSnapshot? currentState = await this.GetStateSnapshot(gameId);
        if (currentState == null) return;
        
        ClientData newClient = game.Clients[clientId];

        string refClientId = game.GetReferenceClient();
        
        ClientData refClient = game.Clients[refClientId];

        StateSnapshot newState = new StateSnapshot() {Files = currentState.Files};

        string clientsDir = Path.Combine(path, Launcher.ClientsDir);
        string newClientDir = Path.Combine(clientsDir, newClient.ID);
        string refClientDir = Path.Combine(clientsDir, refClientId);

        List<string> subjectReplace = new();
        List<string> subjectRemove = new();
        List<string> subjectRestore = new();
        List<string> subjectCreate = new();
        foreach (var file in currentState.Files)
        {
            if (newClient.Files.Contains(file.Key) && file.Value != newClient.ID)
            {
                subjectReplace.Add(file.Key);
                File.Copy(Path.Combine(newClientDir, file.Key), Path.Combine(path, file.Key), overwrite: true);
                newState.Files[file.Key] = newClient.ID;
            }

            if (!newClient.Files.Contains(file.Key) && refClient.Files.Contains(file.Key) && file.Value != refClientId)
            {
                subjectRestore.Add(file.Key);
                File.Copy(Path.Combine(refClientDir, file.Key), Path.Combine(path, file.Key), overwrite: true);
                newState.Files[file.Key] = refClientId;
            }

            if (!newClient.Files.Contains(file.Key) && !refClient.Files.Contains(file.Key))
            {
                subjectRemove.Add(file.Key);
                File.Delete(Path.Combine(path, file.Key));
                newState.Files.Remove(file.Key);
            }
        }

        foreach (var file in newClient.Files)
        {
            if (!currentState.Files.ContainsKey(file))
            {
                subjectCreate.Add(file);
                File.Copy(Path.Combine(newClientDir, file), Path.Combine(path, file), overwrite: true);
                newState.Files[file] = newClient.ID;
            }
        }

        await game.SetStateSnapshot(newState);
        
    }
}