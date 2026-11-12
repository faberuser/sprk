using System.Collections.Generic;
using NVespa;

namespace NShared;

internal class GetNewHeroSceneBundleNode
{
	public readonly int HeroIndex;

	private List<NShared.GetNewHeroSceneBundleNode> nextNodes;

	public GetNewHeroSceneBundleData Data;

	public GetNewHeroSceneBundleNode(int heroIndex)
	{
		HeroIndex = heroIndex;
	}

	public void AddNext(NShared.GetNewHeroSceneBundleNode nextNode)
	{
		if (nextNode.HeroIndex < HeroIndex)
		{
			VespaDebug.LogError($"AddNext {nextNode.HeroIndex} < {HeroIndex}");
			return;
		}
		if (nextNodes == null)
		{
			nextNodes = new List<NShared.GetNewHeroSceneBundleNode>();
		}
		if (!nextNodes.Contains(nextNode))
		{
			nextNodes.Add(nextNode);
		}
	}

	public NShared.GetNewHeroSceneBundleNode GetNext(int heroIndex)
	{
		if (nextNodes == null || nextNodes.Count == 0)
		{
			return null;
		}
		return nextNodes.Find((NShared.GetNewHeroSceneBundleNode node) => node.HeroIndex == heroIndex);
	}
}
