using System;
using MiscUtil.IO;

namespace Qutter
{
	public class QCharSerializer : QMetaTypeSerializer<char>
	{
		public void Serialize(EndianBinaryWriter bw, char data)
		{
			bw.WriteChar(data);
		}

		public char Deserialize(EndianBinaryReader br, Type type)
		{
			return (char)br.ReadInt16();
		}
	}
}

