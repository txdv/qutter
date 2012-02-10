using System;
using System.Text;
using System.Collections.Generic;

namespace Qutter.App
{
	public class IrcChannelUserMode
	{
		internal IrcChannelUserMode()
		{
		}

		SortedSet<char> mode = new SortedSet<char>();

		internal void addUserMode(string smode)
		{
			foreach (var c in smode) {
				mode.Add(c);
			}
		}

		internal void removeUserMode(string smode)
		{
			foreach (var c in mode) {
				mode.Remove(c);
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(mode.Count);
			foreach (var c in mode) {
				sb.Append(c);
			}
			return sb.ToString();
		}
	}

	public class IrcChannelUser
	{
		public IrcChannel Channel { get; protected set; }
		public IrcUser User { get; protected set; }
		public IrcChannelUserMode Mode { get; protected set; }

		internal IrcChannelUser(IrcChannel channel, IrcUser user)
		{
			Channel = channel;
			User = user;
			Mode = new IrcChannelUserMode();
		}
	}

	public class IrcChannelUserCollection
	{
		public IrcChannel Channel { get; protected set; }

		internal IrcChannelUserCollection(IrcChannel channel)
		{
			Channel = channel;
		}

		internal void add(IrcUser user)
		{
		}

		internal void get(string nick)
		{
		}
	}

	public class IrcChannel
	{
		// TODO: expose this somehow
		SortedSet<char> channelModes = new SortedSet<char>();

		public Network Network { get; protected set; }
		public string Name { get; protected set; }
		public string Topic { get; protected set; }
		public IrcChannelUserCollection Users { get; protected set; }

		public IrcChannel(Network network, string channelName)
		{
			Network = network;
			// TODO: cut out the prefix/
			Name = channelName;
			Users = new IrcChannelUserCollection(this);
		}

		internal void setTopic(string topic)
		{
			Topic = topic;
		}

		internal void addChannelMode(char mode, string target)
		{
			if (target == null) {
				channelModes.Add(mode);
			} else {

			}
		}

		internal void removeChannelMode(char mode, string target)
		{
			if (target == null) {
				channelModes.Remove(mode);
			} else {

			}
		}

		internal void addUserMode(string nick, string mode)
		{
		}

		internal void removeUserMode(string nick, string mode)
		{
		}

		internal void joinIrcUsers(List<string> nicklist, List<string> tmp)
		{
			foreach (var nick in nicklist) {
				joinIrcUser(nick);
			}
		}

		void joinIrcUser(string nick)
		{
			//var user = Network.IrcUserCollection.Get(nick);
			// TODO: user list for channel
		}
	}

	public class IrcChannelCollection
	{
		Dictionary<string, IrcChannel> channels = new Dictionary<string, IrcChannel>();

		public Network Network { get; protected set; }

		public IrcChannelCollection(Network network)
		{
			Network = network;
		}

		public IrcChannel Get(string channelName)
		{
			IrcChannel channel;
			if (!channels.TryGetValue(channelName, out channel)) {
				channel = new IrcChannel(Network, channelName);
				channels[channelName] = channel;
			}
			return channel;
		}
	}
}

