using System;
using System.Text;
using MiscUtil.IO;
using Qutter;

namespace Qutter.App
{
	public class IrcMessage
	{
		public enum MessageType : int
		{
			Plain     = 0x00001,
			Notice    = 0x00002,
			Action    = 0x00004,
			Nick      = 0x00008,
			Mode      = 0x00010,
			Join      = 0x00020,
			Part      = 0x00040,
			Quit      = 0x00080,
			Kick      = 0x00100,
			Kill      = 0x00200,
			Server    = 0x00400,
			Info      = 0x00800,
			Error     = 0x01000,
			DayChange = 0x02000,
			Topic     = 0x04000,
			NetsplitJoin = 0x08000,
			NetsplitQuit = 0x10000,
			Invite = 0x20000,
		}

		public enum Flag : byte
		{
			None       = 0x00,
			Self       = 0x01,
			Highlight  = 0x02,
			Redirected = 0x04,
			ServerMsg  = 0x08,
			Backlog    = 0x80
		}

		public int MessageId { get; set; }

		public DateTime DateTime { get; set; }

		public BufferInfo BufferInfo { get; set; }

		public int BufferId {
			get {
				if (BufferInfo == null) {
					return 0;
				}
				return BufferInfo.Id;
			}
		}

		public string Contents { get; set; }
		public string Sender { get; set; }

		public MessageType Type { get; set; }
		public Flag Flags { get; set; }

		public override string ToString ()
		{
			return string.Format ("[IrcMessage: MessageId={0}, BufferInfo={1}, BufferId={2}, Contents={3}, Sender={4}, Type={5}, Flags={6}]", MessageId, BufferInfo, BufferId, Contents, Sender, Type, Flags);
		}
	}

	public class MessageSerializer : QMetaTypeSerializer<IrcMessage>
	{
		public void Serialize(EndianBinaryWriter bw, IrcMessage data)
		{
		}

		public IrcMessage Deserialize(EndianBinaryReader br, Type type)
		{
			IrcMessage ircMessage = new IrcMessage();
			ircMessage.MessageId = br.ReadInt32();

			ircMessage.DateTime = Epoch.FromUnix(br.ReadInt32());

			ircMessage.Type  = (IrcMessage.MessageType)br.ReadInt32();
			ircMessage.Flags = (IrcMessage.Flag)br.ReadByte();

			ircMessage.BufferInfo = new BufferInfo(br);

			byte[] byteBuffer = QTypeManager.Deserialize<byte[]>(br.BaseStream);

			ircMessage.Sender = Encoding.ASCII.GetString(byteBuffer);

			byteBuffer = QTypeManager.Deserialize<byte[]>(br.BaseStream);
			ircMessage.Contents = (byteBuffer == null ? null : Encoding.ASCII.GetString(byteBuffer));
			return ircMessage;
		}
	}
}

