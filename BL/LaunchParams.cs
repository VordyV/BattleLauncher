namespace BL;

public abstract class ILaunchParam
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public bool FullFormat { get; set; } = false;
};

public abstract class LaunchParam <T> : ILaunchParam
{
    public abstract IEnumerable<string> BuildArgs(T param);
}

public class LaunchParamStr : LaunchParam<string>
{
    public override IEnumerable<string> BuildArgs(string param)
    {
        yield return $"+{this.Id} {param}";
    }
}

public class LaunchParamBool : LaunchParam<bool>
{
    public override IEnumerable<string> BuildArgs(bool param)
    {
        string value = param ? "1" : "0";
        yield return $"+{this.Id} {value}";
    }
}

public class LaunchParamDict : LaunchParam<string>
{
    public Dictionary<string, string> Dictionary { get; set; } = new();
    
    public override IEnumerable<string> BuildArgs(string param)
    {
        if (this.Dictionary.TryGetValue(param, out var args))
            yield return args;
    }
}