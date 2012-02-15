using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO.Compression;

using Qutter.App;

namespace Qutter.Inspector
{
	class StreamInspector
	{
		public int BufferSize { get; protected set; }

		public TcpClient Source { get; protected set; }
		public TcpClient Destination { get; protected set; }

		Thread SourceThread { get; set; }
		Thread DestinationThread { get; set; }

		public StreamInspector(TcpClient source, TcpClient destination)
			: this(1000000, source, destination)
		{
		}

		public StreamInspector(int bufferSize, TcpClient source, TcpClient destination)
		{
			BufferSize = bufferSize;

			Source = source;
			Destination = destination;
			var sourceStream = source.GetStream();
			var destinationStream = destination.GetStream();

			Action<Stream, Stream> move = (src, dst) => {
				MiscUtil.IO.EndianBinaryReader br = new MiscUtil.IO.EndianBinaryReader(MiscUtil.Conversion.EndianBitConverter.Big, src);
				MiscUtil.IO.EndianBinaryWriter bw = new MiscUtil.IO.EndianBinaryWriter(MiscUtil.Conversion.EndianBitConverter.Big, dst);
				while (true) {
					int len = br.ReadInt32();
					var packet = br.ReadBytes(len);
					OnSend(packet);
					bw.Write(len);
					bw.Write(packet);
				}
			};

			SourceThread = new Thread((o) => move(sourceStream, destinationStream));
			SourceThread.Start();
			DestinationThread = new Thread((o) => move(destinationStream, sourceStream));
			DestinationThread.Start();
		}

		protected void OnSend(byte[] buffer)
		{
			if (Send != null) {
				Send(buffer);
			}
		}

		protected void OnReceive(byte[] buffer)
		{
			if (Receive != null) {
				Receive(buffer);
			}
		}

		public event Action<byte[]> Send;
		public event Action<byte[]> Receive;

		public static string format(byte b)
		{
			var str = b.ToString("X");
			str = str.ToUpper();
			if (str.Length == 1) {
				return "0" + str;
			}
			return str;
		}

		public static string inspect(byte[] buffer) {
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < buffer.Length; i++) {
				sb.Append(format(buffer[i]));
				if (buffer.Length != i+1) {
					sb.Append(", ");
				}
			}
			return string.Format("byte[ {0} ] {1}", buffer.Length, sb.ToString());
		}

		public static void Main(string[] args)
		{
			QuasselTypes.Init();
			Run(args);
		}

		public static void Run(string[] args)
		{
			TcpListener listener = new TcpListener(IPAddress.Any, 7000);
			listener.Start(128);
			var source = listener.AcceptTcpClient();

			TcpClient destination = new TcpClient();
			destination.Connect("127.0.0.1", 4242);

			var si = new StreamInspector(source, destination);

			si.Receive += (buffer) => Handle("<-", buffer);
			si.Send += (buffer) => Handle("->", buffer);
		}

		public static void Handle(string prefix, byte[] buffer)
		{
			MemoryStream ms = new MemoryStream(buffer);
			ms.Seek(0, SeekOrigin.Begin);
			try {
				string ret = QVariant.Inspect(QTypeManager.Deserialize<QVariant>(ms));
				Console.WriteLine("{0} {1}", prefix, QVariant.Inspect(ret));
			} catch (Exception exception) {
				Console.WriteLine ("{0} failed ({1})", prefix, buffer.Length);
				Console.WriteLine (inspect(buffer));
				Console.WriteLine (exception);
				return;
			}
		}
	}
}
