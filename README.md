# bereft-souls

[![Discord Label](https://img.shields.io/badge/Discord-Brome%20World-black.svg?labelColor=5865F2)](https://discord.com/invite/nYJfz3jgQy)

> Ultra-modded Terraria mod pack.

---

This is the repository housing the source code for the **Bereft Souls** compatibility mod and any immediately associated add-ons and libraries.
The majority of the project source lies within here, but some projects (such as the separate Mod Pack API) will be moved to a separate repository in the future.

## Building

Our compatibility aides directly reference various mods. For open-source and source-available mods, we include their source code as submodules in our project tree. For closed-source mods, we embed their assemblies. Our approach may change in the future, but this is currently satisfactory.


Since this servers as a monorepository with multiple embedded projects, we have to take an unorthodox approach to making tModLoader understand our project structure:

```bash
# 1. Clone the repository into a directory within `ModSources`
cd path/to/tModLoader/ModSources/
git clone https://github.com/bereft-souls/bereft-souls # You can keep the name as `bereft-souls`

# 2. Update any submodules.
git submodule update --init --recursive

# 3. The project should now be buildable.
dotnet build src/BereftSouls/BereftSouls.csproj -c Release
```
