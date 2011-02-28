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
  }
  
  public class QTypeManager
  {
    private static readonly IDictionary<Type,      QMetaTypeSerializer> typeDict    = new Dictionary<Type,      QMetaTypeSerializer>();
    private static readonly IDictionary<QMetaType, QMetaTypeSerializer> typeNumDict = new Dictionary<QMetaType, QMetaTypeSerializer>();
      
    static void Register(Type type, QMetaTypeSerializer metaType)
    {
      typeDict[type] = metaType;
      if (metaType.Type != QMetaType.None)
        typeNumDict[metaType.Type] = metaType;
    }
    
    static QTypeManager()
    {
      // basic stuff
      Register(typeof(void),   new VoidSerializer());
      Register(typeof(bool),   new BoolSerializer());
      Register(typeof(int),    new QIntegerSerializer());
      Register(typeof(char),   new QCharSerializer());
      Register(typeof(byte[]), new QByteArraySerializer());
      Register(typeof(string), new QStringSerializer());
      
      // Special non generic classes
      Register(typeof(QVariant), new QVariantSerializer());
      
      // hardcoded generic definitions
      // Register(typeof(Dictionary<string, object>), new QVariantMapSerializer());
      
      // generic definitions
      Register(typeof(List<>),        new QListSerializer());
      Register(typeof(Dictionary<,>), new QMapSerializer());
    }
    
    internal static QMetaTypeSerializer GetMetaTypeSerializer(Type type)
    {
        if (typeDict.ContainsKey(type))
          return typeDict[type];
        
        if ((type.IsGenericType) && typeDict.ContainsKey(type.GetGenericTypeDefinition()))
          return typeDict[type.GetGenericTypeDefinition()];
        
        return null;
    }
    
    internal static QMetaTypeSerializer GetMetaTypeSerializer(QMetaType type)
    {
      return typeNumDict[type];
    }
    
    private static QMetaTypeSerializer GetMetaTypeSerializer(int type)
    {
      return GetMetaTypeSerializer((QMetaType)type);
    }
    
    public static void Serialize(EndianBinaryWriter bw, object data)
    {
      QMetaTypeSerializer serializer = GetMetaTypeSerializer(data.GetType());
      serializer.Serialize(bw, data);
    }
    
    public static void Serialize(Stream stream, object data)
    {
      Serialize(new EndianBinaryWriter(EndianBitConverter.Big, stream), data);
    }
    
    internal static object Deserialize(EndianBinaryReader br, Type type)
    {
      QMetaTypeSerializer serializer = GetMetaTypeSerializer(type);
      return serializer.Deserialize(br, type);
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
    private QVariant(object value, QMetaType type)
    {
      Value = value;
      Type  = type;
    }
    
    public QVariant(QMetaType type)
      : this(null, type)
    {
    }
    
    public QVariant(string value)
      : this(value, QMetaType.QString)
    {
    }
    
    public QVariant(bool value)
      : this(value, QMetaType.Bool)
    {
    }
    
    public QVariant(int value)
      : this(value, QMetaType.Int)
    {
    }
    
    public QVariant(QVariant value)
    {
      Value = value.Value;
      Type  = value.Type;
    }
    
    public object Value { get; protected set; }
    public QMetaType Type { get; protected set; }
  }
  
  
  // TODO: implement a working class of this
  public class QVariantMap : Dictionary<string, QVariant>
  {
  }
  
  // TODO: implement a working class of this
  public class QVariantList : List<QVariant>
  {
    
  }
  
  public interface QMetaTypeSerializer
  {
    QMetaType Type { get; }
    void Serialize(EndianBinaryWriter bw, object data);
    object Deserialize(EndianBinaryReader br, Type type);
  }
  
  public class VoidSerializer : QMetaTypeSerializer
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
  
  public class BoolSerializer : QMetaTypeSerializer
  {
    public QMetaType Type { get { return QMetaType.Bool; } }
    
    public string Name { get { return "Bool"; } }
    
    public void Serialize(EndianBinaryWriter bw, object data)
    {
      bw.Write((bool)data);
    }
    
    public object Deserialize(EndianBinaryReader br, Type type)
    {
      return br.ReadBoolean();
    }
  }
  
  public class QIntegerSerializer : QMetaTypeSerializer
  {
    public QMetaType Type { get { return QMetaType.Int; } }
    
    public string Name { get { return "QInteger"; } }
    
    public void Serialize(EndianBinaryWriter bw, object data)
    {
      bw.Write((int)data);
    }
    
    public object Deserialize(EndianBinaryReader br, Type type)
    {
      return br.ReadInt32();
    }
  }
  
  public class QCharSerializer : QMetaTypeSerializer
  {
    public QMetaType Type { get { return QMetaType.QChar; } }
    
    public string Name { get { return "QChar"; } }
    
    public void Serialize(EndianBinaryWriter bw, object data)
    {
      bw.WriteChar((char)data);
    }
    
    public object Deserialize(EndianBinaryReader br, Type type)
    {
      return (char)br.ReadInt16();
    }
  }

  public class QStringSerializer : QMetaTypeSerializer
  {
    public QMetaType Type { get { return QMetaType.QString; } }
    
    public string Name { get { return "QString"; } }
    
    public void Serialize(EndianBinaryWriter bw, object data)
    {
      if (data == null) {
        bw.Write(-1);
      } else {
        byte[] byteData = Encoding.UTF8.GetBytes(data as string);
        bw.Write(byteData.Length * 2);
        foreach (byte b in byteData) {
          bw.WriteChar((char)b);
        }
        
      }
    }
    
    public object Deserialize(EndianBinaryReader br, Type type)
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
  
  public class QByteArraySerializer : QMetaTypeSerializer
  {
    public QMetaType Type { get { return QMetaType.QByteArray; } }
    
    public string Name { get { return "QByteArray"; } }
    
    public void Serialize(EndianBinaryWriter bw, object obj)
    {
      byte[] data = obj as byte[];
      if (data == null) {
        bw.Write(-1);
      } else {
        bw.Write(data.Length);
        bw.Write(data);
      }
    }
    
    public object Deserialize(EndianBinaryReader br, Type type)
    {
      int len = br.ReadInt32();
      if (len == -1)
       return null; 
      
      return br.ReadBytes(len);
    }
  }  
  
  public class QListSerializer : QMetaTypeSerializer
  {
    public QMetaType Type { get { return QMetaType.None; } }
    
    public void Serialize(EndianBinaryWriter bw, object data)
    {
      int len = (int)data.GetType().GetProperty("Count").GetValue(data, new object[] {});
      
      bw.Write(len);
      
      foreach (object obj in (IEnumerable)data) {
        QTypeManager.Serialize(bw, obj);
      }
    }
    
    public object Deserialize(EndianBinaryReader br, Type type)
    {
      int len = br.ReadInt32();
      
      if (len == -1)
        return null; 
      
      var listDef = type.GetGenericTypeDefinition().MakeGenericType(type.GetGenericArguments());
      var list = listDef.GetConstructor(new Type[] {}).Invoke(new object[] { });
      
      Type listElementType = type.GetGenericArguments()[0];
      var listAddMethod = listDef.GetMethod("Add");
      
      for (int i = 0; i < len; i++) {
        object listElement = QTypeManager.Deserialize(br, listElementType);
        listAddMethod.Invoke(list, new object[] { listElement });
      }
      return list;
    }
  }
  
  public class QMapSerializer : QMetaTypeSerializer
  {
    public QMetaType Type { get { return QMetaType.None; } }
    
    public void Serialize(EndianBinaryWriter bw, object data)
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
    
    public object Deserialize(EndianBinaryReader br, Type type)
    {
      int len = br.ReadInt32();
      
      if (len == -1)
        return null;
      
      var mapType = type.GetGenericTypeDefinition().MakeGenericType(type.GetGenericArguments());
      var map = mapType.GetConstructor(new Type[] {}).Invoke(new object[] { });
      
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
  
  public class QVariantMapSerializer : QMetaTypeSerializer
  {
    public QMetaType Type { get { return QMetaType.None; } }
    
    public void Serialize(EndianBinaryWriter bw, object data)
    {
      Dictionary<string, object> dict = data as Dictionary<string, object>;
      
      bw.Write(dict.Count);
    }
    
    public object Deserialize(EndianBinaryReader br, Type type) 
    {
      return null;
    }
  }
  
  public class QVariantSerializer: QMetaTypeSerializer
  {
    public QMetaType Type { get { return QMetaType.None; } }
    
    public void Serialize(EndianBinaryWriter bw, object data)
    {
      QVariant variant = data as QVariant;
      
      bw.Write((int)variant.Type);
      
      if (variant.Value == null) {
        bw.Write((byte)1);
      } else {
        bw.Write((byte)0);
        QTypeManager.Serialize(bw, variant.Value);
      }
    }
    
    public object Deserialize(EndianBinaryReader br, Type type)
    {
      QMetaType metaType = (QMetaType)br.ReadUInt32();
      int n = br.BaseStream.ReadByte();
      
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
    }
  }
}
