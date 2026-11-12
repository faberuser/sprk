using LitJson;
using NGame2.NNodeTree;

public static class Game2_NodeTree_StringKey_ExtensionMethod
{
	public static bool IsNullOrEmpty(this StringKey This)
	{
		if (This == null)
		{
			return true;
		}
		if (This.Index == 0)
		{
			return true;
		}
		return false;
	}

	public static JsonData MakeJson(this StringKey This)
	{
		if (This == null)
		{
			return null;
		}
		return StringKey.MakeJson(This);
	}
}
