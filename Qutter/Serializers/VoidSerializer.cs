using System;
using MiscUtil.IO;

namespace Qutter
{
	public class VoidSerializer : QMetaTypeSerializer<object>
	{
		public void Serialize(EndianBinaryWriter bw, object data)
		{
		}

		public object Deserialize(EndianBinaryReader br, Type type)
		{
			return null;
		}
	}
}

