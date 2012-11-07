using System;
using MiscUtil.IO;

namespace Qutter
{
	public class QIntegerSerializer : QMetaTypeSerializer<int>
	{
		public void Serialize(EndianBinaryWriter bw, int data)
		{
			bw.Write(data);
		}

		public int Deserialize(EndianBinaryReader br, Type type)
		{
			return br.ReadInt32();
		}
	}
}

