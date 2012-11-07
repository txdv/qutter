using System;
using System.Collections;
using System.Collections.Generic;
using MiscUtil.IO;

namespace Qutter
{
	public class QListSerializer<T> : QMetaTypeSerializer<List<T>>
	{
		public void Serialize(EndianBinaryWriter bw, List<T> data)
		{
			int len = (int)data.GetType().GetProperty("Count").GetValue(data, new object[] {});

			bw.Write(len);

			foreach (object obj in (IEnumerable)data) {
				QTypeManager.Serialize(bw, obj);
			}
		}

		public List<T> Deserialize(EndianBinaryReader br, Type type)
		{
			int len = br.ReadInt32();

			if (len == -1) {
				return null;
			}

			var listDef = type.GetGenericTypeDefinition().MakeGenericType(type.GetGenericArguments());
			List<T> list = (List<T>)listDef.GetConstructor(new Type[] {}).Invoke(new object[] { });

			Type listElementType = type.GetGenericArguments()[0];

			for (int i = 0; i < len; i++) {
				T listElement = (T)QTypeManager.Deserialize(br, listElementType);
				list.Add(listElement);
			}
			return list;
		}
	}
}

