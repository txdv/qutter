using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Mono.Terminal;
using Manos.IO;

namespace Qutter.App
{
	public class MainClass
	{
		public static MainWindow MainWindow { get; set; }

		public static void Main(string[] args)
		{
			var settings = new Settings();

			QuasselTypes.Init();

			var acc = settings.Accounts[settings.AutoConnectAccount];
			var coreConnection = new CoreConnection(acc.HostName, acc.Port, acc.User, acc.Password, false);

			Application.Init(Context.Create(Backend.Poll));

			var qc = new QuasselClient(coreConnection);

			MainWindow = new MainWindow(qc);

			AsyncWatcher<QVariant> listnotifier = new AsyncWatcher<QVariant>(Application.Context, (packet) => {
				qc.Handle(packet);
			});

			listnotifier.Start();

			var nt = new Thread((obj) => {
				coreConnection.ReceivePacket += (packet) => {
					listnotifier.Send(packet);
				};

				coreConnection.Exception += (exception) => {
				};

				coreConnection.Connect();

				coreConnection.Loop();
			});

			nt.Start();

			Application.End += (() => {
				nt.Abort();
			});

			Application.Run(MainWindow);
		}
	}
}

