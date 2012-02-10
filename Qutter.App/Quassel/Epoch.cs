using System;

namespace Qutter
{
	class Epoch
	{
		static readonly DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0);

		static readonly DateTimeOffset epochDateTimeOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

		public static DateTime FromUnix(int secondsSinceepoch)
		{
			return epochStart.AddSeconds(secondsSinceepoch);
		}

		public static DateTimeOffset FromUnix(int secondsSinceEpoch, int timeZoneOffsetInMinutes)
		{
			var utcDateTime = epochDateTimeOffset.AddSeconds(secondsSinceEpoch);
			var offset = TimeSpan.FromMinutes(timeZoneOffsetInMinutes);
			return new DateTimeOffset(utcDateTime.DateTime.Add(offset), offset);
		}

		public static int ToUnix(DateTime dateTime)
		{
			return (int)(dateTime - epochStart).TotalSeconds;
		}

		public static int Now {
			get {
				return (int)(DateTime.UtcNow - epochStart).TotalSeconds;
			}
		}
	}
}