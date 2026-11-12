using System;
using NNguiExtension;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class SoulWeaponGradeInfo
{
	public UISpriteSelector SoulWeaponGradeSpriteSelector;

	public UILabel SoulWeaponGradeLabel;

	public void Open(int grade, int reinforceLevel, bool isLocalize, bool isDisable = false)
	{
		Init(grade, reinforceLevel, isLocalize, isDisable);
	}

	private void Init(int grade, int reinforceLevel, bool isLocalize, bool isDisable)
	{
		SetSoulWeaponGradeMark(grade, isDisable);
		SetSoulWeaponGrade(grade, reinforceLevel, isLocalize);
	}

	private void SetSoulWeaponGradeMark(int grade, bool isDiable)
	{
		if (SoulWeaponGradeSpriteSelector != null)
		{
			SoulWeaponGradeSpriteSelector.Select(isDiable ? (grade + "_Disable") : grade.ToString());
		}
	}

	private void SetSoulWeaponGrade(int grade, int reinforceLevel, bool isLocalize)
	{
		if (SoulWeaponGradeLabel != null)
		{
			string text = (isLocalize ? LocalizationManager.GetStringDirect("SOULWEAPON_INFO_UPGRADESTEP", grade) : ((reinforceLevel <= 0) ? "" : reinforceLevel.ToString()));
			UIHelper.SetLabelText(SoulWeaponGradeLabel, text);
		}
	}
}
