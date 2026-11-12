using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleThread.NThreadAction;
using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NGame2.NUtil.NStringProxy.NTypeText;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class SoulWeaponSkillComponent : BaseSkillComponent
{
	public const string PrefabPath = "Prefab2/Component/Battle/SoulWeaponSkillComponent";

	public static PrefabAuto<SoulWeaponSkillComponent> PrefabAuto = new PrefabAuto<SoulWeaponSkillComponent>("Prefab2/Component/Battle/SoulWeaponSkillComponent");

	public UISprite SpriteStack;

	public UILabel LabelUseableCount;

	public GameObject GoUseableEffect;

	public UISprite SpriteUseable;

	public UISprite SpriteCountCoolTime;

	public Transform TransReadyEffect;

	public UISpriteSelector UISpriteSelectorStack;

	public UISpriteSelector UISpriteSelectorButton;

	public UISpriteSelector UISpriteSelectorCount;

	public override void Open(int creatureUid, int skillSlotIndex, Helper.SkillBasicInfo skillBasicInfo, ClickIconCallback onClickAction)
	{
		base.Open(creatureUid, skillSlotIndex, skillBasicInfo, onClickAction);
		SetStackColor(skillBasicInfo.StackTypeName);
	}

	private void SetStackColor(string stackTypeName)
	{
		if (UISpriteSelectorStack != null)
		{
			UISpriteSelectorStack.Select(stackTypeName);
		}
		if (UISpriteSelectorButton != null)
		{
			UISpriteSelectorButton.Select(stackTypeName);
		}
		if (UISpriteSelectorCount != null)
		{
			UISpriteSelectorCount.Select(stackTypeName);
		}
	}

	protected override void SetSkillIcon(int skillIndex)
	{
		string itemIconPath = UIHelper.GetItemIconPath(base.SkillInfo.ItemCode);
		UIHelper.SetBundleTexture(UIBundleSkillIcon, itemIconPath);
	}

	protected override void SetSpriteReadyColor(bool isReady)
	{
		base.SetSpriteReadyColor(isReady);
		Color color = (isReady ? Color.white : Color.gray);
		UIHelper.SetWidgetColor(SpriteUseable, color);
	}

	protected override bool IsDisableSlot()
	{
		return GetSyncSkillUseResult() == SkillUseResult.DisableSkillSlot;
	}

	protected override void SetDisableSkillSlot(bool isDisable)
	{
		UIHelper.SetBundleTextureGrayscale(UIBundleSkillIcon, isDisable);
		UIHelper.SetWidgetGrayScale(SpriteStack, isDisable);
		UIHelper.SetWidgetGrayScale(LabelUseableCount, isDisable);
		UIHelper.SetWidgetGrayScale(SpriteUseable, isDisable);
		UIHelper.SetWidgetGrayScale(SpriteReadyFrame, isDisable);
		if (isDisable)
		{
			SetCachedLastSkillReady(isReady: false);
			StopAllEffect();
		}
	}

	protected override void SetIconLocked(bool isLocked)
	{
		UIHelper.SetWidgetGrayScale(WidgetThis, isLocked);
		UIHelper.EnableGameObject(SpriteStack, !isLocked);
		UIHelper.EnableGameObject(LabelUseableCount, !isLocked);
		UIHelper.EnableGameObject(SpriteUseable, !isLocked);
		if (isLocked)
		{
			StopAllEffect();
			UIHelper.EnableGameObject(TransReadyEffect, enable: false);
		}
	}

	public override void StopAllEffect()
	{
		base.StopAllEffect();
		OnOffSkillReadyEffect(isReady: false);
	}

	protected override void SetSyncCanUseSkill()
	{
		SkillUseResult syncSkillUseResult = GetSyncSkillUseResult();
		_syncCanUseSkill = new GetResultOfCanUseSkill(_creatureUid, base.SkillInfo.TableData, new SkillUseOption
		{
			CheckLevel = false,
			CheckMp = false
		}, GetResultOfCanUseSkill.SkillType.SoulWeapon)
		{
			SkillUseResult = syncSkillUseResult
		};
	}

	protected override void OnOffCooTimeGameObject(bool isOn)
	{
		base.OnOffCooTimeGameObject(isOn);
		UIHelper.EnableGameObject(SpriteCountCoolTime, isOn);
	}

	protected override void OnOffSkillReadyEffect(bool isReady)
	{
		OnOffSkillResevedEffect(isReady);
		UIHelper.EnableGameObject(TransReadyEffect, isReady);
	}

	protected override bool IsReservedSkill()
	{
		return _creatureCache.IsReservedSoulWeaponSkill;
	}

	public override void UpdateExtra()
	{
		if (base.SkillInfo.IsLocked)
		{
			SetIconLocked(isLocked: true);
			return;
		}
		if (_creatureCache.SoulWeaponUsableCount == 0)
		{
			SetIconLocked(isLocked: true);
			return;
		}
		if (_toolTipTextInfo != null)
		{
			_toolTipTextInfo.CooltimeMs = _syncCanUseSkill.TableCooltimeMs;
		}
		BattleFacade.CreatureCacheData creatureCache = _creatureCache;
		float fillAmount = (float)creatureCache.SoulWeaponStackRatioM / 1000f;
		UIHelper.SetSpriteFillAmount(SpriteStack, fillAmount);
		if (creatureCache.SoulWeaponUsableCount == 1000)
		{
			UIHelper.EnableGameObject(GoUseableEffect, enable: false);
			UIHelper.EnableGameObject(SpriteUseable, enable: false);
			UIHelper.EnableGameObject(LabelUseableCount, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(GoUseableEffect, enable: true);
			UIHelper.EnableGameObject(SpriteUseable, enable: true);
			UIHelper.EnableGameObject(LabelUseableCount, enable: true);
			UIHelper.SetLabelText(LabelUseableCount, creatureCache.SoulWeaponUsableCount);
		}
	}

	protected override string ExtractSkillUseResult(SkillUseResult skillUseResult)
	{
		return _creatureCache.SoulWeaponCanUseSkillResult switch
		{
			SkillUseResult.NotEnoughStack => string.Empty, 
			SkillUseResult.UseSkillMaxCount => string.Empty, 
			_ => base.ExtractSkillUseResult(skillUseResult), 
		};
	}

	protected override void SetTooltipText()
	{
		if (base.SkillInfo.TableIndex == 0)
		{
			return;
		}
		if (_toolTipTextInfo == null)
		{
			_toolTipTextInfo = new SkillTooltipTextInfo();
		}
		else
		{
			_toolTipTextInfo.Reset();
		}
		if (base.SkillInfo.IsLocked)
		{
			_toolTipTextInfo.TooltipType = SkillTooltipTextInfo.SkillTooltipType.SoulWeapon;
			_toolTipTextInfo.IsOnlyDesc = true;
			_toolTipTextInfo.Desc.Value = LocalizationManager.GetStringDirect("SOULWEAPON_SKILL_INFOTOOLTIP");
			return;
		}
		string value = "Unknown";
		if (base.SkillInfo.TableData != null)
		{
			value = SkillDescContainer.GetSkillName(base.SkillInfo.TableData);
		}
		_toolTipTextInfo.IsOnlyDesc = false;
		_toolTipTextInfo.TooltipType = SkillTooltipTextInfo.SkillTooltipType.SoulWeapon;
		_toolTipTextInfo.Name.Value = value;
		_toolTipTextInfo.Desc.Assign(new SkillDescBuilder(base.SkillInfo.TableIndex, _creatureCache.HeroInfo, appendLine: false));
		_toolTipTextInfo.TypeText.Assign(new SkillBuilder(base.SkillInfo, appendLine: false));
		_toolTipTextInfo.isActiveSkill = base.SkillInfo.TableData.TriggerType == SkillTriggerType.None;
		_toolTipTextInfo.CooltimeMs = _syncCanUseSkill.TableCooltimeMs;
	}
}
