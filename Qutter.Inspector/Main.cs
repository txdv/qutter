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

			SourceThread = new Thread((o) => {
				// Use this for unspecfici threads
				/*
				byte[] buffer = new byte[BufferSize];
				int size;
				while ((size = sourceStream.Read(buffer, 0, 512)) > 0) {
					OnSend(buffer, size);
					destinationStream.Write(buffer, 0, size);
				}
				*/

				MiscUtil.IO.EndianBinaryReader br = new MiscUtil.IO.EndianBinaryReader(MiscUtil.Conversion.EndianBitConverter.Big, sourceStream);
				MiscUtil.IO.EndianBinaryWriter bw = new MiscUtil.IO.EndianBinaryWriter(MiscUtil.Conversion.EndianBitConverter.Big, destinationStream);
				while (true) {
					int len = br.ReadInt32();
					var packet = br.ReadBytes(len);
					OnSend(packet);
					bw.Write(len);
					bw.Write(packet);
				}
			});

			DestinationThread = new Thread((o) => {
				MiscUtil.IO.EndianBinaryReader br = new MiscUtil.IO.EndianBinaryReader(MiscUtil.Conversion.EndianBitConverter.Big, destinationStream);
				MiscUtil.IO.EndianBinaryWriter bw = new MiscUtil.IO.EndianBinaryWriter(MiscUtil.Conversion.EndianBitConverter.Big, sourceStream);
				//BinaryReader br = new BinaryReader(destinationStream, Encoding.BigEndianUnicode);
				//BinaryWriter bw = new BinaryWriter(sourceStream, Encoding.BigEndianUnicode);
				//BinaryReader brr = new BinaryReader(destinationStream);
				while (true) {
					int len = br.ReadInt32();
					//var packet = br.ReadBytes(len);
					var packet = br.ReadBytes(len);
					OnReceive(packet);
					bw.Write(len);
					bw.Write(packet);
				}
			});
			SourceThread.Start();
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
			Run (args);
			return;
			while (true) {
				try {
					Run(args);
				} catch {
				}
			}
		}

		/*
			QVariant qvar = new QVariant(new BufferInfo(42, 69, BufferInfo.BufferType.Status, "1234567890"));

			MemoryStream m = new MemoryStream();
			QTypeManager.Serialize(m, qvar);
			m.Flush();
			m.Seek(0, SeekOrigin.Begin);
			Console.WriteLine(inspect(m.ToArray()));

			QVariant qvar2;


			QTypeManager.Deserialize(m, out qvar2);
			Console.WriteLine (MessageParser.Show(qvar2));
		 */
		public static void Run(string[] args)
		{
			TcpListener listener = new TcpListener(IPAddress.Any, 7000);
			listener.Start(128);
			var source = listener.AcceptTcpClient();

			TcpClient destination = new TcpClient();
			destination.Connect("127.0.0.1", 4242);

			var si = new StreamInspector(source,destination);

			si.Receive += (buffer) => {
				/*
				MemoryStream ms = new MemoryStream(buffer);
				ms.Seek(0, SeekOrigin.Begin);
				object o;
				try {
					//Console.WriteLine (buffer.Length);
					string ret = MessageParser.Show(QTypeManager.Deserialize<QVariant>(ms));
					Console.WriteLine("-> {0}", ret);
				} catch (Exception exception) {
					Console.WriteLine ("-> failed ({0})", buffer.Length);
					return;
				}*/
			};

			si.Receive += (buffer) => {
			//si.Send += (buffer) => {
				MemoryStream ms = new MemoryStream(buffer);
				ms.Seek(0, SeekOrigin.Begin);
				try {
					var o = QTypeManager.Deserialize<QVariant>(ms);
					//string ret = MessageParser.Show(o);
					//Console.WriteLine("-> {0}", ret);
				} catch (Exception exception) {
					Console.WriteLine ("-> failed ({0})", buffer.Length);
					Console.WriteLine (inspect(buffer));
					Console.WriteLine (exception);
					return;
				}
			};
		}
	}
}