using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NNodeTree.NObject;

public class PivotInvisible : ObjectBase
{
	public const string PrefabPath = "Prefab/NodeTree/PivotInvisible";

	public static PrefabAuto<PivotInvisible> PrefabAuto = new PrefabAuto<PivotInvisible>("Prefab/NodeTree/PivotInvisible", isGlobal: false, EPrefabType.Default);

	public override Transform FindTransform(string nodeName)
	{
		return base.transform;
	}

	public override void ChainUpdate()
	{
	}

	public override void Visit(IVisitor visitor)
	{
	}

	protected override void OnCreateDerived()
	{
	}

	protected override void OnReleaseDerived()
	{
	}
}
