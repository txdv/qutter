using System;
using MiscUtil.IO;

namespace Qutter
{
	public class QTimeSerializer : QMetaTypeSerializer<TimeSpan>
	{
		public void Serialize(EndianBinaryWriter bw, TimeSpan data)
		{
			long sum = data.Hours * 3600000;
			sum += data.Minutes * 60000;
			sum += data.Seconds * 1000;
			sum += data.Milliseconds;
			bw.Write((uint)sum);
		}

		public TimeSpan Deserialize(EndianBinaryReader br, Type type)
		{
			long millisSinceMidnight = br.ReadUInt32();
			int hour =   (int)(millisSinceMidnight / 3600000);
			int minute = (int)((millisSinceMidnight - (hour*3600000))/60000);
			int second = (int)((millisSinceMidnight - (hour*3600000) - (minute*60000))/1000);
			int millis = (int)((millisSinceMidnight - (hour*3600000) - (minute*60000) - (second * 1000)));
			return new TimeSpan(0, hour, minute, second, millis);
		}
	}
}

