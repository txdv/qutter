using System;
using Mono.Terminal;

	/*

	public class ViewPortEntry : Widget
	{
		public ChatView ChatView { get; internal set; }

		public ColorString Prefix { get; protected set; }
		public ColorString Message { get; protected set; }

		public ViewPortInfo Info { get; protected set; }

		public ViewPortEntry(ViewPortInfo info)
		{
			Mode = true;

			Info = info;

			int AccentColor = 202;

			Prefix = new ColorString(string.Format("{0} {1}\x0000255  ", DecorateTime(AccentColor), DecorateNick(AccentColor, '+')));
			Message = new ColorString(info.Message);
		}

		public int CalculateHeight(int width)
		{
			if (Mode) {
				return (int)Math.Ceiling((double)Message.Length / (width - Prefix.Length));
			} else {
				return (int)Math.Ceiling((double)(Prefix.Length + Message.Length) / width);
			}
		}

		public string DecorateTime(int color)
		{
			return string.Format("\x0000241 (\x0000255 {1}\x0000{0} :\x0000255 {2}\x0000{0} :\x0000255 {3}am\x0000241 )",
				color,
				Info.DateTime.ToString("hh"),
				Info.DateTime.ToString("mm"),
				Info.DateTime.ToString("ss")
			);
		}

		public string DecorateNick(int color, char op) {
			return string.Format("\x0000241 (\x0000{0} {1}\x0000255 {2}\x0000241 )",
				color,
				op,
				Info.Nick
			);
		}

		public bool Mode { get; set; }

		public override void Redraw()
		{
			Fill(' ');

			Move(0, 0);

			if (Mode) {
				Prefix.Draw(this);
				Message.Draw(this, Prefix.Length, 0, Width - Prefix.Length, Height);
			} else {
				Prefix.Draw(this);
				Message.Draw();
				//Message.Draw(this);
			}
		}

		public int Length { get; protected set; }
	}

		*/
namespace Qutter.App
{
	public class Theme
	{
	}

	public class DefaultTheme : Theme
	{
		public static int AccentColor = 202;
		public static bool Mode { get; set; }

		public DefaultTheme()
		{
			//IIrcMessageThemeHandler = new IrcMessageThemeHandler();
		}

		public IIrcMessageThemeHandler IrcMessageTheme  { get; set; }
	}

	public static class ThemeManger
	{
		static ThemeManger()
		{
			Default = new DefaultTheme();
		}

		public static Theme Default { get; set; }
		public static Theme Current { get; set; }

		public static void Change(Theme theme)
		{

		}
	}

	public interface IIrcMessageThemeHandler
	{
		int CalculateHeight(int width);
	}

	public class IrcMessageThemeHandler : IIrcMessageThemeHandler
	{
		public int CalculateHeight(int width)
		{
			return 1;
		}
	}
}

