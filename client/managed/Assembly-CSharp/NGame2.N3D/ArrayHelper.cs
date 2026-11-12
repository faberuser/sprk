namespace NGame2.N3D;

public static class ArrayHelper
{
	private static bool IsValidIndex<T>(this T[] list, int index)
	{
		if (index < 0)
		{
			return false;
		}
		if (list.Length <= index)
		{
			return false;
		}
		return true;
	}
}
