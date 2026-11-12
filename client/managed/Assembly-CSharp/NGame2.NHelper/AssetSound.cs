using System.Collections.Generic;
using NGameAsset;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NHelper;

public static class AssetSound
{
	public static readonly string[] PreloadEquipSound = new string[12]
	{
		"Item.Equip.Sword", "Item.Equip.Dagger", "Item.Equip.Spear", "Item.Equip.Staff", "Item.Equip.Bow", "Item.Equip.Cannon", "Item.Equip.LightArmor", "Item.Equip.MediumArmor", "Item.Equip.HeavyArmor", "Item.Equip.ETC",
		"Item.Equip.Accessory", "Item.Equip.Rune"
	};

	public const string EquipSoundPrefix = "Item.Equip.";

	public static List<IAsyncLoad<AudioClip>> ReserveItemSound()
	{
		List<IAsyncLoad<AudioClip>> list = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(PreloadEquipSound); i++)
		{
			string code = PreloadEquipSound[i];
			list = SingletonAuto<SoundContainer>.instance.ReserveSound(code, list);
		}
		return list;
	}

	public static void PlayItemEquipSound(int itemIndex)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data != null && !String_Extension_Method.IsNullOrEmpty(data.AssetGroup))
		{
			SoundStock stock = new SoundStock
			{
				Code = "Item.Equip." + data.AssetGroup,
				LocalTimer = UnityLocalTimer.Default
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
		}
	}
}
