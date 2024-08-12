# DragonEssentials
A "modloader" mod for [Reloaded-II](https://reloaded-project.github.io/Reloaded-II/) that makes it easy for other mods to replace or load new files files in Dragon Engine's Yakuza/Like a Dragon games.  

## WARNING  
- Currently this will successfully load and replace all the common file types inside of the standard PAR files (chara, db, etc) EXCEPT any .dds files, this means that currently loading new custom models is not possible (unless all of that model's textures already exist within the game's files).
- Currently this only really supports Like a Dragon: Infinite Wealth (as I want everything to be working first before adding other games support).

## Features
- Loading loose files from PAR
- Logging file access

## Planned Features
- Microsoft Store support (Gamepass)
- (Unknown how different GOG version is from Steam executable)
- TBD

## Usage
You'll need to create a Reloaded mod and set Dragon Esentials as a dependency of it. For more details on making a mod check out Reloaded's [documentation](https://reloaded-project.github.io/Reloaded-II/CreatingMods/).

Next, open your mod's folder and create an `DragonEssentials` folder inside of it, this is where you will put your edited files. 

### Loose Files
To include loose files put them in the `DragonEssentials` folder, replicating their folder structure from the original game (this structure is the same as if the file was inside the original `PAR` archive).
