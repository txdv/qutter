using System;
using MiscUtil.IO;

namespace Qutter
{
	public class BoolSerializer : QMetaTypeSerializer<bool>
	{
		public void Serialize(EndianBinaryWriter bw, bool data)
		{
			bw.Write(data);
		}

		public bool Deserialize(EndianBinaryReader br, Type type)
		{
			return br.ReadBoolean();
		}
	}
}

