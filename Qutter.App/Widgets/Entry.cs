using System;
using System.Collections.Generic;
using Mono.Terminal;

namespace Qutter.App
{
	public class Entry : Widget
	{
		public int Position { get; set; }

		public Entry()
			: base()
		{
			Text = "";
			Position = 0;
		}

		public string Text { get; set; }

		public override bool CanFocus {
			get {
				return true;
			}
		}

		public void Clear()
		{
			for (int x = 0; x < Width; x++) {
				for (int y = 0; y < Height; y++) {
					Set(X, Y, ' ');
				}
			}
		}

		public override void Redraw()
		{
			base.Redraw();
			Fill(Text);
			SetCursorPosition();
		}

		protected int GetFirstNonWhiteSpace()
		{
			int i = Position;
			while (i >= Text.Length) {
				i--;
			}

			for (; i > 0; i--) {
				if (Text[i] != ' ') {
					return i;
				}
			}

			return 0;
		}

		protected int GetFirstWhiteSpace()
		{
			int i = Position;
			while (i >= Text.Length) {
				i--;
			}

			for (; i > 0; i--) {
				if (Text[i] == ' ') {
					return i;
				}
			}

			return 0;
		}

		protected int GetFirst()
		{
			if (Text[Position - 1] == ' ') {
				return GetFirstNonWhiteSpace();
			} else {
				return GetFirstWhiteSpace();
			}
		}

		const int ctrl = 96;

		public override bool ProcessKey(int key)
		{
			char ch = (char)key;

			Invalid = true;

			switch (key) {
			case (int)'a' - ctrl:
				Position = 0;
				return true;
			case (int)'e' - ctrl:
				Position = Text.Length;
				return true;
			case (int)'w' - ctrl:
				if (Position == 1) {
					Text = Text.Substring(1);
					Position = 0;
				} else if (Position != 0) {
					int i = GetFirst() + 1;
					if (i == 1) {
						Text = Text.Substring(Position);
						Position = 0;
					} else {
						Text = Text.Substring(0, i);
						Position = i;
					}
				}
				return true;
			case 126:
				if (Position < Text.Length) {
					Text = Text.Substring(0, Text.Length - 1);
				}
				return true;
			case 127:
			case Curses.Key.Backspace:
				if (Position > 0) {
					Text = Text.Substring(0, Position - 1) + Text.Substring(Position);
					Position--;
				}
				return true;
			case Curses.Key.Left:
				if (Position > 0) {
					Position--;
				}
				return true;
			case Curses.Key.Right:
				if (Position < Text.Length) {
					Position++;
				}
				return true;
			case Curses.Key.Delete:
				if (Position < Text.Length) {
					Text = Text.Substring(0, Position) + Text.Substring(Position + 1);
				}
				return true;
			case 10:
				if (Enter != null) {
					Enter();
				}
				return true;
			default:
				if (key < 32 || key > 255) {
					return false;
				} else {
					Text = Text.Substring(0, Position) + ch + Text.Substring(Position);
					Position++;
					return true;
				}
			}
		}

		public override void SetCursorPosition()
		{
			Move(Position, 0);
		}

		public event Action Enter;
	}

	public class HistoryEntry : Entry
	{
		public HistoryEntry()
		{
			History = new LinkedList<string>();
		}

		public void AddHistory(string text)
		{
			History.AddLast(text);

			CurrentHistoryElement = null;
		}

		public LinkedList<string> History { get; set; }

		public LinkedListNode<string> CurrentHistoryElement { get; set; }

		public string Current {
			get {
				if (CurrentHistoryElement.Value == null) {
					return string.Empty;
				} else {
					return CurrentHistoryElement.Value;
				}
			}
		}

		public string PrevHistory()
		{
			if (CurrentHistoryElement == null) {
				if (History.Last == null) {
					return string.Empty;
				}
				CurrentHistoryElement = History.Last;
			} else {
				if (CurrentHistoryElement.Previous != null) {
					CurrentHistoryElement = CurrentHistoryElement.Previous;
				}
			}
			return CurrentHistoryElement.Value;
		}

		public string NextHistory()
		{
			if (CurrentHistoryElement == null) {
				if (History.First == null) {
					return string.Empty;
				}
				CurrentHistoryElement = History.Last;
			} else {
				if (CurrentHistoryElement.Next != null) {
					CurrentHistoryElement = CurrentHistoryElement.Next;
				}
			}
			return CurrentHistoryElement.Value;
		}

		public override bool ProcessKey(int key)
		{
			switch (key) {
			case Curses.Key.Up:
				Text = PrevHistory();
				Position = Text.Length;
				return true;
			case Curses.Key.Down:
				Text = NextHistory();
				Position = Text.Length;
				return true;
			default:
				return base.ProcessKey (key);
			}
		}
	}

	public class EntryTemplate : HistoryEntry
	{
		public string Prefix {
			get { return ColorPrefix.String; }
			set { ColorPrefix = new ColorString(value); }
		}

		ColorString prefix;
		public ColorString ColorPrefix {
			get { return prefix; }
			set { prefix = value; Invalid = true; }
		}

		public override void Redraw()
		{
			Invalid = false;
			ColorPrefix.Draw(this, 0, 0, ColorPrefix.Length, 1);
			Curses.attron(ColorPair.From(-1, -1).Attribute);
			Fill(Text, ColorPrefix.Length, 0);
		}

		public override void SetCursorPosition()
		{
			Move(ColorPrefix.Length + Text.Length, 0);
		}
	}
}

