using System.IO;

namespace CLZ
{
    public static class CLZ
    {
        //Based on https://github.com/sukharah/CLZ-Compression/blob/master/source/CLZ.cpp
        internal static void Unpack(Stream infile, Stream outfile)
        {
            const int WINDOW_SIZE = 4096;

            byte[] window = new byte[WINDOW_SIZE];

            int window_ofs = 0;

            //CLZ files contain compressed data after a 16 byte header.
            infile.Seek(16,SeekOrigin.Begin);
            int bits = 0;
            int bit_count = 0;

            byte c = 0;

            const int BUFFER_SIZE = 4096;
            byte[] buffer_in = new byte[BUFFER_SIZE];
            int buffer_size = 0;
            int buffer_ofs = 0;

            bool exit = false;

            while (!exit)
            {
                if (buffer_ofs >= buffer_size)
                {
                    if (!(infile.Position == infile.Length - 1))
                    {
                        buffer_size = infile.Read(buffer_in,0, BUFFER_SIZE);
                        buffer_ofs = 0;
                        if (buffer_ofs < buffer_size)
                        {
                            c = buffer_in[buffer_ofs++];
                        }
                        else
                        {
                            exit = true;
                        }
                    }
                    else
                    {
                        exit = true;
                    }
                }
                else
                {
                    c = buffer_in[buffer_ofs++];
                }
                if (!exit)
                {

                    //The byte `00` is read into the bit stream, which identifies the next 8 bytes as raw data.
                    if (bit_count == 0)
                    {
                        bits = c;
                        bit_count = 8;
                    }
                    else
                    {
                        if ((bits & 1) != 0)
                        {
                            int window_delta = c & 0xff;

                            if (buffer_ofs >= buffer_size)
                            {
                                if (!(infile.Position == infile.Length - 1))
                                {
                                    buffer_size = infile.Read(buffer_in,0, BUFFER_SIZE);
                                    buffer_ofs = 0;
                                    if (buffer_ofs < buffer_size)
                                    {
                                        c = buffer_in[buffer_ofs++];
                                    }
                                    else
                                    {
                                        exit = true;
                                    }
                                }
                                else
                                {
                                    exit = true;
                                }
                            }
                            else
                            {
                                c = buffer_in[buffer_ofs++];
                            }

                            int length = (int)c & 0xff;
                            window_delta |= length << 4 & 0xf00;
                            length = (length & 0x0f) + 3;

                            if (window_ofs + length >= WINDOW_SIZE)
                            {
                                for (int i = window_ofs; i < WINDOW_SIZE; ++i)
                                {
                                    window[i] = window[(window_delta + i) % WINDOW_SIZE];
                                }
                                outfile.Write(window,0,WINDOW_SIZE);
                                window_ofs = window_ofs + length - WINDOW_SIZE;
                                for (int i = 0; i < window_ofs; ++i)
                                {
                                    window[i] = window[(i + window_delta) % WINDOW_SIZE];
                                }
                            }
                            else
                            {
                                for (int i = 0; i < length; ++i)
                                {
                                    window[window_ofs + i] = window[(window_ofs + i + window_delta) % WINDOW_SIZE];
                                }
                                window_ofs += length;
                            }
                        }
                        else
                        {
                            window[window_ofs++] = c;
                            if (window_ofs == WINDOW_SIZE)
                            {
                                outfile.Write(window,0, WINDOW_SIZE);
                                window_ofs = 0;
                            }
                        }
                        bits >>= 1;
                        bit_count--;
                    }
                }
            }
            if (window_ofs != 0)
            {
                outfile.Write(window,0,window_ofs);
            }
        }
	}
}
