using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Qutter.App
{
	public interface ITheme
	{
		string Name { get; }
		string Author { get; }
		MainWindowTemplate CreateMainWindow(QuasselClient client);
	}

	public class ThemeManger
	{
		List<ITheme> themes = new List<ITheme>();

		public ThemeManger()
		{
			Load(Assembly.GetExecutingAssembly());

			Default = themes.Find((theme) => theme.GetType() == typeof(Qutter.App.Themes.Default.Theme));
		}

		public ITheme Default { get; private set; }
		public ITheme Current { get; set; }

		public void Load(Assembly asm)
		{
			var tmp = from type in asm.GetTypes()
				where type.GetInterfaces().Contains(typeof(ITheme))
				select type.GetConstructor(new Type[] { }).Invoke(null) as ITheme;

			themes.AddRange(tmp);
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

