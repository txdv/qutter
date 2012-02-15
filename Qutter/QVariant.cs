using System;
using System.Text;
using System.Collections.Generic;

namespace Qutter
{
	public class QVariant
	{
		public QVariant(object value, string userTypeName)
		{
			Value = value;
			Type = QMetaType.UserType;
			UserTypeName = userTypeName;
		}

		public QVariant(object value, QMetaType type)
		{
			Value = value;
			Type = type;
		}

		public QVariant(Type type)
		: this(null, QTypeManager.GetType(type))
		{
		}

		public QVariant(object value)
		: this(value, QTypeManager.GetType(value.GetType()))
		{
		}

		public object Value { get; protected set; }
		public QMetaType Type { get; protected set; }

		public bool IsUserType {
			get {
				return Type == QMetaType.UserType;
			}
		}

		public string UserTypeName { get; protected set; }

		public T GetValue<T>()
		{
			if (typeof(T) != Value.GetType()) {
				return default(T);
			}

			return (T)Value;
		}

		public static string Inspect(object o)
		{
			return Inspect(o, 0);
		}

		static string Inspect(object o, int level)
		{
			if (o == null) {
				return "(null)";
			} else if (o is Dictionary<string, QVariant>) {
				string ret = "{ ";
				foreach (var k in (Dictionary<string, QVariant>)o) {
					ret += string.Format("(\"{0}\", {1}), ", k.Key, Inspect(k.Value, level + 1));
				}
				return ret + " }";
			} else if (o is QVariant) {
				QVariant var = o as QVariant;
				if (var.IsUserType) {
					return string.Format("QVariant({0}:{1}, {2})", var.Type, var.UserTypeName, Inspect(var.Value, level + 1));
				} else {
					return string.Format("QVariant({0}, {1})", var.Type, Inspect(var.Value, level + 1));
				}
			} else if (o is List<QVariant>) {
				string ret = "[ ";
				var list = o as List<QVariant>;
				for (int i = 0; i < list.Count; i++) {
					QVariant le = list[i];
					ret += string.Format("{0}", Inspect(le, level + 1));
					bool last = list.Count == i + 1;
					if (!last) {
						ret += ", ";
					}
				}
				return ret + " ]";
			} else if (o is string) {
				return string.Format("\"{0}\"", o);
			} else if (o is byte[]) {
				return string.Format("byte[] \"{0}\"", Encoding.ASCII.GetString(o as byte[]));
			} else {
				return o.ToString();
			}
		}
	}
}

