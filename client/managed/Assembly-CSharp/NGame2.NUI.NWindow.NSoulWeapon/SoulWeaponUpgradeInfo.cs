using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class SoulWeaponUpgradeInfo
{
	public interface IListener
	{
		void UpgradeSoulWeapon(EquipItemInfo equipItemInfo);
	}

	public SoulWeaponGradeInfo SoulWeaponGradeInfo;

	public SoulWeaponGradeInfo UpgradeSoulWeaponGradeInfo;

	public SoulWeaponGradeInfo MaxSoulWeaponGradeInfo;

	public SoulWeaponStatInfo SoulWeaponStatInfo;

	public SoulWeaponStatInfo UpgardeSoulWeaponStatInfo;

	public SoulWeaponStatInfo MaxSoulWeaponStatInfo;

	public SoulWeaponSkillInfo SoulWeaponSkillInfo;

	public SoulWeaponSkillInfo MaxSoulWeaponSkillInfo;

	public UILabel ReqEtherLabel;

	public UILabel ReqPriceLabel;

	public UIButton UpgradeButton;

	private SkillIconComponent _soulWeaponSkillIconComponent;

	public void Open(SoulWeaponInfo soulWeaponInfo)
	{
		Init(soulWeaponInfo);
	}

	public void Close()
	{
		FactoryBase.SafeReleaseComponent(ref _soulWeaponSkillIconComponent);
		SoulWeaponSkillInfo.Close();
	}

	private void Init(SoulWeaponInfo soulWeaponInfo)
	{
		SoulWeaponSkillInfo.Open(soulWeaponInfo);
		int grade = 0;
		int reinforceLevel = 0;
		if (soulWeaponInfo != null)
		{
			grade = soulWeaponInfo.Grade;
			reinforceLevel = soulWeaponInfo.ReinforceLevel;
		}
		SoulWeaponGradeInfo.Open(grade, reinforceLevel, isLocalize: true);
		SoulWeaponStatInfo.Open(soulWeaponInfo);
		MaxSoulWeaponGradeInfo.Open(grade, reinforceLevel, isLocalize: true);
		MaxSoulWeaponStatInfo.Open(soulWeaponInfo, isRenew: false, isTotal: true);
		MaxSoulWeaponSkillInfo.Open(soulWeaponInfo);
		SetNextSoulWeaponInfo(soulWeaponInfo);
		SetReqPrice(soulWeaponInfo);
	}

	private void SetNextSoulWeaponInfo(SoulWeaponInfo soulWeaponInfo)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager != null)
		{
			SoulWeaponInfo nextUpgradeSoulWeaponInfo = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.GetNextUpgradeSoulWeaponInfo(soulWeaponInfo);
			UpgradeSoulWeaponGradeInfo.Open(nextUpgradeSoulWeaponInfo?.Grade ?? 0, nextUpgradeSoulWeaponInfo?.ReinforceLevel ?? 0, isLocalize: true);
			UpgardeSoulWeaponStatInfo.Open(nextUpgradeSoulWeaponInfo);
		}
	}

	private void SetReqPrice(SoulWeaponInfo soulWeaponInfo)
	{
		if (ReqPriceLabel == null)
		{
			return;
		}
		if (soulWeaponInfo == null)
		{
			UIHelper.SetLabelText(ReqPriceLabel, "-");
			return;
		}
		SoulWeaponUpgradePriceData soulWeaponUpgradePriceData = soulWeaponInfo.GetSoulWeaponUpgradePriceData();
		if (soulWeaponUpgradePriceData == null)
		{
			UIHelper.SetLabelText(ReqPriceLabel, 0);
		}
		else
		{
			UIHelper.SetLabelText(ReqPriceLabel, soulWeaponUpgradePriceData.ReqGold);
		}
	}
}
