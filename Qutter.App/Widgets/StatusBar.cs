using System;
using System.Text;
using System.Collections.Generic;
using Mono.Terminal;
using Manos.IO;

namespace Qutter.App
{
	public class Attribute
	{
		public Attribute(ColorPair pair)
		{
			Pair = pair;
		}

		public Attribute()
			: this(ColorPair.From(-1, -1))
		{
		}

		public ColorPair Pair { get; protected set; }
		public bool Bold { get; protected set; }
		public bool Underline { get; protected set; }
	}

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

		protected string EscapeSpecials(string str, int normal, int accent, int background)
		{
			return EscapeSpecials(str, ColorPair.From(accent, background), ColorPair.From(normal, background));
		}

		protected string EscapeSpecials(string str, ColorPair selection, ColorPair normal)
		{
			return EscapeSpecials(str, (c) => {
				return string.Format("\x0000{0},{1} {2}\x000{3},{4} ", selection.Foreground,
					selection.Background, c, normal.Foreground, normal.Background);
			});
		}

		protected string Colorize(string str, int fg, int bg)
		{
			return Colorize(str, ColorPair.From(fg, bg));
		}

		protected string Colorize(string str, ColorPair normal)
		{
			return string.Format("\0000{0},{1} {2}", normal.Foreground, normal.Background, str);
		}

		protected string EscapeSpecials(string str, Func<char, string> exchange)
		{
			string ret = "";
			foreach (var c in str) {
				if (!Char.IsLetter(c) && !Char.IsDigit(c)) {
					ret += exchange(c);
				} else {
					ret += c;
				}
			}
			return ret;
		}

		protected string Time(DateTime time, int accent, int normal, int brace, int background)
		{
			var br = ColorPair.From(brace, background);
			var nr = ColorPair.From(normal, background);
			var spec = ColorPair.From(accent, background);
			return Time(time, br, nr, spec);
		}

		protected string Time(DateTime time, ColorPair br, ColorPair nr, ColorPair spec)
		{
			var t = EscapeSpecials(time.ToString(), spec, nr);
			return string.Format("{0}[{1}{2}{3}]{0}", br, nr, t, br);
		}

		protected string Fill(string str, ColorPair pair)
		{
			int len = ColorString.CalculateLength(str);
			string s = "";
			for (int i = len; i < Width; i++) {
				s += " ";
			}
			return string.Format("\x0000{0},{1} {2}{3}", pair.Foreground, pair.Background, str, s);
		}

		protected string GetChannels(int bg, int accent, int normal, int braces)
		{
			StringBuilder sb = new StringBuilder();
			string activename = Client.BufferSyncer.Active.BufferInfo.Name;
			activename = (string.IsNullOrEmpty(activename) ? "status" : activename);

			foreach (var buffer in Client.BufferSyncer) {
				var info = buffer.BufferInfo;
				bool active = buffer == Client.BufferSyncer.Active;
				string name = (info.Type == BufferInfo.BufferType.Status ? "status" : info.Name);
				name = EscapeSpecials(name, normal, accent, bg);
				sb.Append(string.Format("{1} ", 255, name));
			}
			return sb.ToString();
		}

		public override void Redraw()
		{
			base.Redraw();
			int bg = 237;
			int accent = 202;
			int normal = 255;
			int braces = 241;

			Fill(' ');

			string str = "";
			if (Client.BufferSyncer.IsSynced) {
				str = GetChannels(bg, accent, normal, braces);
			}

			str = Time(DateTime.Now, accent, normal, 241, bg) + " " + str;
			ColorString.Draw(this, Fill(str, ColorPair.From(normal, bg)), 0, 0, Width, Height);
			Curses.attron(ColorPair.From(-1, -1).Attribute);
		}
	}
}

