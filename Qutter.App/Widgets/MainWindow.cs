using System;
using System.Collections.Generic;
using Mono.Terminal;
using Manos.IO;

namespace Qutter.App
{
	public class MainWindow : VBox
	{
		public QuasselClient Client { get; protected set; }
		public ChatViewManager ChatViewManager { get; protected set; }
		public StatusBar StatusBar { get; protected set; }
		public StatusEntry Entry { get; protected set; }

		public MainWindow(QuasselClient client)
		{
			Client = client;

			ChatViewManager = new ChatViewManager(client, new ChatView());
			StatusBar = new StatusBar(client) { Height = 1 };
			Entry = new StatusEntry() { Height = 1, Prefix = "[syncing] " };

			Entry.Enter += delegate {
				if (Entry.Text.Length == 0) {
					return;
				}

				string text = Entry.Text.TrimEnd(new char [] { ' ', '\t' });

				if (text.StartsWith("/exit")) {
					Application.Exit = true;
				}

				Client.BufferSyncer.Active.Send(text);

				Entry.AddHistory(text);

				Entry.Text = "";
				Entry.Position = 0;
			};

			Client.BufferSyncer.Synced += (list) => {
				SetPrefix(Client.BufferSyncer.Active);
			};

			Client.BufferSyncer.ActiveChanged += SetPrefix;

			this.Add(ChatViewManager, Box.Setting.Fill);
			this.Add(StatusBar, Box.Setting.Size);
			this.Add(Entry, Box.Setting.Size);
		}

		public void SetPrefix(Buffer buffer)
		{
			string name = buffer.BufferInfo.Name;
			name = string.IsNullOrEmpty(name) ? "(status)" : name;
			Entry.Prefix = string.Format("[{0}] ", name);
		}

		public override bool ProcessKey(int key)
		{
			if (ChatViewManager.ProcessKey(key)) {
				return true;
			}
			return Entry.ProcessKey(key);
		}
	}

}

