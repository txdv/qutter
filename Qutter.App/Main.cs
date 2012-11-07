using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LibuvSharp;
using LibuvSharp.Terminal;

namespace Qutter.App
{
	public class MainClass
	{
		public static MainWindowTemplate MainWindow { get; private set; }
		public static ThemeManger ThemeManager { get; private set; }

		public static void Main(string[] args)
		{
			string configfile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/.config/quassel-irc.org/quasselclient.conf";
			var settings = new Settings(configfile);

			QuasselTypes.Init();

			var acc = settings.Accounts[settings.AutoConnectAccount];
			var coreConnection = new CoreConnection(acc.HostName, acc.Port, acc.User, acc.Password, false);

			Application.Init();

			var qc = new QuasselClient(coreConnection);

			ThemeManager = new ThemeManger();

			MainWindow = ThemeManager.Default.CreateMainWindow(qc);

			AsyncWatcher<QVariant> listnotifier = new AsyncWatcher<QVariant>((packet) => qc.Handle(packet));
			AsyncWatcher<Exception> excenotifier = new AsyncWatcher<Exception>((exce) => qc.Handle(exce));

			var nt = new Thread((obj) => {
				coreConnection.ReceivePacket += (packet) => {
					listnotifier.Send(packet);
				};

				coreConnection.Exception += (exception) => {
					excenotifier.Send(exception);
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

