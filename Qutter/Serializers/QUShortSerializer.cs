using System;
using MiscUtil.IO;

namespace Qutter
{
	public class QUShortSerializer : QMetaTypeSerializer<ushort>
	{
		public void Serialize (EndianBinaryWriter bw, ushort data)
		{
			bw.Write(data);
		}

		public ushort Deserialize (EndianBinaryReader br, Type type)
		{
			return br.ReadUInt16();
		}
	}
}

