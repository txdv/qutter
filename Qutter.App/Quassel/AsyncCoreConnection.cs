using System;
using System.Threading;
using LibuvSharp;
using LibuvSharp.Terminal;

namespace Qutter.App
{
	public class AsyncCoreConnection
	{
		AsyncWatcher<QVariant> listnotifier;
		AsyncWatcher<Exception> exceptionnotifier;
		Thread thread;

		public Loop Loop { get; protected set; }
		public CoreConnection CoreConnection { get; protected set; }

		public AsyncCoreConnection(Loop loop, CoreConnection coreConnection)
		{
			Loop = loop;
			CoreConnection = coreConnection;

			listnotifier = new AsyncWatcher<QVariant>((packet) => OnReceivePacket(packet));
			exceptionnotifier = new AsyncWatcher<Exception>((exception) => OnException(exception));

			thread = new Thread((obj) => {
				coreConnection.ReceivePacket += (packet) => {
					listnotifier.Send(packet);
				};

				coreConnection.Exception += (exception) => {
					//exceptionnotifier.Send(exception);
				};

				coreConnection.Connect();

				coreConnection.Loop();
			});
		}

		public void Start()
		{
			thread.Start();

		}

		public void Stop()
		{
			thread.Abort();
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

		public event Action<Exception> Exception;	}
}
