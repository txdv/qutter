using System;
using Nini.Config;
using System.IO;

namespace Qutter.App
{
	public class CoreAccount
	{
		public int Index { get; protected set; }
		public string Name { get; protected set; }
		public string HostName { get; protected set; }
		public string Password { get; protected set; }
		public int Port { get; protected set; }
		public string User { get; protected set; }

		public CoreAccount(int i, IConfig config)
		{
			Index = i;

			Name = config.GetString(_("AccountName"));
			HostName = config.GetString(_("HostName"));
			Password = config.GetString(_("Password"));
			Port = config.GetInt(_("Port"));
			User = config.GetString(_("User"));
		}

		string _(string str)
		{
			return string.Format("{0}\\{1}", Index, str);
		}
	}

	public class Settings
	{
		public CoreAccount[] Accounts { get; protected set; }
		public int AutoConnectAccount { get; protected set; }

		public Settings(string file)
		{
			var source = new IniConfigSource(file);

			var config = source.Configs["Config"];

			config = source.Configs["CoreAccounts"];

			AutoConnectAccount = config.GetInt("AutoConnectAccount") - 1;
			int count = config.GetInt("LastAccount");

			Accounts = new CoreAccount[count];
			for (int i = 1; i <= count; i++) {
				Accounts[i - 1] = new CoreAccount(i, config);
			}
		}
	}
}

