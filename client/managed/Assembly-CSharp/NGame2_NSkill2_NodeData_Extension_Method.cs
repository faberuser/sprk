using NGame2.NNodeTree;

public static class NGame2_NSkill2_NodeData_Extension_Method
{
	public static int GetNodeId(this NodeData This)
	{
		return This?.NodeId ?? (-1);
	}

	public static TreeData GetTree(this NodeData This)
	{
		return This?.Tree;
	}
}
