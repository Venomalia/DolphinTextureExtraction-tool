# Hack.io
**A Collection of File In/Out libraries for various formats**<br/>
Previously referred to as "Hackio.IO"

## What is it?
The Hack.io libraries are easy-to-use, modular C# libraries that can be used in multiple projects. I made these with the intent of just using them so I wouldn't have to constantly repeat my code when doing projects.
In the end, here they are, feel free to use them in your projects.

## Library Listing

- **Hack.io**<br/>
Hack.io is the base library used by all the other Hack.io libraries.

- **Hack.io.BCK**<br/>
Bone Animations for J3D Models (SMG/SMG2, SMS, Pikmin, etc.)

- **Hack.io.BMD**<br/>
Library for J3D Models (SMG/SMG2, SMS, Pikmin, etc.)<br/>
Heavily based on SuperBMD, but only does Read/Write to/from BMD/BDL. (no model importing and no model exporting)

- **Hack.io.BTK**<br/>
Texture Position Animations for J3D Models (SMG/SMG2, SMS, Pikmin, etc.)

- **Hack.io.BRK**<br/>
Colour Register Animations for J3D Models (SMG/SMG2, SMS, Pikmin, etc.)

- **Hack.io.BTI**<br/>
Library for the BTI image format. Supports all image formats and mipmaps

- **Hack.io.BTP**<br/>
Frame Animations for J3D Models (SMG/SMG2, SMS, etc.)<br/>
This would animate a material by swapping out texture indicies

- **Hack.io.BVA**<br/>
Mesh Visibility Animations for J3D Models (SMG/SMG2, etc.)

- **Hack.io.BPK**<br/>
Palette Animations for J3D Models (SMG/SMG2, etc.)

- **Hack.io.BCSV**<br/>
Comma Seperated Values (SMG/SMG2)

- **Hack.io.RARC**<br/>
Revolution (Wii) Archives. (SMG/SMG2, SMS)<br/>
**THESE ARE NOT U8 FORMATTED ARCHIVES!**

- **Hack.IO.YAZ0**<br/>
Library for Compressing/Decompressing data to/from YAZ0. Works with pretty much anything

- **Hack.IO.YAY0**<br/>
Library for Compressing/Decompressing data to/from YAY0. Works with pretty much anything

## How to use
Download the Libraries you want and then reference them in your program's Assembly References.<br/>
Using statements will also be needed. The statements are identical to the library name.<br/>
Example:<br>
```using Hack.io.RARC;```<br/>
For Library specific tutorials, please visit it's corresponding Wiki Page.


# Credits
- Super Hackio - Main Programmer/Maintainer
- RenolY2 aka Yoshi2 - Code reference for Hack.io.BTK & Hack.io.BRK
- NoClip.website - Code reference for Hack.io.BCK, Hack.io.BTP, Hack.io.BVA & Hack.io.BPK
- tarsa129 - Code reference for Hack.io.BCK because NoClip wasn't enough
- Old SMG Researchers - File formats for Hack.io.CANM, Hack.io.BCSV & Hack.io.RARC
- Gericom - Quick YAZ0 Compression
- Daniel-McCarthy - YAY0 Compression
