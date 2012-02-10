using System;
using System.Collections.Generic;

namespace Qutter.App
{
	public class Network
	{
		public int Id { get; protected set; }
		public int Latency { get; protected set; }

		public NetworkCollection Collection { get; protected set; }
		public IrcUserCollection IrcUserCollection { get; protected set; }
		public IrcChannelCollection IrcChannelCollection { get; protected set; }

		public Network(NetworkCollection networkCollection, int id)
		{
			Collection = networkCollection;
			Id = id;
			IrcUserCollection = new IrcUserCollection(this);
			IrcChannelCollection = new IrcChannelCollection(this);
		}

		public void Init()
		{
			List<QVariant> list = new List<QVariant>() {
				new QVariant((int)RequestType.InitData),
				new QVariant("Network"),
				new QVariant(Id.ToString())
			};
			try {
				Collection.Client.Send(list);
			} catch {
			}
		}

		internal void setLatency(int latency)
		{
			Latency = latency;
			if (LatencyChanged != null) {
				LatencyChanged(latency);
			}
		}

		public event Action<int> LatencyChanged;

		internal void addIrcUser(string userString)
		{
			IrcUserCollection.Get(userString);
		}

		internal void addIrcChannel(string channel)
		{
			IrcChannelCollection.Get(channel);
		}
	}

	public class NetworkCollection
	{
		Dictionary<int, Network> networks = new Dictionary<int, Network>();

		public QuasselClient Client { get; protected set; }

		public NetworkCollection(QuasselClient client)
		{
			Client = client;
		}

		public Network Get(int id)
		{
			Network network;
			if (!networks.TryGetValue(id, out network)) {
				network = new Network(this, id);
				networks[id] = network;
			}
			return network;
		}
	}
}

