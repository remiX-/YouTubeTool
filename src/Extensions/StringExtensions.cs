using System.Linq;

namespace YouTubeTool.Extensions
{
	public static class StringExtensions
	{
		public static string NullIfBlank(this string str)
		{
			return str.IsBlank() ? null : str;
		}

		public static bool IsBlank(this string str)
		{
			return string.IsNullOrEmpty(str);
		}

		public static bool IsNotBlank(this string str)
		{
			return !str.IsBlank();
		}

		public static string Replace(this string str, char[] list, char replace)
		{
			return list.Aggregate(str, (current, c) => current.Replace(c, replace));
		}
	}
}
