using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class WeaponCostumeDataContainer : JitDataContainerDefault<int, WeaponCostumeData>
{
	public const string TABLENAME = "WeaponCostumeTable";

	private JitDictionary<int, List<WeaponCostumeData>> _lists_by_HeroIndex = new JitDictionary<int, List<WeaponCostumeData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("WeaponCostumeTable");

	public WeaponCostumeDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_HeroIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (WeaponCostumeData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_HeroIndex.JsonLoad_AddToListValue(item.HeroIndex, item);
		}
	}

	public IList<WeaponCostumeData> GetDataListByHeroIndex(int heroIndex)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_lists_by_HeroIndex.GetOrDefault(heroIndex));
	}

	public WeaponCostumeData GetDataByHeroStar(int heroIndex, int star)
	{
		foreach (WeaponCostumeData item in __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_lists_by_HeroIndex.GetOrDefault(heroIndex)))
		{
			if (item.Star == star)
			{
				return item;
			}
		}
		return null;
	}
}
