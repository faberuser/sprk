using System.Collections.Generic;
using NGameAsset.NCreature;
using NShared;
using UnityEngine;

namespace NGameAsset;

public struct CreatureStock
{
	public enum ELoadLevel
	{
		Normal,
		Minimum,
		IncludeFacial,
		NodeNPC,
		PcAvatar
	}

	public string Code;

	public float Size;

	public int Star;

	public string BodyPrefix;

	public string HairPrefix;

	public int CostumeIndex;

	public WeaponStock WeaponStock;

	public ELoadLevel LoadLevel;

	public bool DontDestroyOnLoad;

	public HashSet<AnimationKeyData> PreloadSet;

	private Dictionary<AccessoryPartType, CustomizeStock> CustomizeStockDictionary;

	public string Prefix
	{
		set
		{
			BodyPrefix = value;
			WeaponStock.WeaponPrefix = value;
			HairPrefix = value;
		}
	}

	public void SetCustomizeStock(CustomizeStock stock)
	{
		SprkAccessories.SetStock(ref CustomizeStockDictionary, stock);
	}

	public Dictionary<AccessoryPartType, CustomizeStock>.ValueCollection CustomizeStockValues()
	{
		if (CustomizeStockDictionary == null)
		{
			return null;
		}
		return CustomizeStockDictionary.Values;
	}

	public void ClearCustomizeStock()
	{
		Vespa_Generic_ICollection_Extension_Method.CLEAR(CustomizeStockDictionary);
	}

	public int GetWeaponStarSafe()
	{
		if (WeaponStock.WeaponStar != 0)
		{
			return WeaponStock.WeaponStar;
		}
		return Star;
	}

	public bool NeedPreload(IAnimationData animationData)
	{
		if (PreloadSet == null)
		{
			return true;
		}
		AnimationKeyData key = animationData.GetKey();
		if (key.MotionType == DefineCreature.EMotion.Idle || key.MotionType == DefineCreature.EMotion.BattleIdle)
		{
			return true;
		}
		return PreloadSet.Contains(key);
	}

	public bool IsEquals(CreatureStock rhs)
	{
		if (Code != rhs.Code)
		{
			return false;
		}
		if (!Mathf.Approximately(Size, rhs.Size))
		{
			return false;
		}
		if (Star != rhs.Star)
		{
			return false;
		}
		if (BodyPrefix != rhs.BodyPrefix)
		{
			return false;
		}
		if (HairPrefix != rhs.HairPrefix)
		{
			return false;
		}
		if (WeaponStock.WeaponPrefix != rhs.WeaponStock.WeaponPrefix)
		{
			return false;
		}
		if (WeaponStock.WeaponStar != rhs.WeaponStock.WeaponStar)
		{
			return false;
		}
		if (WeaponStock.IsEquipSoulWeapon != rhs.WeaponStock.IsEquipSoulWeapon)
		{
			return false;
		}
		if (WeaponStock.SoulWeaponGrade != rhs.WeaponStock.SoulWeaponGrade)
		{
			return false;
		}
		if (LoadLevel != rhs.LoadLevel)
		{
			return false;
		}
		if (DontDestroyOnLoad != rhs.DontDestroyOnLoad)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(PreloadSet) != __Extension_Method_ICollection__.COUNT(rhs.PreloadSet))
		{
			return false;
		}
		if (PreloadSet != null && PreloadSet.Count > 0)
		{
			foreach (AnimationKeyData item in PreloadSet)
			{
				if (!rhs.PreloadSet.Contains(item))
				{
					return false;
				}
			}
		}
		if (__Extension_Method_ICollection__.COUNT(CustomizeStockDictionary) != __Extension_Method_ICollection__.COUNT(rhs.CustomizeStockDictionary))
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(CustomizeStockDictionary) > 0)
		{
			Dictionary<AccessoryPartType, CustomizeStock>.Enumerator enumerator2 = CustomizeStockDictionary.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				if (!rhs.CustomizeStockDictionary.ContainsKey(enumerator2.Current.Key))
				{
					return false;
				}
				CustomizeStock customizeStock = rhs.CustomizeStockDictionary[enumerator2.Current.Key];
				if (!customizeStock.IsEqualItem(enumerator2.Current.Value))
				{
					return false;
				}
				if (!customizeStock.IsEqualTransform(enumerator2.Current.Value))
				{
					return false;
				}
			}
		}
		return true;
	}
}
