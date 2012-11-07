using System;
using System.Collections;
using System.Collections.Generic;
using MiscUtil.IO;

namespace Qutter
{
	public class QMapSerializer<T1, T2> : QMetaTypeSerializer<Dictionary<T1, T2>>
	{
		public void Serialize(EndianBinaryWriter bw, Dictionary<T1, T2> data)
		{
			if (data == null) {
				bw.Write(-1);
			} else {
				int len = (int)data.GetType().GetProperty("Count").GetValue(data, new object[] {});

				bw.Write(len);

				foreach (DictionaryEntry kvp in (IDictionary)data) {
					QTypeManager.Serialize(bw, kvp.Key);
					QTypeManager.Serialize(bw, kvp.Value);
				}
			}
		}

		public Dictionary<T1, T2> Deserialize(EndianBinaryReader br, Type type)
		{
			int len = br.ReadInt32();

			if (len == -1) {
				return null;
			}

			var mapType = type.GetGenericTypeDefinition().MakeGenericType(type.GetGenericArguments());
			Dictionary<T1, T2> map = (Dictionary<T1, T2>)mapType.GetConstructor(new Type[] {}).Invoke(new object[] { });

			Type keyType = type.GetGenericArguments()[0];
			Type valueType = type.GetGenericArguments()[1];
			var mapAddMethod = mapType.GetMethod("Add");

			for (int i = 0; i < len; i++) {
				object key   = QTypeManager.Deserialize(br, keyType);
				object value = QTypeManager.Deserialize(br, valueType);
				mapAddMethod.Invoke(map, new object[] { key, value });
			}

			return map;
		}
	}
}

