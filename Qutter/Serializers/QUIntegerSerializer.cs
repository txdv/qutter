using System;
using MiscUtil.IO;

namespace Qutter
{
	public class QUIntegerSerializer : QMetaTypeSerializer<uint>
	{
		public void Serialize(EndianBinaryWriter bw, uint data)
		{
			bw.Write(data);
		}

		public uint Deserialize(EndianBinaryReader br, Type type)
		{
			return br.ReadUInt32();
		}
	}
}

