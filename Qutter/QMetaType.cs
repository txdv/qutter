using System;

namespace Qutter
{
	public enum QMetaType : int
	{
		None      = -1,
		Void      = 0,
		Bool      = 1,
		Int       = 2,
		UInt      = 3,
		LongLong  = 4,
		ULongLong = 5,

		Double       = 6,
		QChar        = 7,
		QVariantMap  = 8,
		QVariantList = 9,

		QString     = 10,
		QStringList = 11,
		QByteArray  = 12,

		QBitArray = 13,
		QDate     = 14,
		QTime     = 15,
		QDateTime = 16,
		QUrl      = 17,

		UserType = 127,

		UShort = 133,
	}}

