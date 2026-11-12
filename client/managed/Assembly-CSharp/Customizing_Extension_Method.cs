using System;
using System.Collections.Generic;
using NGame2.NUI;
using NGame2.NUI.NWindow.NCustomizing;
using NGame2.NUtil.NCostume;
using NShared;
using UnityEngine;

public static class Customizing_Extension_Method
{
	public static string GetCostumeLog(this CachedHeroInfo i_this)
	{
		if (i_this == null)
		{
			return string.Empty;
		}
		return string.Concat(string.Format(" C ({0}) /Hair ({9}) /Hide({8}) /W ({1}) /A1 ({2}) /A2 ({3}) /A3 ({4}) /A4 ({5}) /A5 ({6}) /A6 ({7}) ", i_this.CostumeIndex, i_this.WeaponCostumeIndex, i_this.AccessoryCostumeIndex1, i_this.AccessoryCostumeIndex2, i_this.AccessoryCostumeIndex3, i_this.AccessoryCostumeIndex4, i_this.AccessoryCostumeIndex5, i_this.AccessoryCostumeIndex6, i_this.HideUniqueWeapon, i_this.HairCostumeIndex) + "\n", $"Pos1 ({i_this.AccessoryPositionInfo1.LOG()}) /Pos2 ({i_this.AccessoryPositionInfo2.LOG()}) /Pos3 ({i_this.AccessoryPositionInfo3.LOG()}) /Pos4 ({i_this.AccessoryPositionInfo4.LOG()}) /Pos5 ({i_this.AccessoryPositionInfo5.LOG()}) /Pos6 ({i_this.AccessoryPositionInfo6.LOG()}) /");
	}

	public static string LOG(this PositionInfo i_this)
	{
		if (i_this == null)
		{
			return string.Empty;
		}
		return i_this.Log();
	}

	public static CachedHeroInfoCostumeStock GetCostumeStock(this CachedHeroInfo i_this)
	{
		CachedHeroInfoCostumeStock result = default(CachedHeroInfoCostumeStock);
		if (i_this == null)
		{
			return result;
		}
		result.HeroIndex = i_this.HeroIndex;
		result.CostumeIndex = i_this.CostumeIndex;
		result.HairCostumeIndex = i_this.HairCostumeIndex;
		result.HideUniqueWeapon = i_this.HideUniqueWeapon;
		result.WeaponCostumeIndex = i_this.WeaponCostumeIndex;
		result.AccessoryCostumeIndex1 = i_this.AccessoryCostumeIndex1;
		result.AccessoryCostumeIndex2 = i_this.AccessoryCostumeIndex2;
		result.AccessoryCostumeIndex3 = i_this.AccessoryCostumeIndex3;
		result.AccessoryCostumeIndex4 = i_this.AccessoryCostumeIndex4;
		result.AccessoryCostumeIndex5 = i_this.AccessoryCostumeIndex5;
		result.AccessoryCostumeIndex6 = i_this.AccessoryCostumeIndex6;
		result.AccessoryPositionInfo1 = i_this.AccessoryPositionInfo1;
		result.AccessoryPositionInfo2 = i_this.AccessoryPositionInfo2;
		result.AccessoryPositionInfo3 = i_this.AccessoryPositionInfo3;
		result.AccessoryPositionInfo4 = i_this.AccessoryPositionInfo4;
		result.AccessoryPositionInfo5 = i_this.AccessoryPositionInfo5;
		result.AccessoryPositionInfo6 = i_this.AccessoryPositionInfo6;
		return result;
	}

	public static bool IsEqualCostumeStock(this CachedHeroInfo i_this, CachedHeroInfoCostumeStock costumeStock)
	{
		if (i_this == null)
		{
			return false;
		}
		if (i_this.HeroIndex != costumeStock.HeroIndex)
		{
			return false;
		}
		if (i_this.CostumeIndex != costumeStock.CostumeIndex)
		{
			return false;
		}
		if (i_this.HideUniqueWeapon != costumeStock.HideUniqueWeapon)
		{
			return false;
		}
		if (i_this.WeaponCostumeIndex != costumeStock.WeaponCostumeIndex)
		{
			return false;
		}
		if (i_this.HairCostumeIndex != costumeStock.HairCostumeIndex)
		{
			return false;
		}
		if (i_this.AccessoryCostumeIndex1 != costumeStock.AccessoryCostumeIndex1)
		{
			return false;
		}
		if (i_this.AccessoryCostumeIndex2 != costumeStock.AccessoryCostumeIndex2)
		{
			return false;
		}
		if (i_this.AccessoryCostumeIndex3 != costumeStock.AccessoryCostumeIndex3)
		{
			return false;
		}
		if (i_this.AccessoryCostumeIndex4 != costumeStock.AccessoryCostumeIndex4)
		{
			return false;
		}
		if (i_this.AccessoryCostumeIndex5 != costumeStock.AccessoryCostumeIndex5)
		{
			return false;
		}
		if (i_this.AccessoryCostumeIndex6 != costumeStock.AccessoryCostumeIndex6)
		{
			return false;
		}
		if (!i_this.AccessoryPositionInfo1.Approximately(costumeStock.AccessoryPositionInfo1))
		{
			return false;
		}
		if (!i_this.AccessoryPositionInfo2.Approximately(costumeStock.AccessoryPositionInfo2))
		{
			return false;
		}
		if (!i_this.AccessoryPositionInfo3.Approximately(costumeStock.AccessoryPositionInfo3))
		{
			return false;
		}
		if (!i_this.AccessoryPositionInfo4.Approximately(costumeStock.AccessoryPositionInfo4))
		{
			return false;
		}
		if (!i_this.AccessoryPositionInfo5.Approximately(costumeStock.AccessoryPositionInfo5))
		{
			return false;
		}
		if (!i_this.AccessoryPositionInfo6.Approximately(costumeStock.AccessoryPositionInfo6))
		{
			return false;
		}
		return true;
	}

	public static bool IsEqualCostumeInfo(this CachedHeroInfo i_this, CachedHeroInfo rhs)
	{
		if (i_this == rhs)
		{
			return SprkAccessories.Equal(equal: true, i_this, rhs);
		}
		if (i_this == null)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (rhs == null)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.HeroIndex != rhs.HeroIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.CostumeIndex != rhs.CostumeIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.HideUniqueWeapon != rhs.HideUniqueWeapon)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.WeaponCostumeIndex != rhs.WeaponCostumeIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.HairCostumeIndex != rhs.HairCostumeIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex1 != rhs.AccessoryCostumeIndex1)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex2 != rhs.AccessoryCostumeIndex2)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex3 != rhs.AccessoryCostumeIndex3)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex4 != rhs.AccessoryCostumeIndex4)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex5 != rhs.AccessoryCostumeIndex5)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex6 != rhs.AccessoryCostumeIndex6)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex1 != 0 && !i_this.AccessoryPositionInfo1.Approximately(rhs.AccessoryPositionInfo1))
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex2 != 0 && !i_this.AccessoryPositionInfo2.Approximately(rhs.AccessoryPositionInfo2))
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex3 != 0 && !i_this.AccessoryPositionInfo3.Approximately(rhs.AccessoryPositionInfo3))
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex4 != 0 && !i_this.AccessoryPositionInfo4.Approximately(rhs.AccessoryPositionInfo4))
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex5 != 0 && !i_this.AccessoryPositionInfo5.Approximately(rhs.AccessoryPositionInfo5))
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex6 != 0 && !i_this.AccessoryPositionInfo6.Approximately(rhs.AccessoryPositionInfo6))
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		return SprkAccessories.Equal(equal: true, i_this, rhs);
	}

	public static void SetCostumeNameList(this CachedHeroInfo i_this, List<string> costumeNameList)
	{
		if (i_this == null)
		{
			return;
		}
		if (i_this.CostumeIndex != 0)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(i_this.CostumeIndex);
			if (data != null)
			{
				costumeNameList.Add(data.CostumeName);
			}
		}
		if (i_this.HairCostumeIndex != 0)
		{
			HairCostumeData data2 = GameRoot.Instance.HairCostumeDataContainer.GetData(i_this.HairCostumeIndex);
			if (data2 != null)
			{
				costumeNameList.Add(data2.Name);
			}
		}
		if (i_this.WeaponCostumeIndex != 0)
		{
			WeaponCostumeData data3 = GameRoot.Instance.WeaponCostumeDataContainer.GetData(i_this.WeaponCostumeIndex);
			if (data3 != null)
			{
				costumeNameList.Add(data3.Name);
			}
		}
		if (i_this.AccessoryCostumeIndex1 != 0)
		{
			AccessoryCostumeData data4 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(i_this.AccessoryCostumeIndex1);
			if (data4 != null)
			{
				costumeNameList.Add(data4.Name);
			}
		}
		if (i_this.AccessoryCostumeIndex2 != 0)
		{
			AccessoryCostumeData data5 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(i_this.AccessoryCostumeIndex2);
			if (data5 != null)
			{
				costumeNameList.Add(data5.Name);
			}
		}
		if (i_this.AccessoryCostumeIndex3 != 0)
		{
			AccessoryCostumeData data6 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(i_this.AccessoryCostumeIndex3);
			if (data6 != null)
			{
				costumeNameList.Add(data6.Name);
			}
		}
		if (i_this.AccessoryCostumeIndex4 != 0)
		{
			AccessoryCostumeData data7 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(i_this.AccessoryCostumeIndex4);
			if (data7 != null)
			{
				costumeNameList.Add(data7.Name);
			}
		}
		if (i_this.AccessoryCostumeIndex5 != 0)
		{
			AccessoryCostumeData data8 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(i_this.AccessoryCostumeIndex5);
			if (data8 != null)
			{
				costumeNameList.Add(data8.Name);
			}
		}
		if (i_this.AccessoryCostumeIndex6 != 0)
		{
			AccessoryCostumeData data9 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(i_this.AccessoryCostumeIndex6);
			if (data9 != null)
			{
				costumeNameList.Add(data9.Name);
			}
		}
	}

	public static CachedHeroInfo SetCostumeStorageSlotInfo(this CachedHeroInfo i_this, CostumeStorageSlotInfo slotInfo)
	{
		if (i_this == null)
		{
			SprkAccessories.Copy(null, slotInfo);
			return null;
		}
		if (slotInfo == null)
		{
			SprkAccessories.Copy(i_this, slotInfo);
			return i_this;
		}
		i_this.CostumeIndex = slotInfo.CostumeIndex;
		i_this.HairCostumeIndex = slotInfo.HairCostumeIndex;
		i_this.HideUniqueWeapon = slotInfo.HideUniqueWeapon;
		i_this.WeaponCostumeIndex = slotInfo.WeaponCostumeIndex;
		i_this.AccessoryCostumeIndex1 = slotInfo.AccessoryCostumeIndex1;
		i_this.AccessoryCostumeIndex2 = slotInfo.AccessoryCostumeIndex2;
		i_this.AccessoryCostumeIndex3 = slotInfo.AccessoryCostumeIndex3;
		i_this.AccessoryCostumeIndex4 = slotInfo.AccessoryCostumeIndex4;
		i_this.AccessoryCostumeIndex5 = slotInfo.AccessoryCostumeIndex5;
		i_this.AccessoryCostumeIndex6 = slotInfo.AccessoryCostumeIndex6;
		if (i_this.AccessoryCostumeIndex1 != 0)
		{
			i_this.AccessoryPositionInfo1 = PositionInfo.Create().TryAssign(slotInfo.PositionInfo1);
		}
		if (i_this.AccessoryCostumeIndex2 != 0)
		{
			i_this.AccessoryPositionInfo2 = PositionInfo.Create().TryAssign(slotInfo.PositionInfo2);
		}
		if (i_this.AccessoryCostumeIndex3 != 0)
		{
			i_this.AccessoryPositionInfo3 = PositionInfo.Create().TryAssign(slotInfo.PositionInfo3);
		}
		if (i_this.AccessoryCostumeIndex4 != 0)
		{
			i_this.AccessoryPositionInfo4 = PositionInfo.Create().TryAssign(slotInfo.PositionInfo4);
		}
		if (i_this.AccessoryCostumeIndex5 != 0)
		{
			i_this.AccessoryPositionInfo5 = PositionInfo.Create().TryAssign(slotInfo.PositionInfo5);
		}
		if (i_this.AccessoryCostumeIndex6 != 0)
		{
			i_this.AccessoryPositionInfo6 = PositionInfo.Create().TryAssign(slotInfo.PositionInfo6);
		}
		SprkAccessories.Copy(i_this, slotInfo);
		return i_this;
	}

	public static CachedHeroInfo CreateCachedHeroInfo(this CostumeStorageSlotInfo i_this)
	{
		if (i_this == null)
		{
			SprkAccessories.Copy(null, i_this);
			return null;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(i_this.HeroIndex);
		if (data == null)
		{
			SprkAccessories.Copy(null, i_this);
			return null;
		}
		CachedHeroInfo cachedHeroInfo = new CachedHeroInfo(data);
		cachedHeroInfo.CostumeIndex = i_this.CostumeIndex;
		cachedHeroInfo.HairCostumeIndex = i_this.HairCostumeIndex;
		cachedHeroInfo.HideUniqueWeapon = i_this.HideUniqueWeapon;
		cachedHeroInfo.WeaponCostumeIndex = i_this.WeaponCostumeIndex;
		cachedHeroInfo.AccessoryCostumeIndex1 = i_this.AccessoryCostumeIndex1;
		cachedHeroInfo.AccessoryCostumeIndex2 = i_this.AccessoryCostumeIndex2;
		cachedHeroInfo.AccessoryCostumeIndex3 = i_this.AccessoryCostumeIndex3;
		cachedHeroInfo.AccessoryCostumeIndex4 = i_this.AccessoryCostumeIndex4;
		cachedHeroInfo.AccessoryCostumeIndex5 = i_this.AccessoryCostumeIndex5;
		cachedHeroInfo.AccessoryCostumeIndex6 = i_this.AccessoryCostumeIndex6;
		if (cachedHeroInfo.AccessoryCostumeIndex1 != 0)
		{
			cachedHeroInfo.AccessoryPositionInfo1 = PositionInfo.Create().TryAssign(i_this.PositionInfo1);
		}
		if (cachedHeroInfo.AccessoryCostumeIndex2 != 0)
		{
			cachedHeroInfo.AccessoryPositionInfo2 = PositionInfo.Create().TryAssign(i_this.PositionInfo2);
		}
		if (cachedHeroInfo.AccessoryCostumeIndex3 != 0)
		{
			cachedHeroInfo.AccessoryPositionInfo3 = PositionInfo.Create().TryAssign(i_this.PositionInfo3);
		}
		if (cachedHeroInfo.AccessoryCostumeIndex4 != 0)
		{
			cachedHeroInfo.AccessoryPositionInfo4 = PositionInfo.Create().TryAssign(i_this.PositionInfo4);
		}
		if (cachedHeroInfo.AccessoryCostumeIndex5 != 0)
		{
			cachedHeroInfo.AccessoryPositionInfo5 = PositionInfo.Create().TryAssign(i_this.PositionInfo5);
		}
		if (cachedHeroInfo.AccessoryCostumeIndex6 != 0)
		{
			cachedHeroInfo.AccessoryPositionInfo6 = PositionInfo.Create().TryAssign(i_this.PositionInfo6);
		}
		SprkAccessories.Copy(cachedHeroInfo, i_this);
		return cachedHeroInfo;
	}

	public static PlayerAvatarHeroInfo CreatePlayerAvatarHeroInfo(this CachedUserInfo i_this)
	{
		if (i_this == null)
		{
			return null;
		}
		return new PlayerAvatarHeroInfo
		{
			HeroIndex = i_this.HeroIndex,
			CostumeIndex = i_this.CostumeIndex,
			HideUniqueWeapon = i_this.HideUniqueWeapon,
			WeaponCostumeIndex = i_this.WeaponCostumeIndex,
			HairCostumeIndex = i_this.HairCostumeIndex,
			AccessoryCostumeIndex1 = i_this.AccessoryCostumeIndex1,
			PositionInfo1 = i_this.PositionInfo1,
			AccessoryCostumeIndex2 = i_this.AccessoryCostumeIndex2,
			PositionInfo2 = i_this.PositionInfo2,
			AccessoryCostumeIndex3 = i_this.AccessoryCostumeIndex3,
			PositionInfo3 = i_this.PositionInfo3,
			AccessoryCostumeIndex4 = i_this.AccessoryCostumeIndex4,
			PositionInfo4 = i_this.PositionInfo4,
			AccessoryCostumeIndex5 = i_this.AccessoryCostumeIndex5,
			PositionInfo5 = i_this.PositionInfo5,
			AccessoryCostumeIndex6 = i_this.AccessoryCostumeIndex6,
			PositionInfo6 = i_this.PositionInfo6
		};
	}

	public static bool IsEqualCostumeInfo(this CachedHeroInfo i_this, CostumeStorageSlotInfo slotInfo)
	{
		if (i_this == null && slotInfo == null)
		{
			return SprkAccessories.Equal(equal: true, i_this, slotInfo);
		}
		if (i_this == null)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (slotInfo == null)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.HeroIndex != slotInfo.HeroIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.CostumeIndex != slotInfo.CostumeIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.HideUniqueWeapon != slotInfo.HideUniqueWeapon)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.WeaponCostumeIndex != slotInfo.WeaponCostumeIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.HairCostumeIndex != slotInfo.HairCostumeIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.AccessoryCostumeIndex1 != slotInfo.AccessoryCostumeIndex1)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.AccessoryCostumeIndex2 != slotInfo.AccessoryCostumeIndex2)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.AccessoryCostumeIndex3 != slotInfo.AccessoryCostumeIndex3)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.AccessoryCostumeIndex4 != slotInfo.AccessoryCostumeIndex4)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.AccessoryCostumeIndex5 != slotInfo.AccessoryCostumeIndex5)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.AccessoryCostumeIndex6 != slotInfo.AccessoryCostumeIndex6)
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		PositionInfo rhs = (String_Extension_Method.IsNullOrEmpty(slotInfo.PositionInfo1) ? null : PositionInfo.Create().TryAssign(slotInfo.PositionInfo1));
		PositionInfo rhs2 = (String_Extension_Method.IsNullOrEmpty(slotInfo.PositionInfo2) ? null : PositionInfo.Create().TryAssign(slotInfo.PositionInfo2));
		PositionInfo rhs3 = (String_Extension_Method.IsNullOrEmpty(slotInfo.PositionInfo3) ? null : PositionInfo.Create().TryAssign(slotInfo.PositionInfo3));
		PositionInfo rhs4 = (String_Extension_Method.IsNullOrEmpty(slotInfo.PositionInfo4) ? null : PositionInfo.Create().TryAssign(slotInfo.PositionInfo4));
		if (i_this.AccessoryCostumeIndex1 != 0 && !i_this.AccessoryPositionInfo1.Approximately(rhs))
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.AccessoryCostumeIndex2 != 0 && !i_this.AccessoryPositionInfo2.Approximately(rhs2))
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.AccessoryCostumeIndex3 != 0 && !i_this.AccessoryPositionInfo3.Approximately(rhs3))
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.AccessoryCostumeIndex4 != 0 && !i_this.AccessoryPositionInfo4.Approximately(rhs4))
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.AccessoryCostumeIndex5 != 0 && !i_this.AccessoryPositionInfo5.Approximately(rhs3))
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		if (i_this.AccessoryCostumeIndex6 != 0 && !i_this.AccessoryPositionInfo6.Approximately(rhs4))
		{
			return SprkAccessories.Equal(equal: false, i_this, slotInfo);
		}
		return SprkAccessories.Equal(equal: true, i_this, slotInfo);
	}

	public static bool IsEqual(this PlayerAvatarHeroInfo i_this, PlayerAvatarHeroInfo rhs)
	{
		if (i_this == rhs)
		{
			return SprkAccessories.Equal(equal: true, i_this, rhs);
		}
		if (i_this == null || rhs == null)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.HeroIndex != rhs.HeroIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.CostumeIndex != rhs.CostumeIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.HideUniqueWeapon != rhs.HideUniqueWeapon)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.WeaponCostumeIndex != rhs.WeaponCostumeIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.HairCostumeIndex != rhs.HairCostumeIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex1 != rhs.AccessoryCostumeIndex1)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex2 != rhs.AccessoryCostumeIndex2)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex3 != rhs.AccessoryCostumeIndex3)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.AccessoryCostumeIndex4 != rhs.AccessoryCostumeIndex4)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.PositionInfo1 != rhs.PositionInfo1)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.PositionInfo2 != rhs.PositionInfo2)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.PositionInfo3 != rhs.PositionInfo3)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		if (i_this.PositionInfo4 != rhs.PositionInfo4)
		{
			return SprkAccessories.Equal(equal: false, i_this, rhs);
		}
		return SprkAccessories.Equal(equal: true, i_this, rhs);
	}

	public static bool IsEqualCostumeInfo(this CachedHeroInfo i_this, PlayerAvatarHeroInfo playerAvatarHeroInfo)
	{
		if (i_this == null && playerAvatarHeroInfo == null)
		{
			return SprkAccessories.Equal(equal: true, i_this, playerAvatarHeroInfo);
		}
		if (i_this == null)
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (playerAvatarHeroInfo == null)
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (i_this.HeroIndex != playerAvatarHeroInfo.HeroIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (i_this.CostumeIndex != playerAvatarHeroInfo.CostumeIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (i_this.HideUniqueWeapon != playerAvatarHeroInfo.HideUniqueWeapon)
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (i_this.WeaponCostumeIndex != playerAvatarHeroInfo.WeaponCostumeIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (i_this.HairCostumeIndex != playerAvatarHeroInfo.HairCostumeIndex)
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (i_this.AccessoryCostumeIndex1 != playerAvatarHeroInfo.AccessoryCostumeIndex1)
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (i_this.AccessoryCostumeIndex2 != playerAvatarHeroInfo.AccessoryCostumeIndex2)
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (i_this.AccessoryCostumeIndex3 != playerAvatarHeroInfo.AccessoryCostumeIndex3)
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (i_this.AccessoryCostumeIndex4 != playerAvatarHeroInfo.AccessoryCostumeIndex4)
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		PositionInfo rhs = (String_Extension_Method.IsNullOrEmpty(playerAvatarHeroInfo.PositionInfo1) ? null : PositionInfo.Create().TryAssign(playerAvatarHeroInfo.PositionInfo1));
		PositionInfo rhs2 = (String_Extension_Method.IsNullOrEmpty(playerAvatarHeroInfo.PositionInfo2) ? null : PositionInfo.Create().TryAssign(playerAvatarHeroInfo.PositionInfo2));
		PositionInfo rhs3 = (String_Extension_Method.IsNullOrEmpty(playerAvatarHeroInfo.PositionInfo3) ? null : PositionInfo.Create().TryAssign(playerAvatarHeroInfo.PositionInfo3));
		PositionInfo rhs4 = (String_Extension_Method.IsNullOrEmpty(playerAvatarHeroInfo.PositionInfo4) ? null : PositionInfo.Create().TryAssign(playerAvatarHeroInfo.PositionInfo4));
		if (i_this.AccessoryCostumeIndex1 != 0 && !i_this.AccessoryPositionInfo1.Approximately(rhs))
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (i_this.AccessoryCostumeIndex2 != 0 && !i_this.AccessoryPositionInfo2.Approximately(rhs2))
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (i_this.AccessoryCostumeIndex3 != 0 && !i_this.AccessoryPositionInfo3.Approximately(rhs3))
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		if (i_this.AccessoryCostumeIndex4 != 0 && !i_this.AccessoryPositionInfo4.Approximately(rhs4))
		{
			return SprkAccessories.Equal(equal: false, i_this, playerAvatarHeroInfo);
		}
		return SprkAccessories.Equal(equal: true, i_this, playerAvatarHeroInfo);
	}

	public static CachedHeroInfo CreatePlayerAvatarHeroInfo(this CachedHeroInfo i_this, PlayerAvatarHeroInfo playerAvatarHeroInfo)
	{
		if (i_this == null)
		{
			SprkAccessories.Copy(null, playerAvatarHeroInfo);
			return null;
		}
		CachedHeroInfo cachedHeroInfo = JsonMarshaler.Clone(i_this);
		if (playerAvatarHeroInfo == null)
		{
			SprkAccessories.Copy(cachedHeroInfo, playerAvatarHeroInfo);
			return cachedHeroInfo;
		}
		if (playerAvatarHeroInfo.HeroIndex != i_this.HeroIndex)
		{
			SprkAccessories.Copy(cachedHeroInfo, playerAvatarHeroInfo);
			return cachedHeroInfo;
		}
		cachedHeroInfo.CostumeIndex = playerAvatarHeroInfo.CostumeIndex;
		cachedHeroInfo.HideUniqueWeapon = playerAvatarHeroInfo.HideUniqueWeapon;
		cachedHeroInfo.WeaponCostumeIndex = playerAvatarHeroInfo.WeaponCostumeIndex;
		cachedHeroInfo.HairCostumeIndex = playerAvatarHeroInfo.HairCostumeIndex;
		cachedHeroInfo.AccessoryCostumeIndex1 = playerAvatarHeroInfo.AccessoryCostumeIndex1;
		cachedHeroInfo.AccessoryCostumeIndex2 = playerAvatarHeroInfo.AccessoryCostumeIndex2;
		cachedHeroInfo.AccessoryCostumeIndex3 = playerAvatarHeroInfo.AccessoryCostumeIndex3;
		cachedHeroInfo.AccessoryCostumeIndex4 = playerAvatarHeroInfo.AccessoryCostumeIndex4;
		cachedHeroInfo.AccessoryPositionInfo1 = (String_Extension_Method.IsNullOrEmpty(playerAvatarHeroInfo.PositionInfo1) ? null : PositionInfo.Create().TryAssign(playerAvatarHeroInfo.PositionInfo1));
		cachedHeroInfo.AccessoryPositionInfo2 = (String_Extension_Method.IsNullOrEmpty(playerAvatarHeroInfo.PositionInfo2) ? null : PositionInfo.Create().TryAssign(playerAvatarHeroInfo.PositionInfo2));
		cachedHeroInfo.AccessoryPositionInfo3 = (String_Extension_Method.IsNullOrEmpty(playerAvatarHeroInfo.PositionInfo3) ? null : PositionInfo.Create().TryAssign(playerAvatarHeroInfo.PositionInfo3));
		cachedHeroInfo.AccessoryPositionInfo4 = (String_Extension_Method.IsNullOrEmpty(playerAvatarHeroInfo.PositionInfo4) ? null : PositionInfo.Create().TryAssign(playerAvatarHeroInfo.PositionInfo4));
		SprkAccessories.Copy(cachedHeroInfo, playerAvatarHeroInfo);
		return cachedHeroInfo;
	}

	public static AccessoryPartType GetAccessoryType(this ToggleType i_this)
	{
		return EnumAttributeContainer<ToggleType, ToggleTypeAttribute>.Get(i_this)?.accessoryPartType ?? AccessoryPartType.None;
	}

	public static AccessoryPartType GetCustomizingCameraType(this ToggleType i_this)
	{
		return EnumAttributeContainer<ToggleType, ToggleTypeAttribute>.Get(i_this)?.customizingCameraSettingType ?? AccessoryPartType.None;
	}

	public static void SetSlider(this UISlider i_this, float min, float max, float value)
	{
		if (!(i_this == null))
		{
			bool flag = Mathf.Approximately(min, max);
			UIHelper.SetWidgetGrayScale(i_this.GetComponent<UIWidget>(), flag, setColliders: true);
			if (flag)
			{
				i_this.value = 0.5f;
				return;
			}
			float value2 = Mathf.InverseLerp(min, max, value);
			i_this.value = value2;
		}
	}

	public static bool Approximately(this PositionInfo i_this, PositionInfo rhs)
	{
		if (i_this == null && rhs == null)
		{
			return true;
		}
		if (i_this == null)
		{
			return false;
		}
		if (rhs == null)
		{
			return false;
		}
		if (!Approximately(new Vector3(i_this.PositionX, i_this.PositionY, i_this.PositionZ), new Vector3(rhs.PositionX, rhs.PositionY, rhs.PositionZ)))
		{
			return false;
		}
		if (!Approximately(new Vector3(i_this.RotationX, i_this.RotationY, i_this.RotationZ), new Vector3(rhs.RotationX, rhs.RotationY, rhs.RotationZ)))
		{
			return false;
		}
		if (!Approximately(i_this.Scale, rhs.Scale))
		{
			return false;
		}
		return true;
	}

	public static bool Approximately(this float i_this, float rhs)
	{
		return Mathf.Approximately(i_this, rhs);
	}

	public static bool Approximately(this Vector3 i_this, Vector3 rhs)
	{
		if (!Mathf.Approximately(i_this.x, rhs.x))
		{
			return false;
		}
		if (!Mathf.Approximately(i_this.y, rhs.y))
		{
			return false;
		}
		if (!Mathf.Approximately(i_this.z, rhs.z))
		{
			return false;
		}
		return true;
	}

	public static PositionInfo TryAssign(this PositionInfo i_this, string serializedPositionInfo)
	{
		if (i_this == null || serializedPositionInfo == null)
		{
			return i_this;
		}
		try
		{
			if (JsonMarshaler.Parse(typeof(PositionInfo), serializedPositionInfo) is PositionInfo i_this2)
			{
				i_this.SetPosition(i_this2.GetPosition());
				i_this.SetRotation(i_this2.GetRotation());
				i_this.SetScale(i_this2.GetScale());
			}
		}
		catch (Exception)
		{
			return i_this;
		}
		return i_this;
	}

	public static void SetValidValue(this PositionInfo i_this, Vector3 postionVariation, Vector3 rotationVariation, float scaleVariation)
	{
		if (i_this != null)
		{
			Vector3 vector = -1f * postionVariation;
			Vector3 vector2 = postionVariation;
			Vector3 vector3 = -1f * rotationVariation;
			Vector3 vector4 = rotationVariation;
			float num = 1f - scaleVariation;
			float max = 1f + scaleVariation;
			if (num <= 0f)
			{
				num = 0.1f;
			}
			Vector3 position = i_this.GetPosition();
			Vector3 rotation = i_this.GetRotation();
			float scale = i_this.GetScale();
			Vector3 position2 = new Vector3(Mathf.Clamp(position.x, vector.x, vector2.x), Mathf.Clamp(position.y, vector.y, vector2.y), Mathf.Clamp(position.z, vector.z, vector2.z));
			Vector3 rotation2 = new Vector3(Mathf.Clamp(rotation.x, vector3.x, vector4.x), Mathf.Clamp(rotation.y, vector3.y, vector4.y), Mathf.Clamp(rotation.z, vector3.z, vector4.z));
			float scale2 = Mathf.Clamp(scale, num, max);
			i_this.SetPosition(position2);
			i_this.SetRotation(rotation2);
			i_this.SetScale(scale2);
		}
	}

	public static Vector3 GetPosition(this PositionInfo i_this)
	{
		if (i_this == null)
		{
			return Vector3.zero;
		}
		return new Vector3(i_this.PositionX, i_this.PositionY, i_this.PositionZ);
	}

	public static void SetPosition(this PositionInfo i_this, Vector3 position)
	{
		if (i_this != null)
		{
			i_this.PositionX = position.x;
			i_this.PositionY = position.y;
			i_this.PositionZ = position.z;
		}
	}

	public static Vector3 GetRotation(this PositionInfo i_this)
	{
		if (i_this == null)
		{
			return Vector3.zero;
		}
		return new Vector3(i_this.RotationX, i_this.RotationY, i_this.RotationZ);
	}

	public static void SetRotation(this PositionInfo i_this, Vector3 rotation)
	{
		if (i_this != null)
		{
			i_this.RotationX = rotation.x;
			i_this.RotationY = rotation.y;
			i_this.RotationZ = rotation.z;
		}
	}

	public static float GetScale(this PositionInfo i_this)
	{
		return i_this?.Scale ?? 1f;
	}

	public static void SetScale(this PositionInfo i_this, float scale)
	{
		if (i_this != null)
		{
			i_this.Scale = scale;
		}
	}

	public static ClientCustomizingPivotData GetCustomizingPivotData(this AccessoryCostumeData i_this, int heroIndex)
	{
		if (i_this == null)
		{
			return null;
		}
		return GameRoot.Instance.ClientCustomizingPivotDataContainer.GetData(heroIndex, i_this.PivotType);
	}

	public static Vector3 GetInitPosition(this ClientCustomizingPivotData i_this)
	{
		if (i_this == null)
		{
			return Vector3.zero;
		}
		if (i_this.InitPosition == null)
		{
			return Vector3.zero;
		}
		float[] initPosition = i_this.InitPosition;
		float x = ((__Extension_Method_ICollection__.COUNT(initPosition) < 1) ? 0f : initPosition[0]);
		float y = ((__Extension_Method_ICollection__.COUNT(initPosition) < 2) ? 0f : initPosition[1]);
		float z = ((__Extension_Method_ICollection__.COUNT(initPosition) < 3) ? 0f : initPosition[2]);
		return new Vector3(x, y, z);
	}

	public static Vector3 GetInitRotation(this ClientCustomizingPivotData i_this)
	{
		if (i_this == null)
		{
			return Vector3.zero;
		}
		if (i_this.InitRotation == null)
		{
			return Vector3.zero;
		}
		float[] initRotation = i_this.InitRotation;
		float x = ((__Extension_Method_ICollection__.COUNT(initRotation) < 1) ? 0f : initRotation[0]);
		float y = ((__Extension_Method_ICollection__.COUNT(initRotation) < 2) ? 0f : initRotation[1]);
		float z = ((__Extension_Method_ICollection__.COUNT(initRotation) < 3) ? 0f : initRotation[2]);
		return new Vector3(x, y, z);
	}

	public static Vector3 GetPositionVariation(this ClientCustomizingPivotData i_this)
	{
		if (i_this == null)
		{
			return Vector3.zero;
		}
		if (i_this.PositionVariation == null)
		{
			return Vector3.zero;
		}
		float[] positionVariation = i_this.PositionVariation;
		float x = ((__Extension_Method_ICollection__.COUNT(positionVariation) < 1) ? 0f : positionVariation[0]);
		float y = ((__Extension_Method_ICollection__.COUNT(positionVariation) < 2) ? 0f : positionVariation[1]);
		float z = ((__Extension_Method_ICollection__.COUNT(positionVariation) < 3) ? 0f : positionVariation[2]);
		return new Vector3(x, y, z);
	}

	public static Vector3 GetRoationVariation(this ClientCustomizingPivotData i_this)
	{
		if (i_this == null)
		{
			return Vector3.zero;
		}
		if (i_this.RotationVariation == null)
		{
			return Vector3.zero;
		}
		float[] rotationVariation = i_this.RotationVariation;
		float x = ((__Extension_Method_ICollection__.COUNT(rotationVariation) < 1) ? 0f : rotationVariation[0]);
		float y = ((__Extension_Method_ICollection__.COUNT(rotationVariation) < 2) ? 0f : rotationVariation[1]);
		float z = ((__Extension_Method_ICollection__.COUNT(rotationVariation) < 3) ? 0f : rotationVariation[2]);
		return new Vector3(x, y, z);
	}
}
