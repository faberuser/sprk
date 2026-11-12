using System;
using System.Collections.Generic;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUtil.NCostume;

public class HeroCostumeValidChecker
{
	public static void SetValidPositionInfo(int heroIndex, AccessoryPartPivotType PivotType, PositionInfo positionInfo)
	{
		if (positionInfo != null)
		{
			ClientCustomizingPivotData data = GameRoot.Instance.ClientCustomizingPivotDataContainer.GetData(heroIndex, PivotType);
			if (data != null)
			{
				positionInfo.SetValidValue(data.GetPositionVariation(), data.GetRoationVariation(), data.ScaleVariation);
			}
		}
	}

	public static void CheckPivotDataContainer()
	{
		new Dictionary<int, ClientCustomizingPivotData>();
		foreach (ClientCustomizingPivotData allDatum in GameRoot.Instance.ClientCustomizingPivotDataContainer.GetAllData())
		{
			ClientCustomizingPivotData clientCustomizingPivotData = allDatum;
			if (clientCustomizingPivotData.PresetHeroIndex != 0)
			{
				clientCustomizingPivotData = GameRoot.Instance.ClientCustomizingPivotDataContainer.GetData(clientCustomizingPivotData.HeroIndex, clientCustomizingPivotData.PivotType);
			}
			if (!CheckValid(clientCustomizingPivotData))
			{
				VespaDebug.LogError(string.Format("HeroIndex ({0})({4}) PivotData Invalid ({1}) ({2}) ({3})", allDatum.HeroIndex, allDatum.GetInitPosition(), allDatum.GetInitRotation(), allDatum.InitScale, allDatum.PivotType));
			}
		}
	}

	private static bool CheckValid(ClientCustomizingPivotData data)
	{
		Vector3 initPosition = data.GetInitPosition();
		int num = 1 & (IsValidPosition(initPosition) ? 1 : 0);
		Vector3 initRotation = data.GetInitRotation();
		int num2 = num & (IsValidRotation(initRotation) ? 1 : 0);
		float initScale = data.InitScale;
		return (byte)((uint)num2 & (IsValidScale(initScale) ? 1u : 0u)) != 0;
	}

	public static bool IsValidPosition(Vector3 positionVector)
	{
		if (positionVector.x > 1f)
		{
			return false;
		}
		if (positionVector.y > 1f)
		{
			return false;
		}
		if (positionVector.z > 1f)
		{
			return false;
		}
		return true;
	}

	public static bool IsValidRotation(Vector3 rotationVector)
	{
		if (rotationVector.x > 100f)
		{
			return false;
		}
		if (rotationVector.y > 100f)
		{
			return false;
		}
		if (rotationVector.z > 100f)
		{
			return false;
		}
		return true;
	}

	public static bool IsValidScale(float scale)
	{
		if (scale > 5f)
		{
			return false;
		}
		if (scale < 0.2f)
		{
			return false;
		}
		return true;
	}

	public static CheckResultInfo Check(CachedHeroInfo heroInfo)
	{
		CheckResultInfo result = new CheckResultInfo
		{
			HeroIndex = heroInfo.HeroIndex
		};
		try
		{
			result.CreatureName = heroInfo.CreatureData.Name;
		}
		catch (Exception)
		{
			if (GameRoot.Instance.CreatureDataContainer.GetData(result.HeroIndex) == null)
			{
				VespaDebug.LogError("data == null / " + heroInfo.HeroIndex);
			}
		}
		result.IsValid = true;
		result.InValidList = new List<string>();
		GameRoot instance = GameRoot.Instance;
		if (heroInfo.CostumeIndex != 0 && instance.CostumeDataContainer.GetData(heroInfo.CostumeIndex) == null)
		{
			string item = string.Format("{0} : ({1})", "CostumeData is null", heroInfo.CostumeIndex);
			result.InValidList.Add(item);
		}
		if (heroInfo.HairCostumeIndex != 0 && instance.HairCostumeDataContainer.GetData(heroInfo.HairCostumeIndex) == null)
		{
			string item2 = string.Format("{0} : ({1})", "HairCostumeData is null", heroInfo.HairCostumeIndex);
			result.InValidList.Add(item2);
		}
		if (heroInfo.WeaponCostumeIndex != 0)
		{
			if (instance.WeaponCostumeDataContainer.GetData(heroInfo.WeaponCostumeIndex) == null)
			{
				string item3 = string.Format("{0} : ({1})", "WeaponCostumeData is null", heroInfo.WeaponCostumeIndex);
				result.InValidList.Add(item3);
			}
			if (heroInfo.HideUniqueWeapon != 1)
			{
				string item4 = string.Format("{0} : ({1})", "HideUniqueWeapon != 1 ", heroInfo.HideUniqueWeapon);
				result.InValidList.Add(item4);
			}
		}
		List<Tuple<int, int, PositionInfo>> list = new List<Tuple<int, int, PositionInfo>>();
		new List<Tuple<int, PositionInfo>>();
		if (heroInfo.AccessoryCostumeIndex1 != 0)
		{
			list.Add(new Tuple<int, int, PositionInfo>(1, heroInfo.AccessoryCostumeIndex1, heroInfo.AccessoryPositionInfo1));
		}
		if (heroInfo.AccessoryCostumeIndex2 != 0)
		{
			list.Add(new Tuple<int, int, PositionInfo>(2, heroInfo.AccessoryCostumeIndex2, heroInfo.AccessoryPositionInfo2));
		}
		if (heroInfo.AccessoryCostumeIndex3 != 0)
		{
			list.Add(new Tuple<int, int, PositionInfo>(3, heroInfo.AccessoryCostumeIndex3, heroInfo.AccessoryPositionInfo3));
		}
		if (heroInfo.AccessoryCostumeIndex4 != 0)
		{
			list.Add(new Tuple<int, int, PositionInfo>(4, heroInfo.AccessoryCostumeIndex4, heroInfo.AccessoryPositionInfo4));
		}
		if (heroInfo.AccessoryCostumeIndex5 != 0)
		{
			list.Add(new Tuple<int, int, PositionInfo>(5, heroInfo.AccessoryCostumeIndex5, heroInfo.AccessoryPositionInfo5));
		}
		if (heroInfo.AccessoryCostumeIndex6 != 0)
		{
			list.Add(new Tuple<int, int, PositionInfo>(6, heroInfo.AccessoryCostumeIndex6, heroInfo.AccessoryPositionInfo6));
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			int heroIndex = heroInfo.HeroIndex;
			int item5 = list[i].Item1;
			int item6 = list[i].Item2;
			PositionInfo item7 = list[i].Item3;
			AccessoryCostumeData data = instance.AccessoryCostumeDataContainer.GetData(item6);
			if (data == null)
			{
				string item8 = string.Format("{0} : (accessoryIndex({1}) : accessoryCostumeIndex({2}))", "accessoryData is null", item5, item6);
				result.InValidList.Add(item8);
				continue;
			}
			AccessoryPartPivotType pivotType = data.PivotType;
			PositionInfo positionInfo = item7.Clone();
			SetValidPositionInfo(heroIndex, pivotType, item7);
			if (!item7.Approximately(positionInfo))
			{
				string item9 = string.Format("{0} : (accessoryIndex({1}) : b({3}) != a({2}))", "positionInfo Check Valid Fail", item5, item7.Log(), positionInfo.Log());
				result.InValidList.Add(item9);
			}
		}
		result.IsValid = __Extension_Method_ICollection__.COUNT(result.InValidList) == 0;
		return result;
	}
}
