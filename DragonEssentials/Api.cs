using DragonEssentials.Interfaces;

namespace DragonEssentials;
public class Api : IDragonEssentials
{
    private Action<string> _addFolder;

    internal Api(Action<string> addFolder)
    {
        _addFolder = addFolder;
    }

    public void AddFromFolder(string path) => _addFolder(path);
}
