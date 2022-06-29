// LibCRIComp.h

#pragma once

using namespace System;

namespace LibCRIComp {

	public ref class CriCompression
	{
		public:static int CRIcompress(unsigned char *dest, int *destLen, unsigned char *src, int srcLen);
	};
}
