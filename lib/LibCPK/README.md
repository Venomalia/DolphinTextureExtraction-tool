CriPakTools-mod
===========
forked from uyjulian/CriPakTools

This tool is based off of code by Falo , Nanashi3 ,esperknight and uyjulian

I forked and added batch reimport and compress code .

Thanks for KenTse 's CRILAYLA compression method


* Add Batch Mode
* Add compression option
* Fix GTOC & ETOC
* Fix CPK header

* Still need to do:
* Add GUI



===========

Tool to extract/update contents of CRIWARE's CPK archive format. (aka CRI FileMajik)  
This is based off of code uploaded by Falo's code released on the Xentax forums (http://forum.xentax.com/viewtopic.php?f=10&t=10646) which was futher modified by Nanashi3 (http://forums.fuwanovel.org/index.php?/topic/1785-request-for-psp-hackers/page-4), which is then further modified by esperknight (https://github.com/esperknight/CriPakTools).  
I cleaned up the command line flags and enable to extract 0 byte CRILAYLA compressed files.  
If something breaks, open an issue.  
To print out options see CriPackTools -h  

Compiling
=========
Change directory to where the `CriPakTools.sln` file is located, then run `xbuild` if you have Mono. Output file should be in `CriPakTools/bin/CriPackTools.exe`. Otherwise, just open the `CriPakTools.sln` file in Visual Studio 2013 and build.

TODO:
* Add more error checking
* Clean up code
* Add option to create an archive
