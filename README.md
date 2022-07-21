# DolphinTextureExtraction-tool

Is a GC and Wii texture dump tool, it dumps all textures at once and compatible with dolphins textures hash.
it is still in an early stage, Currently, mainly typical gamecube formats are supported.

## How to use
INFO: currently no ROM images are supported, Please unpack them with dolphin into a folder.  
Right click on a game -> **Properties** -> **Filesystem** -> right click on "**Disc - [Game ID]**" -> **Extract Files**...

### Command-line UI
Launch `DolphinTextureExtraction tool.exe` and
Follow the instructions of the application

### Drag and Drop
Drop a folder on the `DolphinTextureExtraction tool.exe`,
output to the same place only with a "~" in front of the folder.

### Command-line
- **Syntax:** `EXTRACT "Input" "Output" -mip`
   > Extracts all textures and their mipmaps textures.

- **Syntax:** `HELP`
   > For a list with all commands.

## Known results 
### Games ( 80-100% )
- Animal Crossing Wii
- Boom Street
- DONKEY KONG JUNGLE BEAT
- Donkey Konga 2
- Doshin the Giant
- Harvest Moon A Wonderful Life
- LUIGI'S MANSION
- Mario & Sonic at the London 2012 Olympic Games
- Mario Kart Double Dash!
- Mario Kart Wii
- Mario Party 9
- Metroid: Other M
- Paper Mario: The Thousand-Year Door
- Pikmin 2
- SONIC COLOURS
- SUPER MARIO GALAXY 1
- SUPER MARIO GALAXY 2
- SUPER PAPER MARIO
- Sin & Punishment: Star Successor
- Spectrobes: Origins
- Star Fox Assault
- Super Mario Sunshine
- THE LEGEND OF ZELDA The Wind Waker[¹](#notes)
- The Legend of Zelda Four Swords
- The Legend of Zelda Skyward Sword
- The Legend of Zelda Twilight
- WarioWare Smooth Moves
- Wii Party
- Wii Sports
- Wii Sports Resort
- Zack & Wiki: Quest for Barbaros' Treasure 

### Games( 50-80% )
- Castlevania Judgment
- Mario Soccer
- Odama
- Super Smash Bros. Brawl
- Takt of Magic[¹](#notes) 

### Games( 20-50% )
- Go Vacation
- Mario Strikers Charged Football
- Night's Journey of Dreams
- Pikmin 1 

### Not supported games ( 0-20% )
- Kirby Air Ride
- MarioGolf Toadstool Tour
- F-ZERO GX
- SOULCALIBUR2
- FFCC Crystal Bearers
- Super Smash Bros Melee
- Pokemon XD & Colosseum
- Mario Party 4-8
- Metroid Prime 1-3 + Trilogy
- Pandora s Tower
- Punch Out
- Xenoblade Chronicles
- Donkey Konga
- Sonic Adventuere
- Tales of Symphonia
- Rune Factory Frontier
- Rune Factory Tides of Destiny
- Beyond Good and Evil
- Prince of Persia: Warrior Within
- One Piece: Grand Adventure
- Tony Hawk's Underground
- Tony Hawk's Proving Ground
- Tony Hawk's Pro Skater 4
- Tom Clancy's Splinter Cell
- Need for Speed - Undercover
- Need for Speed - Carbon
- Need for Speed - Underground
- Tomb Raider - Legend
- FIFA 15 - Legacy Edition
- Driver - San Francisco
- Dragon Ball - Revenge of King Piccolo
- Dragon Ball Z - Sagas
- Dragon Ball Z - Budokai Tenkaichi 3


#### Notes
1. set Extract textures from unknown files to true

## Supported formats
- RARC, NARC, U8, CPK, bres, AFS Archives
- YAZ, YAY, CLZ, LZ11, LZSS Compressing
- BTI, TPL, NUTC, REFT, TXE, TEX1, TEX0, TXTR, PTLG Textures
- BMD3, BDL4 J3D Models

## Credits
 
- [Hack.io](https://github.com/SuperHackio/Hack.io)
    - to read RARC, U8 Archives
    - YAZ, YAY Compressing
    - BTI, TPL, TEX1 Textures
    - BMD, BDL J3D Models

- [HashDepot](https://github.com/ssg/HashDepot)
    - used for xxHash generation

- [AFSLib](https://github.com/MaikelChan/AFSLib)
    - to read AFS archive format

- [cpk-tools](https://github.com/ConnorKrammer/cpk-tools)
    - to read CRIWARE's CPK archive format
	
- [Wexos's_Toolbox](https://wiki.tockdom.com/wiki/Wexos's_Toolbox)
    - to read LZ11 Compressing

- [CLZ-Compression](https://github.com/sukharah/CLZ-Compression)
    - to read CLZ Compressing

- [Switch-Toolbox](https://github.com/KillzXGaming/Switch-Toolbox/blob/12dfbaadafb1ebcd2e07d239361039a8d05df3f7/File_Format_Library/FileFormats/NLG/MarioStrikers/StrikersRLT.cs)
    - PTLG format base on