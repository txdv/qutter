using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Qutter.App
{
	public class QuasselClient
	{
		private CoreConnection connection;
		public NetworkCollection NetworkCollection { get; protected set; }
		public BufferSyncer BufferSyncer { get; protected set; }

		internal QuasselClient(CoreConnection connection)
		{
			this.connection = connection;
			NetworkCollection = new NetworkCollection(this);
			BufferSyncer = new BufferSyncer(this);
		}

		internal void Send(QVariant variant)
		{
			connection.Send(variant);
		}

		internal void Send(int bla, string className, string objectName)
		{
			List<QVariant> l = new List<QVariant>();
			l.Add(new QVariant(bla));
			l.Add(new QVariant(className));
			l.Add(new QVariant(objectName));
			Send(new QVariant(l));
		}

		internal void Send(Dictionary<string, QVariant> qmap)
		{
			Send(new QVariant(qmap));
		}

		internal void Send(List<QVariant> list)
		{
			Send(new QVariant(list));
		}

		internal void Init()
		{
			BufferSyncer.Init();
		}

		public void Handle(QVariant packet)
		{
			switch (packet.Type) {
			case QMetaType.QVariantList:
				Handle(packet.Value as List<QVariant>);
				break;
			case QMetaType.QVariantMap:
				Handle (packet.Value as Dictionary<string, QVariant>);
				break;
			}
		}

		public void Handle(Dictionary<string, QVariant> map)
		{
			QVariant type;
			if (map.TryGetValue("MsgType", out type)) {
				switch (type.GetValue<string>()) {
				case "SessionInit":
					HandleSessionInit(map);
					break;
				default:
					break;
				}
			}
		}

		public void HandleSessionInit(Dictionary<string, QVariant> map)
		{
			MainClass.MainWindow.ChatViewManager.Debug(Show(map));
			var state = (map["SessionState"] as QVariant).Value as Dictionary<string, QVariant>;

			var networks = (state["NetworkIds"].Value as List<QVariant>).Select(net => net.GetValue<int>()).ToArray();
			foreach (var netid in networks) {
				NetworkCollection.Get(netid);
			}

			var ircUserCount = state["IrcUserCount"].GetValue<int>();
			var ircChannelCount = state["IrcChannelCount"].GetValue<int>();
			var identities = (state["Identities"].Value as List<QVariant>).Select(v => v.GetValue<Identity>()).ToArray();
			var bufferInfo = (state["BufferInfos"].Value as List<QVariant>).Select(bi => bi.GetValue<BufferInfo>()).ToArray();

			BufferSyncer.Sync(bufferInfo);

			foreach (int netid in networks) {
				SendNetwork(netid);
			}
		}

		void SendNetwork(int netid)
		{
			List<QVariant> list = new List<QVariant>();
			list.Add(new QVariant((int)RequestType.InitRequest));
			list.Add(new QVariant("Network"));
			list.Add(new QVariant(netid.ToString()));
			Send(list);
		}

		void RequestBacklog(int bufferId, int firstMsgId = -1, int lastMsgId = -1, int limit = 100, int additional = 0)
		{
			List<QVariant> packet = new List<QVariant>();
			packet.Add(new QVariant((int)RequestType.Sync));
			packet.Add(new QVariant("BacklogManager"));
			packet.Add(new QVariant(null, QMetaType.QString));
			packet.Add(new QVariant(Encoding.ASCII.GetBytes("requestBacklog")));
			packet.Add(new QVariant(bufferId, "BufferId"));
			packet.Add(new QVariant(firstMsgId, "MsgId"));
			packet.Add(new QVariant(lastMsgId, "MsgId"));
			packet.Add(new QVariant(limit));
			packet.Add(new QVariant(additional));
			Send(packet);
		}

		public void Handle(List<QVariant> list)
		{
			MainClass.MainWindow.ChatViewManager.DebugChatView.Add(new MultiLineStringChatViewEntry(Show(list)));

			if (list[1].GetValue<string>() == "__objectRenamed__") {
				return;
			}

			var type = (RequestType)list[0].GetValue<int>();

			switch (type) {
			case RequestType.Sync:
				switch (list[1].GetValue<string>()) {
				case "Network":
					HandleNetwork(list);
					break;
				case "IrcUser":
					HandleIrcUser(list);
					break;
				case "BufferSyncer":
					HandleBufferSyncer(list);
					break;
				case "IrcChannel":
					HandleIrcChannel(list);
					break;
				}
				break;
			case RequestType.RpcCall:
				HandleRpcCallCommand(list);
				break;
			default:
				Console.Error.WriteLine ("not handled: {0}", Show(list));
				break;
			}
		}

		Network GetNetwork(string str, out string reminder)
		{
			var tmp = str.Split(new char[] { '/' });
			reminder = tmp[1];
			return NetworkCollection.Get(Convert.ToInt32(tmp[0]));
		}

		IrcChannel GetIrcChannel(string str)
		{
			string channelName;
			var network = GetNetwork(str, out channelName);
			return network.IrcChannelCollection.Get(str);
		}

		IrcUser GetIrcUser(string str)
		{
			string nick;
			var network = GetNetwork(str, out nick);
			return network.IrcUserCollection.Get(nick);
		}

		void HandleIrcChannel(List<QVariant> list)
		{

			var channel = GetIrcChannel(list[2].GetValue<string>());
			var function = Encoding.ASCII.GetString(list[3].GetValue<byte[]>());
			string nick, usermode, target = null;
			char mode;
			switch (function) {
			case "addChannelMode":
				mode = list[4].GetValue<char>();
				//target = list[5].GetValue<string>();
				channel.addChannelMode(mode, target);
				break;
			case "removeChannelMode":
				mode = list[4].GetValue<char>();
				//target = list[5].GetValue<string>();
				channel.removeChannelMode(mode, target);
				break;
			case "addUserMode":
				nick = list[4].GetValue<string>();
				usermode = list[5].GetValue<string>();
				channel.addUserMode(nick, usermode);
				break;
			case "removeUserMode":
				nick = list[4].GetValue<string>();
				usermode = list[5].GetValue<string>();
				channel.removeUserMode(nick, usermode);
				break;
			case "joinIrcUsers":
				channel.joinIrcUsers(list[4].GetValue<List<string>>(), list[5].GetValue<List<string>>());
				break;
			default:
				Console.Error.WriteLine(Show(list));
				break;
			}

		}

		void HandleBufferSyncer(List<QVariant> list)
		{
			var function = Encoding.ASCII.GetString(list[3].GetValue<byte[]>());
			switch (function) {
			case "setLastSeenMsg":
				BufferSyncer.setLastSeenMsg(list[4].GetValue<int>(), list[5].GetValue<int>());
				break;
			case "markBufferAsRead":
				BufferSyncer.markBufferAsRead(list[4].GetValue<int>());
				break;
			case "setMarkerLine":
				BufferSyncer.setMarkerLine(list[4].GetValue<int>(), list[5].GetValue<int>());
				break;
			default:
				Console.Error.WriteLine(Show(list));
				break;
			}
		}

		void HandleNetwork(List<QVariant> list)
		{
			int id = Convert.ToInt32(list[2].GetValue<string>());
			var network = NetworkCollection.Get(id);
			string function = Encoding.ASCII.GetString(list[3].GetValue<byte[]>());
			switch (function) {
			case "setLatency":
				network.setLatency(list[4].GetValue<int>());
				break;
			case "addIrcUser":
				network.addIrcUser(list[4].GetValue<string>());
				break;
			case "addIrcChannel":
				network.addIrcChannel(list[4].GetValue<string>());
				break;
			default:
				Console.Error.WriteLine(Show(list));
				break;
			}
		}

		void HandleIrcUser(List<QVariant> list)
		{
			var user = GetIrcUser(list[2].GetValue<string>());

			var func = Encoding.ASCII.GetString(list[3].GetValue<byte[]>());
			switch (func) {
			case "quit":
				user.quit();
				break;
			case "setNick":
				user.setNick(list[3].GetValue<string>());
				break;
			case "setUser":
				user.setNick(list[3].GetValue<string>());
				break;
			case "setHost":
				user.setHost(list[3].GetValue<string>());
				break;
			case "partChannel":
				user.partChannel(list[3].GetValue<string>());
				break;
			case "setRealName":
				user.setRealName(list[3].GetValue<string>());
				break;
			case "setServer":
				user.setServer(list[3].GetValue<string>());
				break;
			case "setIdleTime":
				user.setIdleTime(list[3].GetValue<DateTime>());
				break;
			case "setLoginTime":
				user.setLoginTime(list[3].GetValue<DateTime>());
				break;
			case "setAway":
				user.setAway(list[3].GetValue<bool>());
				break;
			case "setAwaymessage":
				user.setAwayMessage(list[3].GetValue<string>());
				break;
			default:
				Console.Error.WriteLine(Show(list));
				break;
			}
		}

		void HandleRpcCallCommand(List<QVariant> list)
		{
			var obj = list[1].Value;
			if (obj is string) {
				Console.Error.WriteLine ("this shit: {0}", Show(list));
				return;
			}
			switch (Encoding.ASCII.GetString(list[1].GetValue<byte[]>())) {
			case "2displayMsg(Message)":
				BufferSyncer.Display(list[2].GetValue<IrcMessage>());
				break;
			default:
				Console.Error.WriteLine ("this shit: {0}", Show(list));
				break;
			}
		}

		internal static string Show(object o)
		{
			return Show(o, 0);
		}

		internal static string Show(object o, int level)
		{
			if (o == null) {
				return "(null)";
			}

			if (o is Dictionary<string, QVariant>) {
				string ret = "{ ";
				foreach (var k in (Dictionary<string, QVariant>)o) {
					ret += string.Format("(\"{0}\", {1}), ", k.Key, Show(k.Value, level + 1));
				}
				return ret + " }";
			} else if (o is QVariant) {
				QVariant var = o as QVariant;

				return string.Format("QVariant({0}, {1})", var.Type, Show(var.Value, level + 1));
				/*
				if (var.Value == null) {
					return string.Format("{0}(null)", var.GetType().ToString());
				} else {
					return Show((o as QVariant).Value, level);
				}
				*/
			} else if (o is List<QVariant>) {
				string ret = "[ ";
				bool first = true;
				var list = o as List<QVariant>;
				for (int i = 0; i < list.Count; i++) {
					QVariant le = list[i];
					if (first && le.Value is int) {
						first = false;
						ret += string.Format("{0}", (RequestType)le.Value);
					} else {
						first = false;
						ret += string.Format("{0}", Show(le, level + 1));
					}
					bool last = list.Count == i + 1;
					if (!last) {
						ret += ", ";
					}
				}
				return ret + " ]";
			} else if (o is string) {
				return string.Format("\"{0}\"", o);
			} else if (o is byte[]) {
				return string.Format("byte[] \"{0}\"", Encoding.ASCII.GetString(o as byte[]));
			}
			return o.ToString();
		}
/*
		public static void List(QVariant v)
		{
			if (v.Value is Dictionary<string, QVariant>) {
				foreach (var kvp in v.GetValue<Dictionary<string, QVariant>>()) {
					Console.WriteLine("{0}({1}) = {2}", kvp.Key, kvp.Value.Type, kvp.Value.Value);
				}
			} else if (v.Value is List<QVariant>) {
				foreach (QVariant d in v.GetValue<List<QVariant>>()) {
					Console.WriteLine("{0}", d.Value);
				}
			}
		}
		*/
	}
}

