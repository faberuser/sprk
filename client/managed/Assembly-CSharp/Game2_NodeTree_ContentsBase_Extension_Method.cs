using NGame2.NNodeTree;

public static class Game2_NodeTree_ContentsBase_Extension_Method
{
	public static NodeData GetNode(this ContentsBase This)
	{
		return This?.Node;
	}

	public static string GETCONTENTSTYPENAME(this ContentsBase This)
	{
		if (This == null)
		{
			return "(null)";
		}
		return This.GetContentsTypeName();
	}
}
