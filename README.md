# DolphinTextureExtraction-tool
Is a Command line tool for extracting GC and Wii textures from disc and is compatible with Dolphin's texture hash.
It allows you to extract textures from a game without having to play through the game with dolphin first.
The tool can be used for [any game](https://github.com/Venomalia/DolphinTextureExtraction-tool/wiki/Known-results), but the extraction rate depends on what formats the game uses and if the tool supports them.
the tool can also unpack all supported formats.
The most common formats are already supported, new features and formats will be added over time.


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
- RARC, NARC, U8, CPK, bres, BIN_MP, AFS, RKV2, FBC, FBTI, NLCM, PCKG, RTDP, PAK_Retro, PAK_RetroWii, POD5 Archives
- YAZ, YAY, CLZ, GZIP, LZ11, LZ77, LZSS, ZLib Compressing
- ATB, BTI, TPL, NUTC, REFT, TEX_KS, TEX_RFS, TEX, TEX1, TEX0, TXE, XTR, PTLG, HXTB, WTMD Textures
- MOD, BMD3, BDL4 Models
## Credits
 
- [Hack.io](https://github.com/SuperHackio/Hack.io)
    - to read RARC, U8 Archives
    - YAZ, YAY Compressing
    - BTI, TPL, TEX1 Textures
    - BMD, BDL J3D Models

- [HashDepot](https://github.com/ssg/HashDepot)
    - used for xxHash generation

- [SharpZipLib](https://github.com/icsharpcode/SharpZipLib)
    - to read ZLib Compressing
	
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
	
- [MODConv](https://github.com/intns/MODConv)
    - reference for MOD Format.
	
- [mpbintools](https://github.com/gamemasterplc/mpbintools)
    - reference for BIN_MP Format.
	
- [mpatbtools](https://github.com/gamemasterplc/mpatbtools)
    - reference for ATB Format.