using System;

namespace Qutter.App
{
	public class Mono : ITheme
	{
		public string Name { get { return "qutter default"; } }
		public string Author { get { return "Andrius Bentkus"; } }

		public MainWindowTemplate CreateMainWindow(QuasselClient client)
		{
			return new MainWindowTemplate(client);
		}
	}
}

