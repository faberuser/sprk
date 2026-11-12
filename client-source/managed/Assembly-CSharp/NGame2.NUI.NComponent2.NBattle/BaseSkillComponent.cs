using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleThread.NThreadAction;
using NGame2.NBattleView;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NGame2.NUtil.NStringProxy.NTypeText;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class BaseSkillComponent : LinkFactoryComponent
{
	public delegate void ClickIconCallback(int creatureUid, int skillSlotIndex);

	private enum ETooltip
	{
		None,
		Waiting,
		Show
	}

	private const int EffectEntry_UsingSkill = 0;

	private const int EffectEntry_SkillCharge = 1;

	private const int EffectEntry_SkillReady = 2;

	private const int EffectEntry_SkillReserve = 3;

	public UIWidget WidgetThis;

	public UIBundleLoader UIBundleSkillIcon;

	public UILabel LabelSkillReserved;

	public UILabel LabelStatus;

	public UISprite SpriteCoolTime;

	public UILabel LabelCoolTime;

	public UISprite SpriteReadyFrame;

	public UISpriteSelector SpriteSelectorReqLevel;

	public UIGame3DWidget EffectGroup;

	public UISprite SpriteDisableSkillSlot;

	protected int _creatureUid;

	protected GetResultOfCanUseSkill _syncCanUseSkill;

	protected SkillTooltipTextInfo _toolTipTextInfo;

	private Holder _receiverHolder = new Holder();

	private bool _skillPressed;

	private bool _cachedLastSkillReady;

	protected bool _isReservedActive;

	private bool _showingTooltip;

	private ClickIconCallback _onClickAction;

	private Transform _cachedTransform;

	public Helper.SkillBasicInfo SkillInfo { get; private set; }

	public int SkillSlotIndex { get; private set; }

	public int SkillIndex { get; private set; }

	protected virtual BattleFacade.CreatureCacheData _creatureCache => SingletonAuto<BattleFacade>.instance.GetCreatureCache(_creatureUid);

	public virtual void Open(int creatureUid, int skillSlotIndex, Helper.SkillBasicInfo skillBasicInfo, ClickIconCallback onClickAction)
	{
		_cachedLastSkillReady = false;
		_isReservedActive = false;
		_skillPressed = false;
		_cachedTransform = base.transform;
		_onClickAction = onClickAction;
		_creatureUid = creatureUid;
		SkillSlotIndex = skillSlotIndex;
		UIHelper.SetLabelText(LabelStatus, string.Empty);
		if (creatureUid != 0 && _creatureCache.Uid != 0)
		{
			SetSkillInfo(skillBasicInfo, skillBasicInfo.TableIndex);
			UIHelper.SetBundleTextureGrayscale(UIBundleSkillIcon, isGrayscale: false);
			CreatureVerifier verifier = new CreatureVerifier(_creatureUid);
			_receiverHolder.AddByCreatureUid<CallbackMessage<BeginFireSkill>>(verifier, null, _creatureUid);
		}
	}

	public void SetSkillInfo(Helper.SkillBasicInfo skillBasicInfo, int skillIndex)
	{
		SkillInfo = skillBasicInfo;
		SkillIndex = skillIndex;
		if (SkillInfo.TableData != null)
		{
			SetSyncCanUseSkill();
			SetSkillIcon(skillIndex);
			SetTooltipText();
			SetIconLocked(SkillInfo.IsLocked);
			UpdateExtra();
		}
	}

	protected virtual void SetIconLocked(bool isLocked)
	{
	}

	protected virtual void SetDisableSkillSlot(bool isDisable)
	{
	}

	protected virtual void SetSyncCanUseSkill()
	{
		SkillUseResult syncSkillUseResult = GetSyncSkillUseResult();
		_syncCanUseSkill = new GetResultOfCanUseSkill(_creatureUid, SkillInfo.TableData)
		{
			SkillUseResult = syncSkillUseResult
		};
	}

	public void ChainUpdate()
	{
		AddThreadSync();
		UpdateButtonStatus();
	}

	public virtual void Close()
	{
		_receiverHolder.Clear();
		StopAllEffect();
	}

	public virtual void OnMessageFast(CallbackMessage<BeginFireSkill> message)
	{
		if (!message.IsValidMessage() || message.Callback.CreatureUid != _creatureUid || message.Callback.SkillSlotIndex != SkillSlotIndex)
		{
			return;
		}
		if (message.Callback.SkillId == SkillInfo.TableIndex)
		{
			PlayEffect(0);
			return;
		}
		int nextIndex = SkillInfo.TableData.NextIndex;
		while (nextIndex != 0)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(nextIndex);
			if (data == null)
			{
				break;
			}
			if (data.Index == message.Callback.SkillId)
			{
				if (data.TriggerType == SkillTriggerType.NextActiveSkill)
				{
					PlayEffect(0);
				}
				break;
			}
			nextIndex = data.NextIndex;
		}
	}

	public virtual void SetReplaceSkillData(ReplaceSkillData replaceSkill, Helper.SkillBasicInfo skillInfo)
	{
		int skillIndex = replaceSkill.SkillIndex;
		if (skillIndex != SkillIndex)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
			if (data != null && (replaceSkill.Type != EReplaceSkillType.NextActiveSkill || data.HasTriggerFlag(SkillTriggerFlag.HideChangedSkillInfo)))
			{
				SetSkillInfo(skillInfo, skillIndex);
			}
		}
		if (IsPlayingEffect(1) && replaceSkill.Type != EReplaceSkillType.NextActiveSkill)
		{
			StopEffect(1, immediatelyClear: true);
		}
		else if (!IsPlayingEffect(1) && replaceSkill.Type == EReplaceSkillType.NextActiveSkill)
		{
			PlayEffect(1);
		}
	}

	public virtual void UpdateExtra()
	{
	}

	private void AddThreadSync()
	{
		if (!_syncCanUseSkill.IsWaiting)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadSync(_syncCanUseSkill);
		}
	}

	public virtual void UpdateButtonStatus()
	{
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (!_isReservedActive && battleCache.IsBattleStarted)
		{
			_isReservedActive = true;
		}
		if (SkillInfo.IsLocked)
		{
			SetSkillIconColor(isWhite: true);
			SetSpriteReadyColor(isReady: false);
			return;
		}
		UpdateSkillStatusLabel();
		bool flag = IsDisableSlot();
		SetDisableSkillSlot(flag);
		if (!flag)
		{
			BattleFacade.CreatureCacheData creatureCache = _creatureCache;
			if (!CanUpdate(creatureCache))
			{
				SetSpriteReadyColor(isReady: false);
				SetButtonScale(Vector3.one);
				StopAllEffect();
			}
			else
			{
				UpdateEffect(creatureCache);
				UpdateCoolTime();
				UpdateButtonScale();
				UpdateExtra();
			}
		}
	}

	protected virtual void SetSkillIcon(int skillIndex)
	{
		HeroInfo heroInfoFromCreatureUid = CreatureManager.GetHeroInfoFromCreatureUid(_creatureUid);
		string skillIconPath = UIHelper.GetSkillIconPath(skillIndex, heroInfoFromCreatureUid);
		UIHelper.SetBundleTexture(UIBundleSkillIcon, skillIconPath);
	}

	private void SetSkillIconColor(bool isWhite)
	{
		Color color = (isWhite ? Color.white : Colors.whiteGray);
		UIHelper.SetWidgetColor(UIBundleSkillIcon.UITEX(), color);
	}

	protected virtual void SetSpriteReadyColor(bool isReady)
	{
		Color color = (isReady ? Color.white : Color.gray);
		UIHelper.SetWidgetColor(SpriteReadyFrame, color);
	}

	private float GetCoolTimeFillAmount()
	{
		if (_syncCanUseSkill == null)
		{
			return 0f;
		}
		return 1f - _syncCanUseSkill.CooltimeFillAmount;
	}

	private int GetRemainCoolTime()
	{
		if (_syncCanUseSkill == null)
		{
			return 0;
		}
		return Mathf.CeilToInt((float)(_syncCanUseSkill.CooltimeDuratiomMs - _syncCanUseSkill.CooltimeElapsedMs) / 1000f);
	}

	protected virtual SkillUseResult GetSyncSkillUseResult()
	{
		if (_syncCanUseSkill == null)
		{
			return SkillUseResult.Fail;
		}
		return _syncCanUseSkill.SkillUseResult;
	}

	private void UpdateSkillStatusLabel()
	{
		SkillUseResult skillUseResult = (IsUsingSkill() ? SkillUseResult.AlreadyUsing : GetSyncSkillUseResult());
		UIHelper.SetLabelText(LabelStatus, ExtractSkillUseResult(skillUseResult));
	}

	private void UpdateCoolTime()
	{
		int remainCoolTime = GetRemainCoolTime();
		if (remainCoolTime > 0)
		{
			OnOffCooTimeGameObject(isOn: true);
			UIHelper.SetLabelText(LabelCoolTime, remainCoolTime);
		}
		else
		{
			OnOffCooTimeGameObject(isOn: false);
		}
		float coolTimeFillAmount = GetCoolTimeFillAmount();
		UIHelper.SetSpriteFillAmount(SpriteCoolTime, coolTimeFillAmount);
	}

	protected virtual void OnOffCooTimeGameObject(bool isOn)
	{
		UIHelper.EnableGameObject(LabelCoolTime, isOn);
	}

	protected void SetButtonScale(Vector3 scale)
	{
		if (_cachedTransform == null)
		{
			_cachedTransform = base.transform;
		}
		if (_cachedTransform.localScale != scale)
		{
			_cachedTransform.localScale = scale;
		}
	}

	private void UpdateButtonScale()
	{
		Vector3 buttonScale = (_skillPressed ? (Vector3.one * 1.1f) : Vector3.one);
		SetButtonScale(buttonScale);
	}

	protected virtual void UpdateEffect(BattleFacade.CreatureCacheData creatureCache)
	{
		SkillUseResult syncSkillUseResult = GetSyncSkillUseResult();
		bool num = IsReservedSkill();
		bool flag = IsSkillReady(syncSkillUseResult);
		bool skillIconColor = CanUseSkill(syncSkillUseResult);
		SetSkillIconColor(skillIconColor);
		SetSpriteReadyColor(flag);
		if (!_cachedLastSkillReady && flag)
		{
			OnOffSkillReadyEffect(isReady: true);
		}
		else if (_cachedLastSkillReady && !flag)
		{
			OnOffSkillReadyEffect(isReady: false);
		}
		_cachedLastSkillReady = flag;
		bool flag2 = IsAutoSkillReserved(creatureCache);
		if ((num && _isReservedActive) || flag2)
		{
			OnOffSkillResevedEffect(isReserved: true);
		}
		else
		{
			OnOffSkillResevedEffect(isReserved: false);
		}
	}

	protected virtual void OnOffSkillReadyEffect(bool isReady)
	{
		if (isReady)
		{
			PlayEffect(2);
		}
	}

	protected void OnOffSkillResevedEffect(bool isReserved)
	{
		if (isReserved)
		{
			if (!IsPlayingEffect(3))
			{
				PlayEffect(3);
			}
			UIHelper.EnableGameObject(LabelSkillReserved, enable: true);
		}
		else
		{
			if (IsPlayingEffect(3))
			{
				StopEffect(3, immediatelyClear: true);
			}
			UIHelper.EnableGameObject(LabelSkillReserved, enable: false);
		}
	}

	protected bool IsPlayingEffect(int index)
	{
		if (EffectGroup == null)
		{
			return false;
		}
		return EffectGroup.IsPlayEntry(index);
	}

	protected void PlayEffect(int index)
	{
		if (EffectGroup != null)
		{
			EffectGroup.PlayEntry(index, null);
		}
	}

	protected void StopEffect(int index, bool immediatelyClear)
	{
		if (EffectGroup != null)
		{
			EffectGroup.StopEntry(index, immediatelyClear);
		}
	}

	public virtual void StopAllEffect()
	{
		UIHelper.SetSpriteFillAmount(SpriteCoolTime, 0f);
		UIHelper.EnableGameObject(LabelCoolTime, enable: false);
		UIHelper.EnableGameObject(LabelSkillReserved, enable: false);
		if (EffectGroup != null)
		{
			EffectGroup.Stop(immediatelyClear: true);
		}
	}

	protected bool IsAutoSkillReserved(BattleFacade.CreatureCacheData creatureCache)
	{
		if (creatureCache.AutoAiSkillIndex != 0)
		{
			return creatureCache.AutoAiSkillIndex == SkillIndex;
		}
		return false;
	}

	protected bool IsSkillReady(SkillUseResult result)
	{
		if (IsUsingSkill())
		{
			return false;
		}
		if (result == SkillUseResult.Cooltime || result == SkillUseResult.NotEnoughMp || result == SkillUseResult.NotEnoughStack || result == SkillUseResult.UseSkillMaxCount || result == SkillUseResult.NotEnoughStateCount)
		{
			return false;
		}
		return true;
	}

	protected bool CanUseSkill(SkillUseResult result)
	{
		if (result != SkillUseResult.Success)
		{
			return result == SkillUseResult.EnhancedSuccess;
		}
		return true;
	}

	protected virtual bool CanUpdate(BattleFacade.CreatureCacheData creatureCache)
	{
		if (creatureCache.Uid == 0)
		{
			return false;
		}
		if (creatureCache.IsDeadOrResting())
		{
			return false;
		}
		return true;
	}

	protected virtual bool IsDisableSlot()
	{
		return false;
	}

	protected virtual bool IsReservedSkill()
	{
		return _creatureCache.ReservedSkillSlotIndex == SkillSlotIndex;
	}

	protected bool IsUsingSkill()
	{
		if (_creatureCache.UsingFiredSkillIndex == SkillInfo.TableIndex)
		{
			return _creatureCache.UsingSkillSlotIndex == SkillSlotIndex;
		}
		return false;
	}

	protected virtual bool CanClickSkill()
	{
		if (SkillInfo.IsLocked || IsDisableSlot())
		{
			return false;
		}
		return true;
	}

	public virtual void OnClickSkill()
	{
		if (CanClickSkill() && _onClickAction != null)
		{
			_onClickAction(_creatureUid, SkillSlotIndex);
			_isReservedActive = true;
		}
	}

	public void SetCachedLastSkillReady(bool isReady)
	{
		_cachedLastSkillReady = isReady;
	}

	public void OnPressSkill()
	{
		if (CanClickSkill())
		{
			_skillPressed = true;
		}
	}

	public void OnReleaseSkill()
	{
		if (CanClickSkill())
		{
			_skillPressed = false;
		}
	}

	public void OnShowTooltip()
	{
		SkillIconTooltip.Open(WidgetThis, _toolTipTextInfo);
		_showingTooltip = true;
	}

	public void OnHideTooltip()
	{
		SkillIconTooltip.Hide(WidgetThis);
		_showingTooltip = false;
	}

	public bool IsShowingTooltip()
	{
		return _showingTooltip;
	}

	protected virtual void SetTooltipText()
	{
		if (SkillInfo.TableIndex != 0)
		{
			if (_toolTipTextInfo == null)
			{
				_toolTipTextInfo = new SkillTooltipTextInfo();
			}
			else
			{
				_toolTipTextInfo.Reset();
			}
			string value = "Unknown";
			if (SkillInfo.TableData != null)
			{
				value = SkillDescContainer.GetSkillName(_creatureCache.HeroInfo, SkillInfo.TableData);
			}
			_toolTipTextInfo.Name.Value = value;
			_toolTipTextInfo.Desc.Assign(new SkillDescBuilder(SkillInfo.TableIndex, _creatureCache.HeroInfo, appendLine: false));
			_toolTipTextInfo.TypeText.Assign(new SkillBuilder(SkillInfo, appendLine: false));
			_toolTipTextInfo.SkillLevel = SkillInfo.Level;
			_toolTipTextInfo.isActiveSkill = SkillInfo.TableData.TriggerType == SkillTriggerType.None;
			long maxMp = _creatureCache.MaxMp;
			int tableReqMp = _syncCanUseSkill.TableReqMp;
			_toolTipTextInfo.InitReqManaGauge(tableReqMp, (int)maxMp);
			_toolTipTextInfo.CooltimeMs = _syncCanUseSkill.TableCooltimeMs;
		}
	}

	protected virtual string ExtractSkillUseResult(SkillUseResult skillUseResult)
	{
		return skillUseResult switch
		{
			SkillUseResult.Success => string.Empty, 
			SkillUseResult.Fail => "Error", 
			SkillUseResult.BattleDisabled => LocalizationManager.GetString(Battle.SKILL_USE_RESULT_NOT_BATTLE), 
			SkillUseResult.Died => LocalizationManager.GetString(Battle.SKILL_USE_RESULT_DEAD), 
			SkillUseResult.AlreadyCasting => LocalizationManager.GetString(Battle.SKILL_USE_RESULT_CASTING), 
			SkillUseResult.AlreadyUsing => LocalizationManager.GetString(Battle.SKILL_USE_RESULT_CASTING), 
			SkillUseResult.InvalidState => LocalizationManager.GetString(Battle.SKILL_USE_RESULT_INVALID_STATE), 
			SkillUseResult.Cooltime => string.Empty, 
			SkillUseResult.NotEnoughMp => string.Empty, 
			SkillUseResult.InvalidTarget => LocalizationManager.GetString(Battle.SKILL_USE_RESULT_INVALID_TARGET), 
			SkillUseResult.InvalidFriendlyType => LocalizationManager.GetString(Battle.SKILL_USE_RESULT_INVALID_TARGET), 
			SkillUseResult.InvalidSkillLevel => LocalizationManager.GetString(Battle.SKILL_USE_RESULT_INVALID_SKILL_LEVEL), 
			SkillUseResult.TargetTooFar => LocalizationManager.GetString(Battle.SKILL_USE_RESULT_TARGET_TOO_FAR), 
			SkillUseResult.TargetTooNear => LocalizationManager.GetString(Battle.SKILL_USE_RESULT_TARGET_TOO_NEAR), 
			SkillUseResult.CreatureTagTypeMismatch => LocalizationManager.GetString(Battle.SKILL_USE_RESULT_CREATURE_TAG_TYPE_MISMATCH), 
			SkillUseResult.ConditionFail => string.Empty, 
			SkillUseResult.NotEnoughStateCount => LocalizationManager.GetString(Battle.SKILL_USE_RESULT_NOT_ENOUGH_STATE_COUNT), 
			SkillUseResult.DisableSkillSlot => string.Empty, 
			_ => "!!Not Yet!!", 
		};
	}

	public static string GetSkillTypeText(Helper.SkillBasicInfo skillInfo)
	{
		if (skillInfo.TableData == null)
		{
			return "";
		}
		return LocalizationManager.GetString(skillInfo.TableData.TriggerType switch
		{
			SkillTriggerType.OnStart => HeroSkill.SKILL_TYPE_PASSIVE, 
			SkillTriggerType.OnAuto => HeroSkill.SKILL_TYPE_AUTO, 
			SkillTriggerType.None => HeroSkill.SKILL_TYPE_ACTIVE, 
			_ => NShared.NLocalization.Common.COMMON_ETC, 
		});
	}
}
