using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace Qutter
{
  public static class EndianBinaryWriterExtensions
  {
    public static void WriteChar(this EndianBinaryWriter bw, char c)
    {
      bw.Write((byte)(c >> 8 & 0xFF));
      bw.Write((byte)(c      & 0xFF));
    }
  }
  
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
  }
  
  public class QTypeManager
  {
    private static readonly IDictionary<Type,      Type> typeDict  = new Dictionary<Type,      Type>();
    private static readonly IDictionary<QMetaType, Type> metaTypes = new Dictionary<QMetaType, Type>();
    private static readonly IDictionary<Type, QMetaType> dnetTypes = new Dictionary<Type, QMetaType>();

    private static void Register(Type type, Type metaTypeSerializer, QMetaType metaType)
    {
      typeDict[type] = metaTypeSerializer;
      if (!metaTypeSerializer.IsGenericType || (metaTypeSerializer.IsGenericType && type != type.GetGenericTypeDefinition())) {
        var o = metaTypeSerializer.GetConstructor(new Type[] { }).Invoke(new object[] { });

        if (metaType == QMetaType.None) {
          return;
        }
        
        metaTypes[metaType] = type;
        dnetTypes[type]     = metaType;
      }
    }
    
    private static readonly IDictionary<string, Type> userDefined = new Dictionary<string, Type>();
    
    public static void RegisterUserDefinedMetaType(string typeName, Type metaTypeSerializer)
    {
      userDefined[typeName] = metaTypeSerializer;
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
      if (typeDict.ContainsKey(type))
        return typeDict[type];
      
      if (type.IsGenericType && typeDict.ContainsKey(type)) {
        return typeDict[type];
      } else {
        Type genericType = type.GetGenericTypeDefinition();
        Type serializer = typeDict[genericType];
        
        if (serializer == null)
          return null;
        
        return genericType.MakeGenericType(type.GetGenericArguments());
      }
    }
    
    internal static object GetMetaTypeSerializerInstance(Type type)
    {
      return GetMetaTypeSerializer(type).GetConstructor(new Type[] { }).Invoke(new object[] { });
    }
    
    internal static Type GetMetaTypeSerializer(string name)
    {
      if (!userDefined.ContainsKey(name))
        return null;
      
      return userDefined[name];
    }
    
    internal static object GetMetaTypeSerializerInstance(string name)
    {
      return GetMetaTypeSerializer(name).GetConstructor(new Type[] { }).Invoke(new object[] { });
    }
    
    internal static Type GetMetaTypeSerializer(QMetaType type)
    {
      if (metaTypes.ContainsKey(type))
        return GetMetaTypeSerializer(metaTypes[type]);
      else
        return null;
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
    
    public static void Serialize(Stream stream, object data)
    {
      Serialize(new EndianBinaryWriter(EndianBitConverter.Big, stream), data);
    }
    
    internal static object Deserialize(EndianBinaryReader br, Type type)
    {
      return Invoke(type, "Deserialize", new object[] { br, type });
    }
    
    internal static object Deserialize(Stream stream, Type type)
    {
      return Deserialize(new EndianBinaryReader(EndianBitConverter.Big, stream), type);
    }
    
    public static T Deserialize<T>(Stream stream)
    {
      return (T)Deserialize(stream, typeof(T));
    }
    
    public static void Deserialize<T>(Stream stream, out T variable)
    {
      variable = Deserialize<T>(stream);
    }
  }
  
  public class QVariant
  {
    internal QVariant(object value, QMetaType type)
    {
      Value = value;
      Type = type;
    }
    
    public QVariant(object value)
      : this(value, QTypeManager.GetType(value.GetType()))
    {
    }
    
    public object Value { get; protected set; }
    public QMetaType Type { get; protected set; }
    
    /// <summary>
    /// Qt like function for retrieving the value
    /// returns null, if the types expected type
    /// differs from the actual
    /// </summary>
    public T GetValue<T>()
    {
      if (typeof(T) != Value.GetType()) {
        return default(T);
      }
      
      return (T)Value;
    }
  }
  
  public interface QMetaTypeSerializer<T>
  {
    void Serialize(EndianBinaryWriter bw, T data);
    T Deserialize(EndianBinaryReader br, Type type);
  }
  
  public class VoidSerializer : QMetaTypeSerializer<object>
  {
    public void Serialize(EndianBinaryWriter bw, object data)
    {
    }
    
    public object Deserialize(EndianBinaryReader br, Type type)
    {
      return null;
    }
  }
  
  public class BoolSerializer : QMetaTypeSerializer<bool>
  {
    public void Serialize(EndianBinaryWriter bw, bool data)
    {
      bw.Write(data);
    }
    
    public bool Deserialize(EndianBinaryReader br, Type type)
    {
      return br.ReadBoolean();
    }
  }
  
  public class QIntegerSerializer : QMetaTypeSerializer<int>
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
  
  public class QUIntegerSerializer : QMetaTypeSerializer<uint>
  {
    public void Serialize(EndianBinaryWriter bw, uint data)
    {
      bw.Write(data);
    }
    
    public uint Deserialize(EndianBinaryReader br, Type type)
    {
      return br.ReadUInt32();
    }
  }
  
  public class QCharSerializer : QMetaTypeSerializer<char>
  {
    public void Serialize(EndianBinaryWriter bw, char data)
    {
      bw.WriteChar(data);
    }
    
    public char Deserialize(EndianBinaryReader br, Type type)
    {
      return (char)br.ReadInt16();
    }
  }

  public class QStringSerializer : QMetaTypeSerializer<string>
  {
    public void Serialize(EndianBinaryWriter bw, string data)
    {
      if (data == null) {
        bw.Write(-1);
      } else {
        byte[] byteData = Encoding.UTF8.GetBytes(data);
        bw.Write(byteData.Length * 2);
        foreach (byte b in byteData) {
          bw.WriteChar((char)b);
        }
      }
    }
    
    public string Deserialize(EndianBinaryReader br, Type type)
    {
      int len = br.ReadInt32();
      if (len == -1)
        return null;
      
      byte[] strData = br.ReadBytes(len);

      // FIXME: this doesn't seem to conform to qt unicode serialization
      // I don't know why, breakes special characters ...
      return UnicodeEncoding.BigEndianUnicode.GetString(strData);
    }
  }
  
  public class QByteArraySerializer : QMetaTypeSerializer<byte[]>
  {
    public void Serialize(EndianBinaryWriter bw, byte[] data)
    {
      if (data == null) {
        bw.Write(-1);
      } else {
        bw.Write(data.Length);
        bw.Write(data);
      }
    }
    
    public byte[] Deserialize(EndianBinaryReader br, Type type)
    {
      int len = br.ReadInt32();
      if (len == -1)
       return null; 
      
      return br.ReadBytes(len);
    }
  }  
  
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
  
  public class QVariantSerializer : QMetaTypeSerializer<QVariant>
  {
    public void Serialize(EndianBinaryWriter bw, QVariant data)
    {
      bw.Write((int)data.Type);
      
      if (data.Value == null) {
        bw.Write((byte)1);
      } else {
        bw.Write((byte)0);
        QTypeManager.Serialize(bw, data.Value);
      }
    }
    
    public QVariant Deserialize(EndianBinaryReader br, Type type)
    {
      QMetaType metaType = (QMetaType)br.ReadUInt32();

      int n = br.BaseStream.ReadByte();
      if (metaType == QMetaType.UserType) {
        byte[] byteData;
        QTypeManager.Deserialize(br.BaseStream, out byteData);
        string name = Encoding.ASCII.GetString(byteData, 0, byteData.Length - 1);
        Type t = QTypeManager.GetMetaTypeSerializer(name);

        if (t == null)
          throw new Exception(string.Format("UserType({0}) not registered", name));
        
        object o = QTypeManager.Invoke(name, "Deserialize", new object[] { br, type });
        return new QVariant(o, QMetaType.UserType);
      }
      
      object data = null;
      if (n == 0) {
        data = QTypeManager.Deserialize(br, QTypeManager.GetType(metaType));
      }

      if (data == null) {
        return new QVariant(data, metaType);
      }

      return new QVariant(data);
    }
  }
  
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

  public class QUShortSerializer : QMetaTypeSerializer<ushort>
  {
    public void Serialize (EndianBinaryWriter bw, ushort data)
    {
      bw.Write(data);
    }

    public ushort Deserialize (EndianBinaryReader br, Type type)
    {
      return br.ReadUInt16();
    }
  }
}
