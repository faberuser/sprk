using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class GetNewHeroSceneBundleDataContainer : JitDataContainerDefault<int, GetNewHeroSceneBundleData>
{
	public const string TABLENAME = "GetNewHeroSceneBundleTable";

	private Dictionary<int, NShared.GetNewHeroSceneBundleNode> _dicStartNode = new Dictionary<int, NShared.GetNewHeroSceneBundleNode>();

	public override TablePathDefine PathDefine => new TablePathDefine("GetNewHeroSceneBundleTable");

	protected override void OnEndLoad_Final()
	{
		foreach (GetNewHeroSceneBundleData allDatum in GetAllData())
		{
			GenerateStartNode(allDatum);
		}
	}

	private void GenerateStartNode(GetNewHeroSceneBundleData bundleData)
	{
		if (bundleData == null || bundleData.BundleHeroIndices == null || bundleData.BundleHeroIndices.Length < 2)
		{
			return;
		}
		int num = bundleData.BundleHeroIndices[0];
		NShared.GetNewHeroSceneBundleNode value = null;
		if (!_dicStartNode.TryGetValue(num, out value))
		{
			value = new NShared.GetNewHeroSceneBundleNode(num);
			_dicStartNode[num] = value;
		}
		NShared.GetNewHeroSceneBundleNode getNewHeroSceneBundleNode = value;
		for (int i = 1; i < bundleData.BundleHeroIndices.Length; i++)
		{
			int heroIndex = bundleData.BundleHeroIndices[i];
			NShared.GetNewHeroSceneBundleNode getNewHeroSceneBundleNode2 = getNewHeroSceneBundleNode.GetNext(heroIndex);
			if (getNewHeroSceneBundleNode2 == null)
			{
				NShared.GetNewHeroSceneBundleNode getNewHeroSceneBundleNode3 = new NShared.GetNewHeroSceneBundleNode(heroIndex);
				getNewHeroSceneBundleNode.AddNext(getNewHeroSceneBundleNode3);
				getNewHeroSceneBundleNode2 = getNewHeroSceneBundleNode3;
			}
			getNewHeroSceneBundleNode = getNewHeroSceneBundleNode2;
		}
		getNewHeroSceneBundleNode.Data = bundleData;
	}

	public List<GetNewHeroSceneBundleData> GetListOfIncludingHeroIndex(int heroIndex)
	{
		List<GetNewHeroSceneBundleData> list = null;
		foreach (GetNewHeroSceneBundleData allDatum in GetAllData())
		{
			if (allDatum.IsIncludeHeroIndex(heroIndex))
			{
				if (list == null)
				{
					list = new List<GetNewHeroSceneBundleData>();
				}
				list.Add(allDatum);
			}
		}
		if (list == null)
		{
			return null;
		}
		return list;
	}

	public GetNewHeroSceneBundleData GetData(List<int> heroIndices)
	{
		if (heroIndices == null || heroIndices.Count < 2)
		{
			return null;
		}
		NShared.GetNewHeroSceneBundleNode value = null;
		if (!_dicStartNode.TryGetValue(heroIndices[0], out value))
		{
			return null;
		}
		NShared.GetNewHeroSceneBundleNode getNewHeroSceneBundleNode = null;
		for (int i = 1; i < heroIndices.Count; i++)
		{
			NShared.GetNewHeroSceneBundleNode next = value.GetNext(heroIndices[i]);
			if (next != null)
			{
				getNewHeroSceneBundleNode = next;
			}
		}
		return getNewHeroSceneBundleNode?.Data;
	}

	public List<GetNewHeroSceneBundleData> GetEnableDataList(List<int> heroIndices)
	{
		if (heroIndices == null || heroIndices.Count < 2)
		{
			return null;
		}
		List<GetNewHeroSceneBundleData> list = new List<GetNewHeroSceneBundleData>();
		for (int i = 0; i < heroIndices.Count; i++)
		{
			int j = i + 1;
			if (j >= heroIndices.Count)
			{
				break;
			}
			NShared.GetNewHeroSceneBundleNode value = null;
			if (!_dicStartNode.TryGetValue(i, out value))
			{
				continue;
			}
			for (; j < heroIndices.Count; j++)
			{
				NShared.GetNewHeroSceneBundleNode getNewHeroSceneBundleNode = value;
				for (int k = j; k < heroIndices.Count; k++)
				{
					NShared.GetNewHeroSceneBundleNode next = getNewHeroSceneBundleNode.GetNext(heroIndices[k]);
					if (next != null)
					{
						getNewHeroSceneBundleNode = next;
						if (getNewHeroSceneBundleNode.Data != null)
						{
							list.Add(getNewHeroSceneBundleNode.Data);
						}
					}
				}
			}
		}
		return list;
	}
}
