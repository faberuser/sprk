using NShared.NJit;

namespace NShared;

public class SoulWeaponNodeTreeDataContainer : JitDataContainerDefault<string, SoulWeaponNodeTreeData>
{
	public const string TABLENAME = "SoulWeaponNodeTreeTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SoulWeaponNodeTreeTable");

	public SoulWeaponNodeTreeData GetDataFromSoulStoneIndex(int soulStoneIndex)
	{
		foreach (SoulWeaponNodeTreeData allDatum in GetAllData())
		{
			if (allDatum.SoulStoneItemIndex == soulStoneIndex)
			{
				return allDatum;
			}
		}
		return null;
	}
}
