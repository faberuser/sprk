using System;
using NGame2.NAccount;
using NGame2.NUI.NDescription;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class SoulWeaponSkillInfo
{
	public UILabel SkillDescLabel;

	public UILabel SkillUseConditionLabel;

	public UIBundleLoader SkillIconBundleLoader;

	public UISpriteSelector StackSpriteSelector;

	public UILabel SkillCoolTimeUseCountLabel;

	private SoulWeaponInfo _soulWeaponInfo;

	private SoulWeaponSkillData SoulWeaponSkillData
	{
		get
		{
			if (_soulWeaponInfo == null)
			{
				return null;
			}
			return _soulWeaponInfo.GetSoulWeaponSkillData();
		}
	}

	private long SoulWeaponSkillCoolTime
	{
		get
		{
			SoulWeaponSkillData soulWeaponSkillData = SoulWeaponSkillData;
			if (soulWeaponSkillData == null)
			{
				return -1L;
			}
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(soulWeaponSkillData.BasicSkillIndex);
			if (data == null)
			{
				return -1L;
			}
			return data.CoolTimeMs / 1000;
		}
	}

	private long SoulWeaponSkillUseCount
	{
		get
		{
			SoulWeaponSkillData soulWeaponSkillData = SoulWeaponSkillData;
			if (soulWeaponSkillData == null)
			{
				return -1L;
			}
			return soulWeaponSkillData.MaxUseCount;
		}
	}

	public void Open(SoulWeaponInfo soulWeaponInfo)
	{
		init(soulWeaponInfo);
	}

	public void OpenUpgrade(SoulWeaponInfo soulWeaponInfo)
	{
		_soulWeaponInfo = soulWeaponInfo;
		SetSkillIcon();
		SetSkillUpgradeDesc();
	}

	private void init(SoulWeaponInfo soulWeaponInfo)
	{
		_soulWeaponInfo = soulWeaponInfo;
		SetSkillIcon();
		SetSkillDesc();
		SetSkillUseCondition();
		SetSkillCoolTimeUseCount();
	}

	public void Refresh()
	{
		SetSkillIcon();
		SetSkillDesc();
		SetSkillUseCondition();
		SetSkillCoolTimeUseCount();
	}

	public void Close()
	{
	}

	private void SetSkillIcon()
	{
		UIHelper.SetBundleTexture(SkillIconBundleLoader, "");
		int soulWeaponEquipHeroIndex = UIHelper.GetSoulWeaponEquipHeroIndex(_soulWeaponInfo);
		if (soulWeaponEquipHeroIndex != 0)
		{
			Helper.SkillBasicInfo soulWeaponBasicSkillInfo = Helper.GetSoulWeaponBasicSkillInfo(soulWeaponEquipHeroIndex, _soulWeaponInfo);
			UIHelper.SetBundleTexture(SkillIconBundleLoader, UIHelper.GetItemIconPath(soulWeaponBasicSkillInfo.ItemCode));
			SetSkillStatType(soulWeaponBasicSkillInfo.StackTypeName);
		}
	}

	private void SetSkillStatType(string stackTypeName)
	{
		if (StackSpriteSelector != null)
		{
			StackSpriteSelector.Select(stackTypeName);
		}
	}

	private void SetSkillUpgradeDesc()
	{
		UIHelper.SetLabelText(SkillDescLabel, "-");
		int soulWeaponEquipHeroIndex = UIHelper.GetSoulWeaponEquipHeroIndex(_soulWeaponInfo);
		if (soulWeaponEquipHeroIndex != 0)
		{
			int skillIndex = _soulWeaponInfo.GetBasicSkillIndex();
			if (_soulWeaponInfo.Grade > 0)
			{
				skillIndex = Vespa_Generic_IList_Extension_Method.GetLast(_soulWeaponInfo.GetAddSkillIndices());
			}
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(soulWeaponEquipHeroIndex);
			string byHeroInfo = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(skillIndex, heroInfo);
			UIHelper.SetLabelText(SkillDescLabel, byHeroInfo);
		}
	}

	private void SetSkillDesc()
	{
		UIHelper.SetLabelText(SkillDescLabel, "-");
		int soulWeaponEquipHeroIndex = UIHelper.GetSoulWeaponEquipHeroIndex(_soulWeaponInfo);
		if (soulWeaponEquipHeroIndex != 0)
		{
			int basicSkillIndex = _soulWeaponInfo.GetBasicSkillIndex();
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(soulWeaponEquipHeroIndex);
			string byHeroInfo = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(basicSkillIndex, heroInfo);
			UIHelper.SetLabelText(SkillDescLabel, byHeroInfo);
		}
	}

	private void SetSkillUseCondition()
	{
		UIHelper.SetLabelText(SkillUseConditionLabel, "-");
		if (SoulWeaponSkillData != null && !__Extension_Method_ICollection__.IsNullOrEmpty(SoulWeaponSkillData.ActiveConditionSkillIndices))
		{
			int skillIndex = SoulWeaponSkillData.ActiveConditionSkillIndices[0];
			int soulWeaponEquipHeroIndex = UIHelper.GetSoulWeaponEquipHeroIndex(_soulWeaponInfo);
			if (soulWeaponEquipHeroIndex != 0)
			{
				CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(soulWeaponEquipHeroIndex);
				string byHeroInfo = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(skillIndex, heroInfo);
				UIHelper.SetLabelText(SkillUseConditionLabel, byHeroInfo);
			}
		}
	}

	private void SetSkillCoolTimeUseCount()
	{
		string stringDirect = LocalizationManager.GetStringDirect("SOULWEAPON_INFO_COOLTIME", SoulWeaponSkillCoolTime);
		long soulWeaponSkillUseCount = SoulWeaponSkillUseCount;
		string stringDirect2 = LocalizationManager.GetStringDirect("SOULWEAPON_INFO_SKILLCOUNT", soulWeaponSkillUseCount);
		UIHelper.SetLabelText(SkillCoolTimeUseCountLabel, stringDirect + ((soulWeaponSkillUseCount > 0) ? (" / " + stringDirect2) : ""));
	}
}
