// 	CRIcompress method by KenTse
#include <windows.h>
#include "stdafx.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "LibCRIComp.h"


namespace LibCRIComp
{
	int CriCompression::CRIcompress(unsigned char* dest, int* destLen, unsigned char* src, int srcLen)
	{
		int n = srcLen - 1, m = *destLen - 0x1, T = 0, d = 0, p, q, i, j, k;
		unsigned char* odest = dest;
		for (; n >= 0x100;)
		{
			j = n + 3 + 0x2000;
			if (j > srcLen) j = srcLen;
			for (i = n + 3, p = 0; i < j; i++)
			{
				for (k = 0; k <= n - 0x100; k++)
				{
					if (*(src + n - k) != *(src + i - k)) break;
				}
				if (k > p)
				{
					q = i - n - 3; p = k;
				}
			}
			if (p < 3)
			{
				d = (d << 9) | (*(src + n--)); T += 9;
			}
			else
			{
				d = (((d << 1) | 1) << 13) | q; T += 14; n -= p;
				if (p < 6)
				{
					d = (d << 2) | (p - 3); T += 2;
				}
				else if (p < 13)
				{
					d = (((d << 2) | 3) << 3) | (p - 6); T += 5;
				}
				else if (p < 44)
				{
					d = (((d << 5) | 0x1f) << 5) | (p - 13); T += 10;
				}
				else
				{
					d = ((d << 10) | 0x3ff); T += 10; p -= 44;
					for (;;)
					{
						for (; T >= 8;)
						{
							*(dest + m--) = (d >> (T - 8)) & 0xff; T -= 8; d = d & ((1 << T) - 1);
						}
						if (p < 255) break;
						d = (d << 8) | 0xff; T += 8; p = p - 0xff;
					}
					d = (d << 8) | p; T += 8;
				}
			}
			for (; T >= 8;)
			{
				*(dest + m--) = (d >> (T - 8)) & 0xff; T -= 8; d = d & ((1 << T) - 1);
			}
		}
		if (T != 0)
		{
			*(dest + m--) = d << (8 - T);
		}
		*(dest + m--) = 0; *(dest + m) = 0;
		for (;;)
		{
			if (((*destLen - m) & 3) == 0) break;
			*(dest + m--) = 0;
		}
		*destLen = *destLen - m; dest += m;
		int l[] = { 0x4c495243,0x414c5941,srcLen - 0x100,*destLen };
		for (j = 0; j < 4; j++)
		{
			for (i = 0; i < 4; i++)
			{
				*(odest + i + j * 4) = l[j] & 0xff; l[j] >>= 8;
			}
		}
		for (j = 0, odest += 0x10; j < *destLen; j++)
		{
			*(odest++) = *(dest + j);
		}
		for (j = 0; j < 0x100; j++)
		{
			*(odest++) = *(src + j);
		}
		*destLen += 0x110;
		return *destLen;
	}

}