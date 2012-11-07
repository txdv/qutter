using System;
using System.Text;
using MiscUtil.IO;

namespace Qutter
{
	public class QStringSerializer : QMetaTypeSerializer<string>
	{
		public void Serialize(EndianBinaryWriter bw, string data)
		{
			if (data == null) {
				bw.Write(-1);
			} else {
				byte[] byteData = Encoding.UTF8.GetBytes(data);
				bw.Write(byteData.Length * 2);
				foreach (byte b in byteData) {
					bw.WriteChar((char)b);
				}
			}
		}

		public string Deserialize(EndianBinaryReader br, Type type)
		{
			int len = br.ReadInt32();
			if (len == -1) {
				return null;
			}

			byte[] strData = br.ReadBytes(len);

			for (int i = 0; i < strData.Length / 2; i++) {
				strData[i] = strData[i * 2 + 1];
			}

			return Encoding.UTF8.GetString(strData, 0, strData.Length / 2);
		}
	}
}

