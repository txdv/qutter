using System;
using System.Collections.Generic;
using Mono.Terminal;
using Manos.IO;

namespace Qutter.App
{
	public class MainWindowTemplate : VBox
	{
		public QuasselClient Client { get; protected set; }
		public ChatViewManager ChatViewManager { get; protected set; }
		public StatusBarTemplate StatusBar { get; protected set; }
		public EntryTemplate Entry { get; protected set; }

		public MainWindowTemplate(QuasselClient client, StatusBarTemplate bar, EntryTemplate entry)
		{
			Client = client;
			StatusBar = bar;
			Entry = entry;

			ChatViewManager = new ChatViewManager(client, new ChatView());

			SetPrefix("[syncing]");

			Entry.Enter += delegate {
				if (Entry.Text.Length == 0) {
					return;
				}

				string text = Entry.Text.TrimEnd(new char [] { ' ', '\t' });

				if (text.StartsWith("/exit")) {
					Application.Exit = true;
				} else if (text.StartsWith("/r")) {
					Client.RequestBacklog(Client.BufferSyncer.Active.BufferInfo.Id);
					return;
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

		public MainWindowTemplate(QuasselClient client)
			: this(client,
				   new StatusBarTemplate(client) { Height = 1 },
				   new EntryTemplate() { Height = 1 })
		{
		}

		public virtual void SetPrefix(Buffer buffer)
		{
			string name = buffer.BufferInfo.Name;
			name = string.IsNullOrEmpty(name) ? "(status)" : name;
			SetPrefix(string.Format("[{0}] ", name));
		}

		public virtual void SetPrefix(string prefix)
		{
			Entry.Prefix = prefix;
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

