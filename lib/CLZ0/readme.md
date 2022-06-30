## CLZ Format
CLZ files contain compressed data after a 16 byte header.
Every file starts with the four character file signature "CLZ\0".
At offsets 4 and 12 in the header are 32-bit big-endian integers representing the size of the uncompressed data.
Bytes 8-11 of the header are 0.

An example header:
```
43 4c 5a 00 00 00 2a 79 00 00 00 00 00 00 2a 79
```
This file is 0x2a79 bytes (10873) when decompressed.

The compressed data that follows is an interleaved bitstream and byte stream.
The bit stream, read in LSB order, determines whether the next entry in the byte stream is a raw byte (0), or a distance/length code (1).

Distance/length codes select between 3-18 bytes from a 4096 byte sliding window to be copied to the output. The codes are encoded as 2 byte values.
The 12-bit value of the distance + 1024 is stored in the first byte (bits 0-7) and the high nibble of the second byte (bits 8-11).
The length is the lower nibble of the second byte + 3.
For example, a code of `d8 f4` represents a length of 7 (4 + 3) and a distance of -40 (0xfd8 - 1024) from the end of the window.

### Example

```
00 52 49 46 46 00 00 2a 79 00 53 43 52 20 43 4f
44 45 01 f4 f0 34 00 00 05 46 17 00
```
To decompress this sequence, the decompressor takes the following steps:

- The byte `00` is read into the bit stream, which identifies the next 8 bytes as raw data.
- The data `52 49 46 46 00 00 2a 79` is copied from the byte stream to the output and sliding window.
- Another `00` byte is read into the bit stream.
- The byte stream data `53 43 52 20 43 4f 44 45` is copied to the output.
- The byte `01` is read into the bit stream.
- The first bit is 1, so the next entry in the byte stream is the distance/length code `f4 f0`.
    - `ff4` is the two's complement of -12, so the data starts on the 12th byte from the end of the window and has length 0 + 3 = 3
    - The decompressor copies the sequence `00 00 2a` from the window to the output and window.
- The remaining 7 bits are all 0, so the byte stream data `34 00 00 05 46 17 00` is copied to the output.

This results in the decompressed sequence
```
52 49 46 46 00 00 2a 79 53 43 52 20 43 4f 44 45
00 00 2a 34 00 00 05 46 17 00
```

## About the program

The code provided in this repository is for a command line utility to provide compression/decompression functionality for CLZ archives.

To compile, the following command can be used
```
g++ -std=c++11 -iquote header "source/main.cpp" "source/CLZ.cpp" "source/CLZHashTable.cpp"
```
Additional parameters can be added to increase portability or improve performance of the code.
```
g++ -static-libgcc -static-libstdc++ -march=native -Ofast -W -Wall -Wextra -std=c++11 -iquote header "source/main.cpp" "source/CLZ.cpp" "source/CLZHashTable.cpp"
```

The program expects three command line arguments.
The first is a case-sensitive 'command' specifying what functionality the program should exhibit.
The second argument is a path to the file to be used as input; for unpack this is a compressed archive, for pack this is the file to be compressed.
The third argument is a path to the output file; an uncompressed file for unpack or a compressed archive for pack.

### Example command lines
To compress a file using standard stream compression:
```
program.exe pack "./uncompressed_input.txt" "./compressed_ouput.clz"
```
To decompress a file:
```
program.exe unpack "./compressed_input.clz" "./uncompressed_ouput.txt"
```
To compress a file using in-memory optimizing compression (saves a few bytes in compressed file):
```
program.exe pack2 "./uncompressed_input.txt" "./compressed_ouput.clz"
```