using NGame2.NNodeTree.NRuntime;
using NShared;
using NVespa;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NString;

public static class ContentsFunc
{
	public static string GetSoulStoneBodyPrefix(IVariable variable, long itemIndex)
	{
		SoulWeaponNodeTreeData soulWeaponNodeTreeData = null;
		foreach (SoulWeaponNodeTreeData allDatum in GameRoot.Instance.SoulWeaponNodeTreeDataContainer.GetAllData())
		{
			if (allDatum.SoulStoneItemIndex == itemIndex)
			{
				soulWeaponNodeTreeData = allDatum;
				break;
			}
		}
		if (soulWeaponNodeTreeData == null)
		{
			VespaDebug.LogError($"Can Not Find SoulWeaponNodeTreeData Index Of ({itemIndex})");
			return string.Empty;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(soulWeaponNodeTreeData.SoulStoneCreatureIndex);
		if (data == null)
		{
			VespaDebug.LogError($"Can Not Find CreatureData Index Of ({soulWeaponNodeTreeData.SoulStoneCreatureIndex})");
			return string.Empty;
		}
		return data.ModelPrefix;
	}
}
