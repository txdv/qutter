using System;
using MiscUtil.IO;

namespace Qutter
{
	public class QDateTimeSerializer : QMetaTypeSerializer<DateTime>
	{
		public void Serialize(EndianBinaryWriter bw, DateTime data)
		{
			int a = (14 - data.Month) / 12;
			int y = data.Year + 4800 - a;
			int m = data.Month + 12 * a - 3;
			int jdn = data.Day + (153 * m + 2) / 5 + 365 * y + y / 4 - y/100 + y/400 - 32045;
			bw.Write(jdn);

			int secondsSinceMidnight = data.Hour * 3600 + data.Minute * 60 + data.Second;
			bw.Write(secondsSinceMidnight);

			// TODO: fix this
			bw.Write((byte)0);
		}
		public DateTime Deserialize(EndianBinaryReader br, Type type)
		{
			// code taken from quasseldroid, i dont know what this shit does.
			long julianDay = br.ReadUInt32();
			long secondsSinceMidnight = br.ReadUInt32();
			long isUTC = br.ReadByte();

			double J = (double)(julianDay) + 0.5f;
			long j = (int) (J + 32044);
			long g = j / 146097;
			long dg = j % 146097;
			long c = (((dg / 36524) + 1) * 3) / 4;
			long dc = dg - c * 36524;
			long b = dc / 1461;
			long db = dc % 1461;
			long a = (db / 365 + 1) * 3 / 4;
			long da = db - a * 365;
			long y = g * 400 + c * 100 + b * 4 + a;
			long m = (da * 5 + 308) / 153 - 2;
			long d = da - (m + 4) * 153 / 5 + 122;

			int year = (int) (y - 4800 + (m+2)/12);
			int month = (int) ((m+2) % 12 + 1);
			int day = (int) (d + 1);

			int hour = (int) (secondsSinceMidnight / 3600000);
			int minute = (int)((secondsSinceMidnight - (hour*3600000))/60000);
			int second = (int)((secondsSinceMidnight - (hour*3600000) - (minute*60000))/1000);
			int millis = (int)((secondsSinceMidnight - (hour*3600000) - (minute*60000) - (second * 1000)));

			if (isUTC == 1) {
				// TODO: do something about this
			}

			return new DateTime(year, month, day, hour, minute, second, millis);
		}
	}
}

