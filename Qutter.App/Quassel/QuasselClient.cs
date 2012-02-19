using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Qutter.App
{
	class RequestCommand
	{
		public string ClassName { get; set; }
		public string ObjectName { get; set; }
		public Action<QVariant[]> Callback { get; set; }
	}

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

		internal void Send(Dictionary<string, QVariant> qmap)
		{
			Send(new QVariant(qmap));
		}

		internal void Send(List<QVariant> list)
		{
			Send(new QVariant(list));
		}

		internal void Send(params QVariant[] vars)
		{
			List<QVariant> packet = new List<QVariant>();
			packet.AddRange(vars);
			Send(packet);
		}

		internal void Send(int type, string className, string objectName)
		{
			Send(new QVariant[] {
				new QVariant(type),
				(className == null ? new QVariant(null, QMetaType.QString) : new QVariant(className)),
				(objectName == null ? new QVariant(null, QMetaType.QString) : new QVariant(objectName))
			});
		}

		internal void Send(RequestType type, string className, string objectName)
		{
			Send((int)type, className, objectName);
		}

		internal void Send(string className, string objectName)
		{
			Send(RequestType.InitRequest, className, objectName);
		}

		List<RequestCommand> commands = new List<RequestCommand>();
		internal void Send(string className, string objectName, Action<QVariant[]> callback)
		{
			var req = new RequestCommand() {
				ClassName = className,
				ObjectName = objectName,
				Callback = callback
			};

			commands.Add(req);
			Send(className, objectName);
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

		void HandleSessionInit(Dictionary<string, QVariant> map)
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
				Send(RequestType.InitRequest, "Network", netid.ToString());
			}
		}

		int errors = 0;
		bool first = true;
		public void Handle(Exception exception)
		{
			if (first) {
				if (errors >= 4) {
					first = false;
					Req();
				}
				errors++;
			} else {
			}
		}

		void Req()
		{
			Send("BufferSyncer", null, (args) => {
				var map = args[0].Value as Dictionary<string, QVariant>;

				var markerLines = map["MarkerLines"].Value as List<QVariant>;
				var lastSeenMsg = map["LastSeenMsg"].Value as List<QVariant>;

			});
			Send("BufferViewManager", null, (args) => {
				var map = args[0].Value as Dictionary<string, QVariant>;
				var list = map["BufferViewIds"].Value as List<QVariant>;
			});
			Send("AliasManager", null);
			Send("NetworkConfig", "GlobalNetworkConfig", (args) => {
				var map = args[0].Value as Dictionary<string, QVariant>;

				var pingTimeoutEnabled = map["pingTimeoutEnabled"].GetValue<bool>();
				var pingInterval = map["pingInterval"].GetValue<int>();
				var maxPingCount = map["maxPingCount"].GetValue<int>();
				var autoWhoNickLimit = map["autoWhoNickLimit"].GetValue<int>();
				var autoWhoInterval = map["autoWhoInterval"].GetValue<int>();
				var autoWhoEnabled = map["autoWhoEnabled"].GetValue<bool>();
				var autoWhoDelay = map["autoWhoDelay"].GetValue<int>();
			});
			Send("IgnoreListManager", null, (args) => {
				Send("BufferViewConfig", "0", (args2) => {
					var map = args2[0].Value as Dictionary<string, QVariant>;

					var bufferList = map["BufferList"].GetValue<List<QVariant>>().
						Select(item => item.GetValue<int>()).
						OrderBy(i => i);

					foreach (var bufferId in bufferList) {
						RequestBacklog(bufferId);
					}
				});
			});
		}

		internal void RequestBacklog(int bufferId, int firstMsgId = -1, int lastMsgId = -1, int limit = 100, int additional = 0)
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

			string classname = null;
			switch (type) {
			case RequestType.Sync:
				classname = list[1].GetValue<string>();
				switch (classname) {
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
				default:
					Console.Error.WriteLine("not handled: {0}", Show(list));
					break;
				}
				break;
			case RequestType.RpcCall:
				HandleRpcCallCommand(list);
				break;
			case RequestType.InitData:
				var className = GetString(list[1]);
				var objectName = list[2].Value as string;
				objectName = objectName == string.Empty ? null : objectName;
				bool done = false;
				foreach (var command in commands) {
					if (command.ClassName == className && command.ObjectName == objectName) {
						command.Callback(list.Skip(3).ToArray());
						commands.Remove(command);
						done = true;
						break;
					}
				}

				if (!done) {
					Console.Error.WriteLine("not handled: {0}", Show(list));
				}

				break;
			default:
				Console.Error.WriteLine("not handled: {0}", Show(list));
				break;
			}
		}

		string GetString(QVariant variable)
		{
			var tmp = variable.GetValue<byte[]>();
			if (tmp == null) {
				return null;
			}
			return Encoding.ASCII.GetString(tmp);
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

		public string Show(object o)
		{
			return QVariant.Inspect(o);
		}
	}
}

