using System;
using System.Text;
using MiscUtil.IO;

namespace Qutter
{
	public class QVariantSerializer : QMetaTypeSerializer<QVariant>
	{
		public void Serialize(EndianBinaryWriter bw, QVariant data)
		{
			bw.Write((int)data.Type);

			if (data.Value == null) {
				if (data.Type == QMetaType.QString) {
					bw.Write((byte)0);
					QTypeManager.Serialize<string>(bw, null);
				} else {
					bw.Write((byte)1);
				}
			} else {
				bw.Write((byte)0);
				if (data.IsUserType) {
					string name = data.UserTypeName;
					byte[] nameBytes = new byte[name.Length + 1];
					Encoding.ASCII.GetBytes(name).CopyTo(nameBytes, 0);
					QTypeManager.Serialize(bw, nameBytes);
				}
				QTypeManager.Serialize(bw, data.Value);
			}
		}

		public QVariant Deserialize(EndianBinaryReader br, Type type)
		{
			QMetaType metaType = (QMetaType)br.ReadUInt32();

			int n = br.BaseStream.ReadByte();
			if (metaType == QMetaType.UserType) {
				byte[] byteData;
				QTypeManager.Deserialize(br.BaseStream, out byteData);
				string name = Encoding.ASCII.GetString(byteData, 0, byteData.Length - 1);
				Type t = QTypeManager.GetMetaTypeSerializer(name);

				if (t == null) {
					throw new Exception(string.Format("UserType({0}) not registered", name));
				}

				if (t.GetMethod("Deserialize") != null) {
					t = t.GetMethod("Deserialize").ReturnType;
				}

				object o = QTypeManager.Invoke(name, "Deserialize", new object[] { br, t });
					return new QVariant(o, name);
				}

				object data = null;

				switch (metaType) {
				case QMetaType.QVariantList:
					data = QTypeManager.Deserialize(br, QTypeManager.GetType(metaType));
					break;
				default:
					if (n == 0) {
						data = QTypeManager.Deserialize(br, QTypeManager.GetType(metaType));
					}
					break;
				}

				if (data == null) {
					return new QVariant(data, metaType);
				}

				return new QVariant(data);
			}
	}
}

