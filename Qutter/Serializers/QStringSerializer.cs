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

			// FIXME: this doesn't seem to conform to qt unicode serialization
			// I don't know why, breakes special characters ...
			return UnicodeEncoding.BigEndianUnicode.GetString(strData);
		}
	}
}

