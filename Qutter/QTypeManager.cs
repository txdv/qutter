using System;
using System.Collections.Generic;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace Qutter
{
	public class QTypeManager
	{
		private static readonly IDictionary<Type,      Type> typeDict  = new Dictionary<Type,      Type>();
		private static readonly IDictionary<QMetaType, Type> metaTypes = new Dictionary<QMetaType, Type>();
		private static readonly IDictionary<Type, QMetaType> dnetTypes = new Dictionary<Type, QMetaType>();
		private static readonly IDictionary<Type,      Type> userTypes = new Dictionary<Type,      Type>();

		private static void Register(Type type, Type metaTypeSerializer, QMetaType metaType)
		{
			typeDict[type] = metaTypeSerializer;
			if (!metaTypeSerializer.IsGenericType || (metaTypeSerializer.IsGenericType && type != type.GetGenericTypeDefinition())) {
				var o = metaTypeSerializer.GetConstructor(new Type[] { }).Invoke(new object[] { });

				if (metaType == QMetaType.None) {
					return;
				}

				metaTypes[metaType] = type;
				dnetTypes[type] = metaType;
			}
		}

		private static readonly IDictionary<string, Type> userDefined = new Dictionary<string, Type>();

		public static void Register(string typeName, Type metaTypeSerializer)
		{
			userDefined[typeName] = metaTypeSerializer;

			var type = metaTypeSerializer.GetMethod("Deserialize").ReturnType;

			if (!type.IsValueType) {
				userTypes[type] = metaTypeSerializer;
			}
		}

		static QTypeManager()
		{
			// non generic values
			Register(typeof(void)    , typeof(VoidSerializer)      , QMetaType.Void);
			Register(typeof(bool)    , typeof(BoolSerializer)      , QMetaType.Bool);
			Register(typeof(int)     , typeof(QIntegerSerializer)  , QMetaType.Int);
			Register(typeof(uint)    , typeof(QUIntegerSerializer) , QMetaType.UInt);
			Register(typeof(char)    , typeof(QCharSerializer)     , QMetaType.QChar);
			Register(typeof(byte[])  , typeof(QByteArraySerializer), QMetaType.QByteArray);
			Register(typeof(string)  , typeof(QStringSerializer)   , QMetaType.QString);
			Register(typeof(DateTime), typeof(QTimeSerializer)     , QMetaType.QTime);
			Register(typeof(DateTime), typeof(QDateTimeSerializer) , QMetaType.QDateTime);
			Register(typeof(ushort)  , typeof(QUShortSerializer)   , QMetaType.UShort);

			// special classes
			Register(typeof(QVariant)                    , typeof(QVariantSerializer)              , QMetaType.None);
			Register(typeof(Dictionary<string, QVariant>), typeof(QMapSerializer<string, QVariant>), QMetaType.QVariantMap);
			Register(typeof(List<QVariant>)              , typeof(QListSerializer<QVariant>)       , QMetaType.QVariantList);
			Register(typeof(List<string>)                , typeof(QListSerializer<string>)         , QMetaType.QStringList);

			// generic definitions
			Register(typeof(List<>),        typeof(QListSerializer<>), QMetaType.None);
			Register(typeof(Dictionary<,>), typeof(QMapSerializer<,>), QMetaType.None);
		}

		internal static QMetaType GetType(Type type)
		{
			if (userTypes.ContainsKey(type)) {
				return QMetaType.UserType;
			}

			if (!dnetTypes.ContainsKey(type)) {
				throw new Exception(string.Format("No such dnetType in cache {0})", type));
			}
			return dnetTypes[type];
		}

		internal static Type GetType(QMetaType type)
		{
			if (!metaTypes.ContainsKey(type)) {
				throw new Exception(string.Format("No such MetaType in cache {0}({1})", type, (int)type));
			}
			return metaTypes[type];
		}

		internal static Type GetMetaTypeSerializer(Type type)
		{
			if (userTypes.ContainsKey(type)) {
				return userTypes[type];
			}

			if (typeDict.ContainsKey(type)) {
				return typeDict[type];
			}

			if (type.IsGenericType && typeDict.ContainsKey(type)) {
				return typeDict[type];
			} else {
				Type genericType = type.GetGenericTypeDefinition();
				Type serializer = typeDict[genericType];

				if (serializer == null) {
					return null;
				}

				return genericType.MakeGenericType(type.GetGenericArguments());
			}
		}

		internal static object GetMetaTypeSerializerInstance(Type type)
		{
			return GetMetaTypeSerializer(type).GetConstructor(new Type[] { }).Invoke(new object[] { });
		}

		internal static Type GetMetaTypeSerializer(string name)
		{
			if (!userDefined.ContainsKey(name)) {
				return null;
			}

			return userDefined[name];
		}

		internal static object GetMetaTypeSerializerInstance(string name)
		{
			return GetMetaTypeSerializer(name).GetConstructor(new Type[] { }).Invoke(new object[] { });
		}

		internal static Type GetMetaTypeSerializer(QMetaType type)
		{
			if (metaTypes.ContainsKey(type)) {
				return GetMetaTypeSerializer(metaTypes[type]);
			} else {
				return null;
			}
		}

		private static Type GetMetaTypeSerializer(int type)
		{
			return GetMetaTypeSerializer((QMetaType)type);
		}

		public static object Invoke(string type, string method, object[] data)
		{
			var o = GetMetaTypeSerializer(type).GetConstructor(new Type[] { }).Invoke(new object[] { });
			return o.GetType().GetMethod(method).Invoke(o, data);
		}

		public static object Invoke(Type type, string method, object[] data)
		{
			if (type.IsGenericType) {
				Type genericType = type.GetGenericTypeDefinition();
				Type serializerType = GetMetaTypeSerializer(genericType);
				var o = serializerType.MakeGenericType(type.GetGenericArguments()).GetConstructor(new Type[] { }).Invoke(new object[] { });
				return o.GetType().GetMethod(method).Invoke(o, data);
			} else {
				Type serializerType = GetMetaTypeSerializer(type);
				var o = serializerType.GetConstructor(new Type[] { }).Invoke(new object[] { });
				return serializerType.GetMethod(method).Invoke(o, data);
			}
		}

		public static void Serialize(EndianBinaryWriter bw, object data)
		{
			Invoke(data.GetType(), "Serialize", new object[] { bw, data });
		}

		public static void Serialize(System.IO.Stream stream, object data)
		{
			Serialize(new EndianBinaryWriter(EndianBitConverter.Big, stream), data);
		}

		public static void Serialize<T>(EndianBinaryWriter bw, object data)
		{
			Invoke(typeof(T), "Serialize", new object[] { bw, data });
		}

		public static void Serialize<T>(System.IO.Stream stream, object data)
		{
			Serialize<T>(new EndianBinaryWriter(EndianBitConverter.Big, stream), data);
		}

		internal static object Deserialize(EndianBinaryReader br, Type type)
		{
			return Invoke(type, "Deserialize", new object[] { br, type });
		}

		internal static object Deserialize(System.IO.Stream stream, Type type)
		{
			return Deserialize(new EndianBinaryReader(EndianBitConverter.Big, stream), type);
		}

		public static T Deserialize<T>(System.IO.Stream stream)
		{
			return (T)Deserialize(stream, typeof(T));
		}

		public static void Deserialize<T>(System.IO.Stream stream, out T variable)
		{
			variable = Deserialize<T>(stream);
		}
	}
}

