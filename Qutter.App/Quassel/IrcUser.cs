using System;
using System.Collections.Generic;

namespace Qutter.App
{
	public class IrcUser
	{
		public Network Network { get; protected set; }

		public string Nick { get; protected set; }
		public string User { get; protected set; }
		public string Host { get; protected set; }
		public string RealName { get; protected set; }
		public string Server { get; protected set; }
		public DateTime IdleTime { get; protected set; }
		public DateTime LoginTime { get; protected set; }
		public bool Away { get; protected set; }
		public string AwayMessage { get; protected set; }

		public string UserString {
			get {
				return string.Format("{0}!{1}@{2}", Nick, User, Host);
			}
		}

		public IrcUser(Network network, string userString)
		{
			if (userString.Contains("!") && userString.Contains("@")) {
				var tmp = userString.Split(new char[] { '!' });
				Nick = tmp[0];
				var tmp2 = tmp[1].Split(new char[] { '@' });
				User = tmp2[0];
				Host = tmp2[1];
			} else {
				Nick = userString;
			}
		}

		internal void setNick(string nick)
		{
			Nick = nick;
		}

		internal void setUser(string user)
		{
			User = user;
		}

		internal void setHost(string host)
		{
			Host = host;
		}

		internal void partChannel(string channel)
		{
			// TODO: do something
		}

		internal void quit()
		{
			// TODO: do something
		}

		internal void setRealName(string realName)
		{
			RealName = realName;
		}

		internal void setServer(string server)
		{
			Server = server;
		}

		internal void setIdleTime(DateTime idleTime)
		{
			IdleTime = idleTime;
		}

		internal void setLoginTime(DateTime loginTime)
		{
			LoginTime = loginTime;
		}

		internal void setAway(bool away)
		{
			Away = away;
		}

		internal void setAwayMessage(string awayMessage)
		{
			AwayMessage = awayMessage;
		}
	}

	public class IrcUserCollection
	{
		public Network Network { get; protected set; }
		Dictionary<string, IrcUser> users = new Dictionary<string, IrcUser>();

		public IrcUserCollection(Network network)
		{
			Network = Network;
		}

		public IrcUser Get(string nick)
		{
			IrcUser user;
			if (!users.TryGetValue(nick, out user)) {
				user = new IrcUser(Network, nick);
				users[nick] = user;
			}
			return user;
		}
	}
}

