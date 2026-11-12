using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2.NCostume;
using NShared;

namespace NGame2.NUtil.NCostume;

public struct HeroCustomizingDataGetter
{
	public Func<CachedHeroInfo> HeroInfoGetter;

	public Func<CostumeData> HeroCostumeDataGetter;

	public Func<HairCostumeComponent.OpenData> HairCostumeOpenDataGetter;

	public Func<WeaponCostumeComponent.OpenData> WeaponCostumeOpenDataGetter;

	public List<Func<AccessoryCostumeComponent.OpenData>> AccessoryCostumeOpenDataGetterList;

	public Func<AccessoryCostumeData, PositionInfo> AccessoryPositionInfoGetter;
}
