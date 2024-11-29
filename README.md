# DolphinTextureExtraction-tool
[![Wiki](https://img.shields.io/badge/Wiki-grey)](https://github.com/Venomalia/DolphinTextureExtraction-tool/wiki)
[![Discord](https://img.shields.io/badge/Discord-blue?logo=Discord&logoColor=fff)](https://discord.gg/vtNRNxahTw)
[![Issues](https://img.shields.io/github/issues/Venomalia/DolphinTextureExtraction-tool?color=orange)](https://github.com/Venomalia/DolphinTextureExtraction-tool/issues)
[![Dolphin](https://img.shields.io/badge/Dolphin-Forum-88e)](https://forums.dolphin-emu.org/Thread-textureextraction-tool-v0-8-2-6)
[![Downloads](https://img.shields.io/github/downloads/Venomalia/DolphinTextureExtraction-tool/total?color=907&label=Downloads)](https://github.com/Venomalia/DolphinTextureExtraction-tool/releases)
[![Stars](https://img.shields.io/github/stars/Venomalia/DolphinTextureExtraction-tool?color=990&label=Stars)](https://github.com/Venomalia/DolphinTextureExtraction-tool/stargazers)

This command-line tool is designed for extracting textures from GameCube and Wii discs, offering compatibility with Dolphin's texture hash.
With allows you to quickly get textures to create a texture pack  without the need to play through them on Dolphin first!
While its extraction rate depends on the game's formats and the tool's support for them, it can handle a wide [range of games](https://github.com/Venomalia/DolphinTextureExtraction-tool/wiki/Known-results).
Additionally, the tool supports unpacking all compatible formats and some other functions. 

Although the most common formats are already supported, the tool is still under development and new functions and other formats will be added over time.

## Download
This is a .NET 8.0 application and requires the .NET Runtime 8.0. If you don't have it installed yet, you can download it from [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

[<img src="https://img.shields.io/github/v/release/Venomalia/DolphinTextureExtraction-tool?style=for-the-badge" alt="Release Download" height="34"/>](https://github.com/Venomalia/DolphinTextureExtraction-tool/releases/latest)

[<img src="https://img.shields.io/github/v/release/Venomalia/DolphinTextureExtraction-tool?include_prereleases&sort=semver&label=prerelease&style=for-the-badge" alt="Pre releases Download" height="34"/>](https://github.com/Venomalia/DolphinTextureExtraction-tool/releases/)

## How to use

### Command-line UI
Launch `DolphinTextureExtraction.tool.exe` and
Follow the instructions of the application

### Drag and Drop
Drop a folder on the `DolphinTextureExtraction.tool.exe`,
output to the same place only with a "~" in front of the new folder.

### Command-line
List of all [commands](https://github.com/Venomalia/DolphinTextureExtraction-tool/wiki/Command-Line-Commands).

- **Syntax:** `EXTRACT "Input" "Output" -dmd -amd`
   > Extracts all textures and arbitrary mipmap.

- **Syntax:** `HELP`
   > For a list with all commands.

## [Known results](https://github.com/Venomalia/DolphinTextureExtraction-tool/wiki/Known-results)

## [Supported formats](https://github.com/Venomalia/DolphinTextureExtraction-tool/wiki/Supported-formats)

## Credits
 
- [AuroraLib.Compression](https://github.com/Venomalia/AuroraLib.Compression) used as Compression library.

- [ImageSharp](https://github.com/SixLabors/ImageSharp) used as graphics API.

- [Hack.io](https://github.com/SuperHackio/Hack.io) Code reference for RARC, U8, BTI, TPL, TEX1, BMD and BDL Format.

- [Puyo Tools](https://github.com/nickworonekin/puyotools) Code reference for ONE GVMH, GBIX, GCIX, GVRT, Format.

- [HashDepot](https://github.com/ssg/HashDepot) used for xxHash generation

- [SevenZip](https://github.com/adoconnection/SevenZipExtractor) to read formats supported by 7Zip
	
- [AFSLib](https://github.com/MaikelChan/AFSLib) to read AFS archive format

- [cpk-tools](https://github.com/ConnorKrammer/cpk-tools) to read CRIWARE's CPK archive format
	
- [Switch-Toolbox](https://github.com/KillzXGaming/Switch-Toolbox/blob/12dfbaadafb1ebcd2e07d239361039a8d05df3f7/File_Format_Library/FileFormats/NLG/MarioStrikers/StrikersRLT.cs) Code reference for PTLG Format
	
- [Rune Factory Frontier Tools](https://github.com/master801/Rune-Factory-Frontier-Tools) Code reference for NLCM Archives
	
- [Custom Mario Kart Wiiki](https://wiki.tockdom.com/wiki/BRRES_(File_Format)) reference for bres, REFT, TEX0.
	
- [MODConv](https://github.com/intns/MODConv) reference for MOD Format.
	
- [mpbintools](https://github.com/gamemasterplc/mpbintools) reference for BIN_MP Format.
	
- [mpatbtools](https://github.com/gamemasterplc/mpatbtools) reference for ATB Format.

- [BrawlCrate](https://github.com/soopercool101/BrawlCrate) reference for ARC0 Format.

- [Helco](https://github.com/Helco/Pitfall) reference for LFXT Format.

- [Endless-Ocean-Files-Converter](https://github.com/NiV-L-A/Endless-Ocean-Files-Converter) reference for TDL0 Format.

- [Dolphin](https://github.com/dolphin-emu/dolphin/blob/master/docs/WiaAndRvz.md) reference for RVZ Format.
