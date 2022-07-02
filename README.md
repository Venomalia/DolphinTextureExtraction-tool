# DolphinTextureExtraction-tool
Dumping of GC and Wii textures, dumps all textures at once and compatible with dolphins textures hash.

## How to use
INFO: currently no ROM images are supported, Please unpack them with dolphin into a folder.  
Right click on a game -> **Properties** -> **Filesystem** -> right click on "**Disc - [Game ID]**" -> **Extract Files**...

### Command-line UI
Launch `DolphinTextureExtraction tool.exe` and
Follow the instructions of the application

### Command-line
- **Syntax:** `EXTRACT "Input" "Output" -mip`
   > Extracts all textures and their mipmaps textures.

- **Syntax:** `HELP`
   > For a list with all commands.

## Supported formats
- RARC, U8, CPK Archives
- YAZ, YAY, CLZ, LZ11 Compressing
- BTI, TPL, TEX1 Textures
- BMD, BDL J3D Models

## Known results
### Fully supported games
- The Legend of Zelda Twilight
- The Legend of Zelda Four Swords
- THE LEGEND OF ZELDA The Wind Waker
- SUPER MARIO GALAXY 1+2
- SUPER PAPER MARIO
- Mario Kart Double Dash!
- Paper Mario: The Thousand-Year Door
- Super Mario Sunshine
- Harvest Moon A Wonderful Life
- Doshin the Giant

### Well supported games
- LUIGI'S MANSION
- Pikmin 2

### Not supported games
> When the unsupported archives are first unpacked with another tool, more or all textures can be dumped.
- Smash Bros. Brawl
- The Legend of Zelda Skyward Sword
- MarioGolf Toadstool Tour
- F-ZERO GX
- Pikmin 1
- Super Smash Bros Melee
- Pokemon XD & Colosseum
- Mario Party 4-9
- Metroid Prime 1-3 + Trilogy
- Pandora s Tower
- Punch Out

## Credits
 
- [Hack.io](https://github.com/SuperHackio/Hack.io)
    - to read RARC, U8 Archives
    - YAZ, YAY Compressing
    - BTI, TPL, TEX1 Textures
    - BMD, BDL J3D Models

- [HashDepot](https://github.com/ssg/HashDepot)
    - used for xxHash generation

- [cpk-tools](https://github.com/ConnorKrammer/cpk-tools)
    - to read CRIWARE's CPK archive format
	
- [Wexos's_Toolbox](https://wiki.tockdom.com/wiki/Wexos's_Toolbox)
    - to read LZ11 Compressing

- [CLZ-Compression](https://github.com/sukharah/CLZ-Compression)
    - to read CLZ Compressing