using System;
using System.Threading;
using Manos.IO;
using Mono.Terminal;

namespace Qutter.App
{
	public class AsyncCoreConnection
	{
		AsyncWatcher<QVariant> listnotifier;
		AsyncWatcher<Exception> exceptionnotifier;
		Thread thread;

		public Context Context { get; protected set; }
		public CoreConnection CoreConnection { get; protected set; }

		public AsyncCoreConnection(Context context, CoreConnection coreConnection)
		{
			Context = context;
			CoreConnection = coreConnection;

			listnotifier = new AsyncWatcher<QVariant>(Context, (packet) => OnReceivePacket(packet));
			exceptionnotifier = new AsyncWatcher<Exception>(Context, (exception) => OnException(exception));

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
			listnotifier.Start();
			exceptionnotifier.Start();

		}

		public void Stop()
		{
			thread.Abort();
			listnotifier.Stop();
			exceptionnotifier.Stop();
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
