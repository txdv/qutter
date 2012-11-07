using System;
using Qutter.App;
using LibuvSharp.Terminal;

namespace Qutter.App.Themes.Default
{
	public class Theme : ITheme
	{
		public string Name { get { return "qutter default"; } }
		public string Author { get { return "Andrius Bentkus"; } }

		public MainWindowTemplate CreateMainWindow(QuasselClient client)
		{
			return new MainWindow(client);
		}

		public static ColorString EscapeSpecials(string str, int accent = 202, int normal = 255, int brace = 241, int background = 237)
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

	}

	public class MainWindow : MainWindowTemplate
	{
		public MainWindow(QuasselClient client)
			: base(client, new StatusBar(client) { Height = 1 }, new EntryTemplate() { Height = 1 })
		{
		}

		public override void SetPrefix(string prefix)
		{
			Entry.ColorPrefix = Theme.EscapeSpecials(prefix, background: -1);
		}
	}

	public class StatusBar : StatusBarTemplate
	{
		public StatusBar(QuasselClient client)
			: base(client)
		{
		}

		public override void DrawStatusBar(string text)
		{
			DrawStatusBar(Theme.EscapeSpecials(text));
		}
	}
}

