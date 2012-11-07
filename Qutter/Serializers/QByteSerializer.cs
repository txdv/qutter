using System;
using MiscUtil.IO;

namespace Qutter
{
	public class QByteArraySerializer : QMetaTypeSerializer<byte[]>
	{
		public void Serialize(EndianBinaryWriter bw, byte[] data)
		{
			if (data == null) {
				bw.Write(-1);
			} else {
				bw.Write(data.Length);
				bw.Write(data);
			}
		}

		public byte[] Deserialize(EndianBinaryReader br, Type type)
		{
			int len = br.ReadInt32();
			if (len == -1) {
				return null;
			}
			return br.ReadBytes(len);
		}
	}
}

