using System;
using Mono.Terminal;

namespace Qutter.App
{
	public class IrcMessageChatViewEntry : ChatViewEntry
	{
		protected IrcMessage IrcMessage { get; set; }

		public string Nick {
			get {
				return IrcMessage.Sender.Split(new char[] { '!' })[0];
			}
		}

		public ColorString Prefix { get; protected set; }
		public ColorString Message { get; protected set; }

		public IrcMessageChatViewEntry(IrcMessage ircMessage)
		{
			IrcMessage = ircMessage;
			Mode = true;

			int AccentColor = 202;

			Prefix = new ColorString(string.Format("{0} {1}\x0000255  ", DecorateTime(AccentColor), DecorateNick(AccentColor, ' ')));

			var str = (IrcMessage.Contents == null ? "EMPTY" : IrcMessage.Contents);
			Message = new ColorString(str);
		}

		public override int CalculateHeight(int width)
		{
			if (Mode) {
				return (int)Math.Ceiling((double)Message.Length / (width - Prefix.Length));
			} else {
				return (int)Math.Ceiling((double)(Prefix.Length + Message.Length) / width);
			}
		}

		public string DecorateTime(int color)
		{
			var dateTime = IrcMessage.DateTime;
			return string.Format("\x0000241 (\x0000255 {1}\x0000{0} :\x0000255 {2}\x0000{0} :\x0000255 {3}am\x0000241 )",
				color,
				dateTime.ToString("hh"),
				dateTime.ToString("mm"),
				dateTime.ToString("ss")
			);
		}

		public virtual string DecorateNick(int color, char op) {
			if (op == ' ') {
				return string.Format("\x0000241 (\x0000255 {2}\x0000241 )",
					color,
					op,
					Nick
				);
			} else {
				return string.Format("\x0000241 (\x0000{0} {1}\x0000255 {2}\x0000241 )",
					color,
					op,
					Nick
				);
			}
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
				Message.Draw(this);
			}
		}
	}

	public class PlainChatViewEntry : IrcMessageChatViewEntry
	{
		public PlainChatViewEntry(IrcMessage message)
			: base(message)
		{
		}
	}

	public class JoinChatViewEntry : IrcMessageChatViewEntry
	{
		public string Channel {
			get {
				return IrcMessage.Contents;
			}
		}

		public JoinChatViewEntry(IrcMessage message)
			: base(message)
		{
			Prefix = new ColorString(DecorateTime(202));
			Message = new ColorString(string.Format(" \x0000255 {0} joined {1}", Nick, Channel));
		}
	}

	public class ActionChatViewEntry : IrcMessageChatViewEntry
	{
		public ActionChatViewEntry(IrcMessage message)
			: base(message)
		{
		}
		public override string DecorateNick(int color, char op) {
			if (op == ' ') {
				return string.Format("\x0000241 \x0000255 {2}\x0000241 ",
					color,
					op,
					Nick
				);
			} else {
				return string.Format("\x0000241 \x0000{0} {1}\x0000255 {2}\x0000241 ",
					color,
					op,
					Nick
				);
			}
		}

	}

	public class ExceptionChatViewEntry : ChatViewEntry
	{
		Exception Exception { get; set; }

		public ExceptionChatViewEntry(Exception exception)
		{
			Exception = exception;
		}

		public override void Redraw ()
		{
			Fill(Exception.ToString() + CalculateHeight(Width));
		}

		public override int CalculateHeight(int width)
		{
			var arr = Exception.ToString().Split(new char[] { '\n' });
			int lines = arr.Length;
			foreach (var str in arr) {
				lines += (int)Math.Ceiling((double)str.Length / Width - 1);
			}
			return lines;
		}
	}

	public class MultiLineStringChatViewEntry : ChatViewEntry
	{
		string Text { get; set; }

		public MultiLineStringChatViewEntry(string text)
		{
			Text = text;
		}

		public override void Redraw ()
		{
			Fill(Text);
		}

		public override int CalculateHeight (int width)
		{
			var arr = Text.Split(new char[] { '\n' });
			foreach (var ch in Text) {

				if (char.IsWhiteSpace(ch) && ch != ' ' && ch != '\n') {
					Console.Error.WriteLine((int)ch);
				}
			}
			int lines = 0;
			foreach (var str in arr) {
				int line = (int)Math.Ceiling((double)str.Length / width);
				lines += line;
			}
			return lines;
		}
	}

	public class CenterChatViewEntry : ChatViewEntry
	{
		public String Text { get; protected set; }
		public CenterChatViewEntry(string text)
		{
			Text = text;
		}

		public override int CalculateHeight (int width)
		{
			return 1;
		}

		public override void Redraw()
		{
			Fill(' ');
			int x = Width / 2 - Text.Length / 2;
			Draw(Text, x, 0);
		}
	}

	/*
	public class DecoratedChatViewEntry<T> : ChatView
	{
		T Object { get; set; }
		Theme Theme {
			get {
				return ThemeManger.Current;
			}
		}

		public DecoratedChatViewEntry(T Object)
		{
			Object = T;
		}

		public override int CalculateHeight(int width)
		{
			return Theme.Get(typeof(T)).CalculateHeight;
		}

		public override void Redraw ()
		{

		}
	}*/

}

