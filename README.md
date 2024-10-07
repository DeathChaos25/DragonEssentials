# DragonEssentials
A "modloader" mod for [Reloaded-II](https://reloaded-project.github.io/Reloaded-II/) that makes it easy for other mods to replace or load new files files in Dragon Engine's Yakuza/Like a Dragon games.  
Makes use of [CRI FileSystem V2 Hook](https://github.com/Sewer56/CriFs.V2.Hook.ReloadedII/releases) as a dependency for Criware support in games such as Judgment and Lost Judgment.

## Currently Supported Games
- Judgment
- Lost Judgment
- Yakuza Kiwami 2
- Yakuza 6: The Song of Life
- Yakuza: Like a Dragon
- Like a Dragon: Gaiden  
- Like a Dragon: Infinite Wealth

  (Note: Only LAD:IW and Gaiden have been thoroughly/extensively tested)

## Features
- Loading loose files from PAR
- Entity file replacement
- Logging file access
- Can use .all instead of language specific directory to replace db/ui files in all languages without copies
- (i.e. using .all will replace db.elvis.en as well as db.elvis.de, db.elvis.es, etc)
- Can replace USMs from within mod files
- Being a Reloaded-II mod means it can be used alongside other great Reloaded-II specific frameworks [such as Ryo](https://github.com/T-PoseRatkechi/Ryo).

## Planned Features
- Microsoft Store support (Gamepass)
- (Unknown how different GOG version is from Steam executable)
- TBD

## Current Limitations  
- Loading brand new non existing stage par files from mods is currently a bit wonky, you need both the actual proper complete par file in the mod, but also have all of its files loose as if doing a replacement.


## Usage
You'll need to create a Reloaded mod and set Dragon Esentials as a dependency of it. For more details on making a mod check out Reloaded's [documentation](https://reloaded-project.github.io/Reloaded-II/CreatingMods/).

Next, open your mod's folder and create an `DragonEssentials` folder inside of it, this is where you will put your edited files. 

### Loose Files
To include loose files put them in the `DragonEssentials` folder, replicating their folder structure from the original game (this structure is the same as if the file was inside the original `PAR` archive, while putting all files inside a root folder names after the PAR).

(Example of expected mod directory structure)  
![image](https://github.com/user-attachments/assets/234cc5e9-66e5-4a15-b641-205658a769c4)
![image](https://github.com/user-attachments/assets/0c64ce12-9ae1-485f-b541-7ac4b0018d8d)  
![image](https://github.com/user-attachments/assets/a99ea52c-75f0-4249-a892-10d693258597)


## Judgment and Lost Judgment  
Currently, it is also supported to replace the files found in Judgment's CPKs (such as AWB files found in Stream), however, these need to be put into their own separate structure, while also having some quirks  

- Create a "Criware" folder next to your "DragonEssentials" folder
- Create a "stream" or "stream_en" folder inside the Criware folder
- Place all your AWB files inside this folder

![image](https://github.com/user-attachments/assets/9947f94e-4b78-4077-a921-47860d217e7d)  
![image](https://github.com/user-attachments/assets/c7075de9-4c62-490b-ab0e-afdb1d3cda63)
![image](https://github.com/user-attachments/assets/664dedfd-9f35-428a-a393-96c23d3cc725)


Please note that currently, having both English and Japanese dub audio loaded via this method for Judgment and Lost Judgment is currently not supported. This is not to say either of them cannot be done, they are simply both individually supported, but not both within the same mod, this is due to the filepath inside the CPK being identical for both.
