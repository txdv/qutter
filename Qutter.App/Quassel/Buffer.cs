using System;
using System.Collections.Generic;
using System.Text;
using MiscUtil.IO;

namespace Qutter.App
{
	public class BufferInfo
	{
		public enum BufferType : int {
			Invalid = 0,
			Status = 1,
			Channel = 2,
			Query = 4,
			Group = 8
		}
		public int Id { get; protected set; }
		public bool IsValid { get { return Id != 0; } }
		public int NetworkId { get; protected set; }
		public BufferInfo.BufferType Type { get; protected set; }
		public int GroupId { get; protected set; }
		public string Name { get; protected set; }

		public BufferInfo(int id, int nid, BufferInfo.BufferType type, string name)
		{
			Id = id;
			NetworkId = nid;
			Type = type;
			Name = name;
		}

		internal BufferInfo(EndianBinaryReader br)
		{
			Id = br.ReadInt32();
			NetworkId = br.ReadInt32();
			Type = (BufferType)br.ReadInt16();
			GroupId = br.ReadInt32();

			Name = Encoding.ASCII.GetString(QTypeManager.Deserialize<byte[]>(br.BaseStream));
		}

		public override string ToString ()
		{
			return string.Format ("[BufferInfo: Id={0}, Valid={1}, NetworkId={2}, Type={3}, GroupId={4}, Name={5}]", Id, IsValid, NetworkId, Type, GroupId, Name);
		}
	}

	public class BufferInfoSerializer : QMetaTypeSerializer<BufferInfo>
	{
		public void Serialize(MiscUtil.IO.EndianBinaryWriter bw, BufferInfo data)
		{
			bw.Write(data.Id);
			bw.Write(data.NetworkId);
			bw.Write((System.Int16)data.Type);
			bw.Write(data.GroupId);

			byte[] arr = new byte[data.Name.Length + 1];
			Encoding.ASCII.GetBytes(data.Name).CopyTo(arr, 0);
			QTypeManager.Serialize(bw, arr);
		}

		public BufferInfo Deserialize(MiscUtil.IO.EndianBinaryReader br, Type type)
		{
			return new BufferInfo(br);
		}
	}

	public class Buffer
	{
		public BufferSyncer Syncer { get; protected set; }

		public Buffer(BufferSyncer syncer, BufferInfo bufferInfo)
		{
			Syncer = syncer;
			BufferInfo = bufferInfo;
			Size = 0;
		}

		LinkedList<IrcMessage> linkedList = new LinkedList<IrcMessage>();

		public int Size { get; protected set; }

		public BufferInfo BufferInfo { get; protected set; }

		public int Id {
			get {
				return BufferInfo.Id;
			}
		}

		public bool Read { get; protected set; }
		public int LastSeenMessageId { get; protected set; }

		internal void markBufferAsRead()
		{
			Read = true;
		}

		internal void setLastSeenMsg(int msgId)
		{
			LastSeenMessageId = msgId;
		}

		internal void Display(IrcMessage ircMessage)
		{
			linkedList.AddLast(ircMessage);
			Size++;
		}

		internal void sendInput(string text)
		{
			List<QVariant> list = new List<QVariant>(new QVariant[] {
				new QVariant((int)RequestType.RpcCall),
				new QVariant(Encoding.ASCII.GetBytes("2sendInput(BufferInfo,QString)")),
				new QVariant(BufferInfo, "BufferInfo"),
				new QVariant(text)
			});
			Syncer.Client.Send(new QVariant(list));
		}

		public void Send(string text)
		{
			sendInput(text);
		}
// -> QVariant(QVariantList, [ RpcCall, QVariant(QByteArray, byte[] "2sendInput(BufferInfo,QString)"), QVariant(UserType, [BufferInfo: Id=3, Valid=True, NetworkId=1, Type=Channel, GroupId=0, Name=#bletnx]), QVariant(QString, "/SAY testing 1 3") ])
// -> QVariant(QVariantList, [ Sync, QVariant(QString, "BufferSyncer"), QVariant(QString, (null)), QVariant(QByteArray, byte[] "requestSetLastSeenMsg"), QVariant(UserType, 3), QVariant(UserType, 35936) ])

	}

	public class BufferSyncer : ActiveList<int, Buffer>
	{
		public QuasselClient Client { get; protected set; }

		public BufferSyncer(QuasselClient client)
		{
			Client = client;


			CurrentChanged += (index) => {
				ActiveChanged(Active);
			};
		}

		internal void Sync(BufferInfo[] bufferInfoList)
		{
			Buffer[] bufferList = new Buffer[bufferInfoList.Length];
			for (int i = 0; i < bufferInfoList.Length; i++) {
				var buffer = new Buffer(this, bufferInfoList[i]);
				bufferList[i] = buffer;
				Add(buffer.Id, buffer);
			}

			OnSynced(bufferList);
		}

		internal void Display(IrcMessage ircMessage)
		{
			OnMessage(Get(ircMessage.BufferId), ircMessage);
		}

		protected void OnMessage(Buffer buffer, IrcMessage ircMessage)
		{
			buffer.Display(ircMessage);
			if (Message != null) {
				Message(buffer, ircMessage);
			}
		}

		public event Action<Buffer, IrcMessage> Message;

		Buffer Get(int id)
		{
			if (Count == 0) {
				return null;
			} else {
				return GetKeyValuePair(IndexOfKey(id)).Value;
			}
		}

		internal void Init()
		{
			List<QVariant> packet = new List<QVariant>() {
				new QVariant((int)RequestType.InitData),
				new QVariant("BufferSyncer"),
				new QVariant(typeof(string))
			};
			Client.Send(new QVariant(packet));
		}

		internal void markBufferAsRead(int id)
		{
			Get(id).markBufferAsRead();
		}

		internal void setLastSeenMsg(int bufferId, int msgId)
		{
			Get(bufferId).setLastSeenMsg(msgId);
		}

		internal void setMarkerLine(int bufferId, int msgId)
		{
		}

		public bool IsSynced { get; private set; }

		protected void OnSynced(Buffer[] syncedBuffers)
		{
			IsSynced = true;
			if (Synced != null) {
				Synced(syncedBuffers);
			}
		}
		public event Action<Buffer[]> Synced;

		public void Prev()
		{
			Current--;
		}

		public void Next()
		{
			Current++;
		}

		public Buffer Active {
			get {
				if (Count == 0) {
					return null;
				}
				return GetKeyValuePair(Current).Value;
			}
		}

		internal void OnActiveChanged()
		{
			if (ActiveChanged != null) {
				ActiveChanged(Active);
			}
		}

		public event Action<Buffer> ActiveChanged;
	}
}

