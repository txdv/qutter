using System;
using Qutter;
using MiscUtil.IO;

namespace Qutter.App
{
	enum RequestType
	{
		Sync           = 1,
		RpcCall        = 2,
		InitRequest    = 3,
		InitData       = 4,
		HeartBeat      = 5,
		HeartBeatReply = 6
	}

	public class IdSerializer : QMetaTypeSerializer<int>
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

