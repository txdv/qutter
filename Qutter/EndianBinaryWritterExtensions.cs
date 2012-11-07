using System;
using MiscUtil.IO;

namespace Qutter
{
	public static class EndianBinaryWriterExtensions
	{
		public static void WriteChar(this EndianBinaryWriter bw, char c)
		{
			bw.Write((byte)(c >> 8 & 0xFF));
			bw.Write((byte)(c      & 0xFF));
		}
	}
}
