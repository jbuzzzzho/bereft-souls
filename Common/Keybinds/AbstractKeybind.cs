using Microsoft.Xna.Framework.Input;

using Terraria.ModLoader;

namespace BereftSouls.Common.Keybinds;

public abstract class AbstractKeybind : ILoadable
{
    public ModKeybind? Keybind { get; private set; }

    protected abstract string Name { get; }

    protected abstract Keys DefaultBinding { get; }

    void ILoadable.Load(Mod mod)
    {
        Keybind = KeybindLoader.RegisterKeybind(mod, Name, DefaultBinding);
    }

    void ILoadable.Unload() { }
}