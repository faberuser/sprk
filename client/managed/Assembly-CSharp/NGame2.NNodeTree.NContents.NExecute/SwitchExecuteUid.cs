namespace NGame2.NNodeTree.NContents.NExecute;

public class SwitchExecuteUid : SwitchExecute<long>
{
	public static string ContentsTypeName = "Execute/" + typeof(SwitchExecuteUid).Name;

	public SwitchExecuteUid(NodeData i_node)
		: base(i_node)
	{
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}
}
