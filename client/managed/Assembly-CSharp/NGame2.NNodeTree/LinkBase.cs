namespace NGame2.NNodeTree;

public abstract class LinkBase
{
	public NodeData OwnerNode => Contents.GetNode();

	public ContentsBase Contents { get; private set; }

	public string LinkName { get; private set; }

	protected LinkBase(ContentsBase i_contents, string i_name)
	{
		i_contents.LinkDic.ContainsKey(i_name);
		Contents = i_contents;
		LinkName = i_name;
		i_contents.LinkDic[i_name] = this;
	}

	public void UnlinkContents()
	{
		if (Contents != null)
		{
			Contents.LinkDic.Remove(LinkName);
		}
	}

	public string GetDebugPath()
	{
		string linkName = LinkName;
		if (Contents == null)
		{
			return linkName;
		}
		linkName = Contents.GETCONTENTSTYPENAME() + ":" + linkName;
		if (Contents.Node == null)
		{
			return linkName;
		}
		linkName = Contents.Node.NodeId + ":" + linkName;
		if (Contents.Node.Tree == null)
		{
			return linkName;
		}
		return Contents.Node.Tree.Code + ":" + linkName;
	}
}
