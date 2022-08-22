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

## [Known results](https://github.com/Venomalia/DolphinTextureExtraction-tool/wiki/Known-results)

## Supported formats
- RARC, NARC, U8, CPK, bres, AFS, RKV2, FBC, FBTI, NLCM, PCKG,RTDP Archives
- YAZ, YAY, CLZ, GZIP, LZ11, LZ77, LZSS Compressing
- BTI, TPL, NUTC, REFT, TXE, TEX, TEX1, TEX0, TXTR, PTLG, HXTB, WTMD Textures
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
	
- [CLZ-Compression](https://github.com/sukharah/CLZ-Compression)
    - Code reference for CLZ Compressing

- [Wexos's_Toolbox](https://wiki.tockdom.com/wiki/Wexos's_Toolbox)
    - Code reference for LZ11, LZ77 Compressing
	
- [Switch-Toolbox](https://github.com/KillzXGaming/Switch-Toolbox/blob/12dfbaadafb1ebcd2e07d239361039a8d05df3f7/File_Format_Library/FileFormats/NLG/MarioStrikers/StrikersRLT.cs)
    - Code reference for PTLG Format
	
- [Rune Factory Frontier Tools](https://github.com/master801/Rune-Factory-Frontier-Tools)
    - Code reference for NLCM Archives
	
- [Custom Mario Kart Wiiki](https://wiki.tockdom.com/wiki/BRRES_(File_Format))
    - reference for bres, REFT, TEX0.