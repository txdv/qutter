using System;
using System.Text;
using System.Collections.Generic;
using Mono.Terminal;
using Manos.IO;

namespace Qutter.App
{
	public class StatusBar : Widget
	{
		public QuasselClient Client { get; protected set; }

		public StatusBar(QuasselClient client)
		{
			Client = client;

			Client.BufferSyncer.Synced += (obj) => {
				Invalid = true;
			};

			Application.Context.CreateTimerWatcher(TimeSpan.Zero, TimeSpan.FromSeconds(1), () => {
				Invalid = true;
			}).Start();
		}

		protected ColorString EscapeSpecials(string str, int accent, int normal, int brace, int background)
		{
			return ColorString.Escape(str, (ch) => {
				switch (ch) {
				case '[':
				case ']':
					return ColorPair.From(brace, background);
				default:
					if (Char.IsDigit(ch) || Char.IsLetter(ch)) {
						return ColorPair.From(normal, background);
					}
					return ColorPair.From(accent, background);
				}
			});
		}

		public override void Redraw()
		{
			base.Redraw();
			int bg = 237;
			int accent = 202;
			int normal = 255;
			int braces = 241;

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
			EscapeSpecials(sb.ToString(), accent, normal, braces, bg).Fill(this);
			Curses.attron(ColorPair.From(-1, -1).Attribute);
		}
	}
}

