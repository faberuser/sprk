namespace NGame2.NNodeTree.NContents.NExecute;

public class SwitchExecuteString : SwitchExecute<string>
{
	public static string ContentsTypeName = "Execute/" + typeof(SwitchExecuteString).Name;

	public SwitchExecuteString(NodeData i_node)
		: base(i_node)
	{
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}
}
