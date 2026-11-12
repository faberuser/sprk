using System.Diagnostics;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NUtil;

public class DrawRuntimeVariableHelper
{
	[Conditional("UNITY_EDITOR")]
	public void Assign(ItemBase item)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public void Release(ItemBase item)
	{
	}
}
