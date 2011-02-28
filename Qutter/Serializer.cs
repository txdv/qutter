using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Qutter.BinaryExtensions;
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
    
    QString    = 10,
    QByteArray = 12,
    
    UserType = 127,
  }
  
  public class QTypeManager
  {
    private static readonly IDictionary<Type,      Type> typeDict    = new Dictionary<Type,      Type>();
    private static readonly IDictionary<QMetaType, Type> typeNumDict = new Dictionary<QMetaType, Type>();
    
    static void Register(Type type, Type metaTypeSerializer)
    {
      typeDict[type] = metaTypeSerializer;
      if (!metaTypeSerializer.IsGenericType) {
        var o = metaTypeSerializer.GetConstructor(new Type[] { }).Invoke(new object[] { });
        QMetaType metaType = (QMetaType)metaTypeSerializer.GetProperty("Type").GetValue(o, new object[] { });
        typeNumDict[metaType] = metaTypeSerializer;
      }
    }
    
    static QTypeManager()
    {
      // non generic values
      Register(typeof(void),   typeof(VoidSerializer));
      Register(typeof(bool),   typeof(BoolSerializer));
      Register(typeof(int),    typeof(QIntegerSerializer));
      Register(typeof(char),   typeof(QCharSerializer));
      Register(typeof(byte[]), typeof(QByteArraySerializer));
      Register(typeof(string), typeof(QStringSerializer));
      
      // special classes
      Register(typeof(QVariant<>), typeof(QVariantSerializer<>));
      
      // generic definitions
      Register(typeof(List<>),        typeof(QListSerializer<>));
      Register(typeof(Dictionary<,>), typeof(QMapSerializer<,>));
    }
    
    internal static Type GetMetaTypeSerializer(Type type)
    {
        if (typeDict.ContainsKey(type))
          return typeDict[type];
        
        if ((type.IsGenericType) && typeDict.ContainsKey(type.GetGenericTypeDefinition()))
          return typeDict[type.GetGenericTypeDefinition()];
        
        return null;
    }
    
    internal static Type GetMetaTypeSerializer(QMetaType type)
    {
      return typeNumDict[type];
    }
    
    private static Type GetMetaTypeSerializer(int type)
    {
      return GetMetaTypeSerializer((QMetaType)type);
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
  
  public static class QVariant
  {
    public static Type MakeGenericType(object qvariant)
    {
      Type type = qvariant.GetType().GetGenericArguments()[0];
      return typeof(QVariant<>).MakeGenericType(new Type[] { type });
    }
      
    public static QMetaType GetType(object qvariant)
    {
      Type t = MakeGenericType(qvariant);
      return (QMetaType)t.GetProperty("Type").GetValue(qvariant, new object[] { });
    }
    
    public static object GetValue(object qvariant)
    {
      var t = MakeGenericType(qvariant);
      return t.GetProperty("Value").GetValue(qvariant, new object[] { });
    }
    
    public static Type GetType(QMetaType type)
    {
      switch (type) {
      case QMetaType.Bool:
        return typeof(bool);
      case QMetaType.QString:
        return typeof(string);
      case QMetaType.QChar:
        return typeof(char);
      default:
        return null;
      }
    }
  }
  
  public class QVariant<T>
  {
    private QVariant(T value, QMetaType type)
    {
      Value = value;
      Type  = type;
    }
    
    public QVariant(T value)
      : this(value, QMetaType.None)
    {
    }
    
    public T Value { get; protected set; }
    public QMetaType Type { get; protected set; }
  }
  
  
  // TODO: implement a working class of this
  public class QVariantMap<T> : Dictionary<string, QVariant<T>>
  {
  }
  
  // TODO: implement a working class of this
  public class QVariantList<T> : List<QVariant<T>>
  {
  }
  
  public interface QMetaTypeSerializer<T>
  {
    QMetaType Type { get; }
    void Serialize(EndianBinaryWriter bw, T data);
    T Deserialize(EndianBinaryReader br, Type type);
  }
  
  public class VoidSerializer : QMetaTypeSerializer<object>
  {
    public QMetaType Type { get { return QMetaType.Void; } }
    
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
    public QMetaType Type { get { return QMetaType.Bool; } }
    
    public string Name { get { return "Bool"; } }
    
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
    public QMetaType Type { get { return QMetaType.Int; } }
    
    public string Name { get { return "QInteger"; } }
    
    public void Serialize(EndianBinaryWriter bw, int data)
    {
      bw.Write(data);
    }
    
    public int Deserialize(EndianBinaryReader br, Type type)
    {
      return br.ReadInt32();
    }
  }
  
  public class QCharSerializer : QMetaTypeSerializer<char>
  {
    public QMetaType Type { get { return QMetaType.QChar; } }
    
    public string Name { get { return "QChar"; } }
    
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
    public QMetaType Type { get { return QMetaType.QString; } }
    
    public string Name { get { return "QString"; } }
    
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
    public QMetaType Type { get { return QMetaType.QByteArray; } }
    
    public string Name { get { return "QByteArray"; } }
    
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
    public QMetaType Type { get { return QMetaType.None; } }
    
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
      
      if (len == -1)
        return null; 
      
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
    public QMetaType Type { get { return QMetaType.None; } }
    
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
      
      if (len == -1)
        return null;
      
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
  
  public class QVariantSerializer<T> : QMetaTypeSerializer<List<T>>
  {
    public QMetaType Type { get { return QMetaType.None; } }
    
    public void Serialize(EndianBinaryWriter bw, List<T> data)
    {
      
      var t = typeof(QVariant<>).MakeGenericType(new Type[] { data.GetType() });
      List<T> o = (List<T>)t.GetConstructor(new Type[] { }).Invoke(new object[] { data });
      
      //QVariant variant = data as QVariant;
      var qvariantValue = t.GetProperty("Value").GetValue(o, new object[] { });
      
      bw.Write((int)QVariant.GetType(o));
      
      if (qvariantValue == null) {
        bw.Write((byte)1);
      } else {
        bw.Write((byte)0);
        QTypeManager.Serialize(bw, qvariantValue);
      }
    }
    
    public List<T> Deserialize(EndianBinaryReader br, Type type)
    {
      
      QMetaType metaType = (QMetaType)br.ReadUInt32();
      int n = br.BaseStream.ReadByte();
      
      Type dotNetType = QVariant.GetType(metaType);
      
      if (n == 1) {
        
      } else { 
        Type metaTypeSerializer = QTypeManager.GetMetaTypeSerializer(metaType);
        Console.WriteLine(metaTypeSerializer);
        //if (metaTypeSerializer == null) {
        //  Console.WriteLine("PZDC");
        //}
      }
      
      
      /*
      if (n == 1) {
        return new QVariant(metaType);
      } else {
        object data = QTypeManager.GetMetaTypeSerializer(metaType).Deserialize(br, type);
        if (data == null)
          return new QVariant(metaType);
        
        switch (metaType) {
        case QMetaType.Int:
          return new QVariant((int)data);
        case QMetaType.QString:
          return new QVariant((string)data);
        default:
          return new QVariant(metaType);
        }
      }
      */
      return null;
    }
  }
}
