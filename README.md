# DolphinTextureExtraction-tool
[![Wiki](https://img.shields.io/badge/Wiki-grey)](https://github.com/Venomalia/DolphinTextureExtraction-tool/wiki)
[![Discord](https://img.shields.io/badge/Discord-blue?logo=Discord&logoColor=fff)](https://discord.gg/vtNRNxahTw)
[![Issues](https://img.shields.io/github/issues/Venomalia/DolphinTextureExtraction-tool?color=orange)](https://github.com/Venomalia/DolphinTextureExtraction-tool/issues)
[![Dolphin](https://img.shields.io/badge/Dolphin-Forum-88e)](https://forums.dolphin-emu.org/Thread-textureextraction-tool-v0-8-2-6)
[![Downloads](https://img.shields.io/github/downloads/Venomalia/DolphinTextureExtraction-tool/total?color=907&label=Downloads)](https://github.com/Venomalia/DolphinTextureExtraction-tool/releases)
[![Stars](https://img.shields.io/github/stars/Venomalia/DolphinTextureExtraction-tool?color=990&label=Stars)](https://github.com/Venomalia/DolphinTextureExtraction-tool/stargazers)

Is a Command line tool for extracting GC and Wii textures from disc and is compatible with Dolphin's texture hash.
It allows you to extract textures from a game without having to play through the game with dolphin first.
The tool can be used for [any game](https://github.com/Venomalia/DolphinTextureExtraction-tool/wiki/Known-results), but the extraction rate depends on what formats the game uses and if the tool supports them.
the tool can also unpack all supported formats.
The most common formats are already supported, new features and formats will be added over time.

## Download
This is a .NET 6.0 application and requires the .NET Runtime 6.0. If you don't have it installed yet, you can download it from [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

[<img src="https://img.shields.io/github/v/release/Venomalia/DolphinTextureExtraction-tool?style=for-the-badge" alt="Release Download" height="34"/>](https://github.com/Venomalia/DolphinTextureExtraction-tool/releases/latest)

[<img src="https://img.shields.io/github/v/release/Venomalia/DolphinTextureExtraction-tool?include_prereleases&sort=semver&label=prerelease&style=for-the-badge" alt="Pre releases Download" height="34"/>](https://github.com/Venomalia/DolphinTextureExtraction-tool/releases/)

## How to use
INFO: If you use RVZ images, unpack them into a folder using Dolphin.  
Right click on a game -> **Properties** -> **Filesystem** -> right click on "**Disc - [Game ID]**" -> **Extract Files**...

### Command-line UI
Launch `DolphinTextureExtraction.tool.exe` and
Follow the instructions of the application

### Drag and Drop
Drop a folder on the `DolphinTextureExtraction.tool.exe`,
output to the same place only with a "~" in front of the new folder.

### Command-line
List of all [commands](https://github.com/Venomalia/DolphinTextureExtraction-tool/wiki/Command-Line-Commands).

- **Syntax:** `EXTRACT "Input" "Output" -mip`
   > Extracts all textures and their mipmaps textures.

- **Syntax:** `HELP`
   > For a list with all commands.

## [Known results](https://github.com/Venomalia/DolphinTextureExtraction-tool/wiki/Known-results)

## Supported formats
### ROM images
- GCDisk (ISO), WiiDisk (ISO), WAD
### Archives
- Asura, AFS, ALAR, ALIG, ARC_Pit, ARC0, BIN_MP, BIG, CMN, CPK, FBC, Filelist, FBTI, FSYS, FPK, GVMH, GSW, GSScene, RSC, NARC, NLCM, ONE_SB, ONE_UN, PAK_FE, PAK_Retro, PAK_RetroWii, PAK_TM2, PCKG, POD5, POSD, RARC, RKV2, RMHG, RTDP, TXAG, TXE, U8, bres, pBin, ShrekDir, SevenZip(zip, 7z, tar, deb, dmg, rpm, xar, bz2, lzh, cab, vhd)
### Compressing
-  AKLZ, AsuraZlb, CNS, CNX, CLZ, COMP, CRILAYLA, CXLZ, FCMP, GCLZ, GZIP, LH, LZ00, LZ01, LZ10, LZ11, LZ77, LZS, LZSS, LZSS_Sega, PRS, PRS_BE, YAY0, YAZ0, YAZ1, ZLB, ZLib, LZ4, LZO, RefPack, Zstd
### Textures
-  ALTX, BTI, FIPAFTEX, FTEX, GCNT, GBIX, GCIX, GCT0, GTX, GVRT, HXTB, LFXT, NUTC, PTLG, REFT, TEX, TEX0, TEX1, TEX_KS, TEX_RFS, TPL, TPL_0, TXE, TXTR, WTMD, ATB
### Model archives
- MOD, BMD3, BDL4, MDL_LM, HSF, PKX, WZX, GSAGTX, GSFILE11

## Credits
 
- [ImageSharp](https://github.com/SixLabors/ImageSharp) used as graphics API.

- [Hack.io](https://github.com/SuperHackio/Hack.io) to read RARC, U8, YAZ, YAY, BTI, TPL, TEX1, BMD and BDL Format

- [Puyo Tools](https://github.com/nickworonekin/puyotools) Code reference for ONE GVMH, GBIX, GCIX, GVRT, Format and to read PRS, CNX, Lz00, lz01 Lz10, Lz11 Compressing.

- [HashDepot](https://github.com/ssg/HashDepot) used for xxHash generation

- [Ironcompress](https://github.com/aloneguid/ironcompress) used for LZO, LZ4 and Zstandard.

- [SevenZip](https://github.com/adoconnection/SevenZipExtractor) to read formats supported by 7Zip
	
- [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) to read ZLib Compressing
	
- [AFSLib](https://github.com/MaikelChan/AFSLib) to read AFS archive format

- [cpk-tools](https://github.com/ConnorKrammer/cpk-tools) to read CRIWARE's CPK archive format
	
- [CLZ-Compression](https://github.com/sukharah/CLZ-Compression) Code reference for CLZ Compressing

- [Switch-Toolbox](https://github.com/KillzXGaming/Switch-Toolbox/blob/12dfbaadafb1ebcd2e07d239361039a8d05df3f7/File_Format_Library/FileFormats/NLG/MarioStrikers/StrikersRLT.cs) Code reference for PTLG Format
	
- [Rune Factory Frontier Tools](https://github.com/master801/Rune-Factory-Frontier-Tools) Code reference for NLCM Archives
	
- [Custom Mario Kart Wiiki](https://wiki.tockdom.com/wiki/BRRES_(File_Format)) reference for bres, REFT, TEX0.
	
- [MODConv](https://github.com/intns/MODConv) reference for MOD Format.
	
- [mpbintools](https://github.com/gamemasterplc/mpbintools) reference for BIN_MP Format.
	
- [mpatbtools](https://github.com/gamemasterplc/mpatbtools) reference for ATB Format.

- [BrawlCrate](https://github.com/soopercool101/BrawlCrate) reference for ARC0 Format.

- [Helco](https://github.com/Helco/Pitfall) reference for LFXT Format.

