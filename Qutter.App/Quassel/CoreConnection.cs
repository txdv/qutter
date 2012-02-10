using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Qutter;

namespace Qutter.App
{
	public class CoreConnection
	{
		TcpClient tcp;
		NetworkStream ns;

		public QuasselClient QuasselClient { get; protected set; }

		public CoreConnection(string address, int port, string username, string password, bool ssl)
		{
			Address = address;
			Port = port;
			SSL = ssl;
			User = username;
			Password = password;
		}

		public string Address  { get; protected set; }
		public int    Port     { get; protected set; }
		public bool   SSL      { get; protected set; }
		public string User     { get; protected set; }
		public string Password { get; protected set; }

		public QVariant GetConnectionPacket()
		{
			Dictionary<string, QVariant> packet = new Dictionary<string, QVariant>();
			packet["ClientDate"]      = new QVariant(DateTime.Now.ToString("MMM dd yyyy HH:mm:ss"));
			packet["UseSsl"]          = new QVariant(SSL);
			packet["ClientVersion"]   = new QVariant(@"v0.6.1 (dist-<a href='http://git.quassel-irc.org/?p=quassel.git;a=commit;h=611ebccdb6a2a4a89cf1f565bee7e72bcad13ffb'>611ebcc</a>)");
			packet["UseCompression"]  = new QVariant(false);
			packet["MsgType"]         = new QVariant("ClientInit");
			packet["ProtocolVersion"] = new QVariant(10);
			return new QVariant(packet);
		}

		public QVariant GetClientLoginPacket()
		{
			Dictionary<string, QVariant> packet = new Dictionary<string, QVariant>();
			packet["MsgType"] =  new QVariant("ClientLogin");
			packet["User"]    =  new QVariant(User);
			packet["Password"] = new QVariant(Password);
			return new QVariant(packet);
		}

		public void Connect()
		{
			tcp = new TcpClient();
			tcp.Connect(Address, Port);
			ns = tcp.GetStream();

			Send(GetConnectionPacket());

			Receive();

			Send(GetClientLoginPacket());

		}

		public void Loop()
		{
			while (true) {
				try {
					OnReceivePacket(Receive());
				} catch (Exception e) {
					OnException(e);
				}
			}
		}

		protected void OnReceivePacket(QVariant packet)
		{
			if (ReceivePacket != null) {
				ReceivePacket(packet);
			}
		}

		public event Action<QVariant> ReceivePacket;

		protected void OnException(Exception exception)
		{
			if (Exception != null) {
				Exception(exception);
			}
		}

		public event Action<Exception> Exception;

		public QVariant Receive()
		{
			MiscUtil.IO.EndianBinaryReader br = new MiscUtil.IO.EndianBinaryReader(MiscUtil.Conversion.EndianBitConverter.Big, ns);
			int len = br.ReadInt32();
			MemoryStream ms = new MemoryStream(br.ReadBytes(len));
			ms.Seek(0, SeekOrigin.Begin);
			return QTypeManager.Deserialize<QVariant>(ms);
		}

		public void Send(QVariant qvariant)
		{
			MiscUtil.IO.EndianBinaryWriter bw = new MiscUtil.IO.EndianBinaryWriter(MiscUtil.Conversion.EndianBitConverter.Big, ns);
			MemoryStream ms = new MemoryStream();
			QTypeManager.Serialize(ms, qvariant);
			ms.Seek(0, SeekOrigin.Begin);
			bw.Write((int)ms.Length);
			QTypeManager.Serialize(ns, qvariant);
			ns.Flush();
		}

		/*
		public void RequestBacklog(int buffer, int firstMsgId, int lastMsgId, int maxAmount) {
			List<QVariant> packet = new List<QVariant>();
			packet.Add(new QVariant((int)RequestType.Sync));
			packet.Add(new QVariant("BacklogManager"));
			packet.Add(new QVariant(""));
			packet.Add(new QVariant("requestBacklog"));
			packet.Add(new QVariant(buffer));
			packet.Add(new QVariant(firstMsgId));
			packet.Add(new QVariant(lastMsgId));
			packet.Add(new QVariant(maxAmount));
			packet.Add(new QVariant(0));
			Send(packet);
		}
		*/

		List<QVariant> PacketList(RequestType type, params QVariant[] obj)
		{
			List<QVariant> packet = new List<QVariant>();
			packet.Add(new QVariant((int)type));
			foreach (var o in obj) {
				packet.Add(o);
			}
			return packet;
		}
	}
}

