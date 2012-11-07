using System;
using MiscUtil.IO;

namespace Qutter
{
	public interface QMetaTypeSerializer<T>
	{
		void Serialize(EndianBinaryWriter bw, T data);
		T Deserialize(EndianBinaryReader br, Type type);
	}
}

