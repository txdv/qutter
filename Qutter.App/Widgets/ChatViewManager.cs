using System;
using System.Collections.Generic;
using LibuvSharp;
using LibuvSharp.Terminal;

namespace Qutter.App
{
	public class ChatViewManager : Container
	{
		static Dictionary<char, int> mapKeys = new Dictionary<char, int>();

		static ChatViewManager()
		{
			int i = 1;
			foreach (var ch in new char[] {
				'1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
				'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p',
				'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', ';',
				'z', 'x', 'c', 'v', 'b', 'n', 'm', ',', '.', '/'
			}) {
				mapKeys.Add(ch, i++);
			}

			Curses.Key.Register(27);
		}

		static int GetKeyMap(int key)
		{
			char ch = (char)Curses.Key.Base(key);

			int val;
			if (mapKeys.TryGetValue(ch, out val)) {
				return val;
			} else {
				return -1;
			}
		}

		public QuasselClient Client { get; protected set; }
		public Loop Loop { get; protected set; }

		public void Debug(string message)
		{
			DebugChatView.Add(new MultiLineStringChatViewEntry(message));

			Client.BufferSyncer.Synced += delegate(Buffer[] obj) {
				Initialized = true;
			};
		}

		public bool Initialized { get; protected set; }

		public ChatView DebugChatView { get; set; }
		public bool DebugMode { get; protected set; }

		public ChatView ActiveChatView {
			get {
				if (!Initialized || DebugMode) {
					return DebugChatView;
				} else {
					return Get(Client.BufferSyncer.Active.Id);
				}
			}
		}

		Dictionary<int, ChatView> chatviews = new Dictionary<int, ChatView>();
		ChatView Get(int id) {
			ChatView chatView;
			if (!chatviews.TryGetValue(id, out chatView)) {
				chatView = new ChatView();
				chatView.SetDim(X, Y, Width, Height);
				chatView.Container = this;
				chatviews[id] = chatView;
			}
			return chatView;

		}

		public ChatViewManager(QuasselClient client, ChatView debug)
			: base()
		{
			Client = client;

			if (debug == null) {
				throw new ArgumentException("debug");
			}

			Client.BufferSyncer.ActiveChanged += (obj) => {
				//Application.Refresh();
			};

			Client.BufferSyncer.Synced += delegate(Buffer[] buffers) {
				foreach (var buffer in buffers) {
					Get(buffer.Id);
				}
			};

			Client.BufferSyncer.Message += (buffer, message) => {
				Get(buffer.Id).Add(new IrcMessageChatViewEntry(message));
			};

			DebugChatView = debug;
			DebugChatView.Add(new CenterChatViewEntry("Debug console"));
		}

		public override void Redraw()
		{
			ActiveChatView.Redraw();
		}

		public override bool ProcessKey(int key)
		{
			if (Curses.Key.Is(key, 27)) {
				int active = GetKeyMap(key);

				DebugChatView.Add(new MultiLineStringChatViewEntry(active.ToString()));
				if (active < 0 || active > Client.BufferSyncer.Count) {
					return false;
				}
				Client.BufferSyncer.Current = active;
				var bi = Client.BufferSyncer.Active.BufferInfo;
				var val = string.Format("{0}:{1}({2})", bi.Name, bi.Type, bi.NetworkId);
				DebugChatView.Add(new MultiLineStringChatViewEntry(val.ToString()));

				return true;
			}
			switch (key) {
			case 4:
				DebugMode = !DebugMode;
				return true;
			case 14:
				Client.BufferSyncer.Next();
				return true;
			case 16:
				Client.BufferSyncer.Prev();
				return true;
			default:
				return ActiveChatView.ProcessKey(key);
			}
		}

		public override void SetDim(int x, int y, int w, int h)
		{
			base.SetDim(x, y, w, h);
			DebugChatView.SetDim(x, y, w, h);
			foreach (var chatView in chatviews) {
				chatView.Value.SetDim(x, y, w, h);
			}
		}

		public override bool CanFocus {
			get {
				return ActiveChatView.CanFocus;
			}
		}

		public override void SetCursorPosition()
		{
			ActiveChatView.SetCursorPosition();
		}

	}

}

