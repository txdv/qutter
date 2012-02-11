using System;
using System.Collections.Generic;
using Mono.Terminal;

namespace Qutter.App
{
	abstract public class ChatViewEntry : Widget
	{
        public ChatView ChatView { get; internal set; }

        abstract public int CalculateHeight(int width);
	}

	public class ChatView : Widget
	{
		private LinkedList<ChatViewEntry> entries = new LinkedList<ChatViewEntry>();

		public ChatView()
		{
			PageFactor = 0.8;
		}

		protected LinkedListNode<ChatViewEntry> Position { get; set; }

		public override void SetDim(int x, int y, int w, int h)
		{
			base.SetDim(x, y, w, h);

			Invalid = true;
		}

		public double PageFactor { get; set; }

		public void Add(ChatViewEntry entry)
		{
			Invalid = true;

			entry.ChatView = this;

			entries.AddLast(entry);
		}

		protected LinkedListNode<ChatViewEntry> PrevPosition(int lines)
		{
			if (Position == null) {
				var current = entries.Last;
				if (current == null) {
					return current;
				}
				int l = lines;
				while (current.Previous != null && l > 0) {
					l -= current.Value.CalculateHeight(Width);
					current = current.Previous;
				}

				while (current.Previous != null && lines > 0) {
					lines -= current.Value.CalculateHeight(Width);
					current = current.Previous;
				}

				return current;
			} else {
				var current = Position;
				while (current.Previous != null && lines > 0) {
					lines -= current.Value.CalculateHeight(Width);
					current = current.Previous;
				}

				return current;
			}
		}

		protected LinkedListNode<ChatViewEntry> NextPosition(int lines)
		{
			if (Position == null || Position == entries.Last) {
				return null;
			}

			LinkedListNode<ChatViewEntry> current;

			for (current = Position; current != null && lines > 0; current = current.Next) {
				lines -= current.Value.CalculateHeight(Width);
			}

			var ret = current;

			if (ret == entries.Last || ret == null) {
				return null;
			}

			lines = Height;

			for (;lines > 0; current = current.Next) {
				if (current == null) {
					return null;
				}
				lines -= current.Value.CalculateHeight(Width);
			}


			return ret;
		}

		public override bool ProcessKey(int key)
		{
			switch (key) {
			case 339:
				Position = PrevPosition((int)(Height * PageFactor));
				Invalid = true;
				return true;
			case 338:
				if (Position == null) {
					return true;
				}
				Position = NextPosition((int)(Height * PageFactor));
				Invalid = true;
				return true;
			default:
				return false;
			}
		}

		public override void Redraw()
		{
			/*
			if (Invalid) {
				Invalid = false;
			} else {
				return;
			}
			*/

			ChatViewEntry entry;

			LinkedListNode<ChatViewEntry> element;

			int x = X;
			int h = 0;
			int w = Width;

			if (Position == null) {
				for (element = entries.Last; element != null && h < Height; element = element.Previous) {
					entry = element.Value;
					entry.Height = entry.CalculateHeight(w);
					h += entry.Height;
					if (entry.Height < 0) {
						throw new Exception(string.Format("{0}.CalculateHeight returns negative number {1}", entry.GetType(), entry.Height));
					}
				}

				if (h < Height) {
					h = 0;
					for (element = entries.First; element != null; element = element.Next) {
						entry = element.Value;
						entry.SetDim(x, h, w, entry.Height);

						h += entry.Height;

						entry.Redraw();
					}
					// fill the rest!
					Fill(' ', 0, h);
				} else {
					h = Height;
					for (element = entries.Last; element != null; element = element.Previous) {
						if (h < 0) {
							return;
						}
						entry = element.Value;
						int ch = entry.CalculateHeight(w);
						if (ch < 0) {
							throw new Exception(string.Format("{0}.CalculateHeight returns negative number {1}", entry.GetType(), entry.Height));
						}
						h -= ch;
						entry.SetDim(x, h, w, ch);
						entry.Redraw();
					}
				}
			} else {
				for (element = Position; element != null && h < Height; element = element.Next) {
					entry = element.Value;

					int th = entry.CalculateHeight(w);
					if (h + th > Height) {
						entry.SetDim(x, h, w, Height - h);
					} else {
						entry.SetDim(x, h, w, th);
					}

					entry.Redraw();

					h += entry.Height;
				}
			}
		}

		protected void DrawTop()
		{
		}
	}

}

