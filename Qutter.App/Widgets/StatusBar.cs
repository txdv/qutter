using System;
using System.Text;
using System.Collections.Generic;
using Mono.Terminal;
using Manos.IO;

namespace Qutter.App
{
	public class StatusBarTemplate : Widget
	{
		public QuasselClient Client { get; protected set; }

		public StatusBarTemplate(QuasselClient client)
		{
			Client = client;

			Client.BufferSyncer.Synced += (obj) => {
				Invalid = true;
			};

			Application.Context.CreateTimerWatcher(TimeSpan.Zero, TimeSpan.FromSeconds(1), () => {
				Invalid = true;
			}).Start();
		}

		public override void Redraw()
		{
			base.Redraw();

			StringBuilder sb = new StringBuilder();

			sb.Append(string.Format("[{0}] ", DateTime.Now));

			if (Client.BufferSyncer.IsSynced) {
				var network = Client.NetworkCollection.Get(Client.BufferSyncer.Active.BufferInfo.NetworkId);
				sb.Append(string.Format("[Lat: {0}] ", network.Latency));

				foreach (var buffer in Client.BufferSyncer) {
					var info = buffer.BufferInfo;
					string name = (info.Type == BufferInfo.BufferType.Status ? "status" : info.Name);
					sb.Append(name);
					sb.Append(" ");
				}
			}
			DrawStatusBar(sb.ToString());
		}

		public virtual void DrawStatusBar(string text)
		{
			Fill(text);
		}

		public virtual void DrawStatusBar(ColorString text)
		{
			text.Fill(this);
			Curses.attron(ColorPair.From(-1, -1).Attribute);
		}
	}
}

