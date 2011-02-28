using System;
using System.IO;

namespace Qutter.BinaryExtensions
{ 
  public static class BinaryReaderExtensions
  {
    #region Big Endian

    public static int BReadInt32(this BinaryReader br)
    {
      byte b1 = br.ReadByte();
      byte b2 = br.ReadByte();
      byte b3 = br.ReadByte();
      byte b4 = br.ReadByte();

      return ((b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
    }

    public static uint BReadUInt32(this BinaryReader br)
    {
      byte b1 = br.ReadByte();
      byte b2 = br.ReadByte();
      byte b3 = br.ReadByte();
      byte b4 = br.ReadByte();

      return (uint)((b4 << 24) | (b3 << 16) | (b2 << 8) | b1);
    }

    public static short BReadInt16(this BinaryReader br)
    {
      byte b1 = br.ReadByte();
      byte b2 = br.ReadByte();
      return (short)((b2 << 8) | b1);
    }

    public static ushort BReadUInt16(this BinaryReader br)
    {
      byte b1 = br.ReadByte();
      byte b2 = br.ReadByte();
      return (ushort)((b2 << 8) | b1);
    }

    #endregion

    #region Little Endian

    public static int LReadInt32(this BinaryReader br)
    {
      byte b1 = br.ReadByte();
      byte b2 = br.ReadByte();
      byte b3 = br.ReadByte();
      byte b4 = br.ReadByte();

      return ((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
    }

    public static uint LReadUInt32(this BinaryReader br)
    {
      byte b1 = br.ReadByte();
      byte b2 = br.ReadByte();
      byte b3 = br.ReadByte();
      byte b4 = br.ReadByte();

      return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
    }

    public static short LReadInt16(this BinaryReader br)
    {
      byte b1 = br.ReadByte();
      byte b2 = br.ReadByte();
      return (short)((b1 << 8) | b2);
    }

    public static ushort LReadUInt16(this BinaryReader br)
    {
      byte b1 = br.ReadByte();
      byte b2 = br.ReadByte();
      return (ushort)((b1 << 8) | b2);
    }
    
    public static char LReadChar(this BinaryReader br)
    {
      return (char)br.LReadUInt16();
    }
    
    public static byte[] LReadBytes(this BinaryReader br, int size)
    {
      byte[] bytes = new byte[size];
      for (int i = 0; i < size; i++) {
        bytes[i] = br.ReadByte();
      }
      return bytes;
      
    }
    #endregion
  }
  
  public static class BinaryWriterExtensions
  {
    public static void LWrite(this BinaryWriter bw, int value)
    {
      bw.Write((byte)(value >> 24 & 0xFF));
      bw.Write((byte)(value >> 16 & 0xFF));
      bw.Write((byte)(value >>  8 & 0xFF));
      bw.Write((byte)(value       & 0xFF));
    }
    
    public static void LWrite(this BinaryWriter bw, char value)
    {
      bw.Write((byte)(value >>  8 & 0xFF));
      bw.Write((byte)(value       & 0xFF));
    }
    
    public static void LWrite(this BinaryWriter bw, byte[] byteArray)
    {
      foreach (byte b in byteArray) {
        bw.LWrite((char)b);
      }
    }
  }
}