using System.IO;

public static class String_Extension_Method
{
	public static string EmptyIfNull(this string This)
	{
		if (This != null)
		{
			return This;
		}
		return string.Empty;
	}

	public static bool IsNullOrEmpty(this string This)
	{
		return string.IsNullOrEmpty(This);
	}

	public static bool TryToInt(this string This, out int output)
	{
		return int.TryParse(This, out output);
	}

	public static string CombinePath(this string This, string rhs)
	{
		return This.TrimEnd('/', '\\') + "/" + rhs.TrimStart('/', '\\');
	}

	public static string CombineFullPath(this string This, string rhs)
	{
		return Path.GetFullPath(Path.Combine(This, rhs));
	}
}
