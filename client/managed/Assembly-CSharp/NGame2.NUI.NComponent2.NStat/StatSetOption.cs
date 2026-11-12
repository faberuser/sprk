using NShared;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NStat;

public class StatSetOption : MonoBehaviour
{
	public StatSetOptionToggle ToggleWeapon;

	public StatSetOptionToggle ToggleArmor;

	public StatSetOptionToggle ToggleSubGear;

	public StatSetOptionToggle ToggleAccessory;

	public StatSetOptionToggle ToggleOrb;

	public void Init()
	{
		ToggleWeapon.On(isOn: false);
		ToggleArmor.On(isOn: false);
		ToggleSubGear.On(isOn: false);
		ToggleAccessory.On(isOn: false);
		ToggleOrb.On(isOn: false);
	}

	public void Toggle(EquipPartType partType, bool isOn)
	{
		switch (partType)
		{
		case EquipPartType.Weapon:
			ToggleWeapon.On(isOn);
			break;
		case EquipPartType.Armor:
			ToggleArmor.On(isOn);
			break;
		case EquipPartType.SecondGear:
			ToggleSubGear.On(isOn);
			break;
		case EquipPartType.Accessory:
			ToggleAccessory.On(isOn);
			break;
		case EquipPartType.Orb:
			ToggleOrb.On(isOn);
			break;
		}
	}
}
