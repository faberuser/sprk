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

public class SkillComponent : LinkFactoryComponent
{
	public delegate void ClickIconCallback(int creatureUid, int skillSlotIndex);

	private enum ETooltip
	{
		None,
		Waiting,
		Show
	}

	public const string PrefabPath = "Prefab2/Component/Battle/SkillComponent";

	public static PrefabAuto<SkillComponent> PrefabAuto = new PrefabAuto<SkillComponent>("Prefab2/Component/Battle/SkillComponent");

	private const int EffectEntry_UsingSkill = 0;

	private const int EffectEntry_SkillCharge = 1;

	private const int EffectEntry_SkillReady = 2;

	private const int EffectEntry_SkillReserve = 3;

	[SerializeField]
	private UIWidget _widgetThis;

	[SerializeField]
	private UIBundleLoader _iconBundleTex;

	[SerializeField]
	private UISprite _frameReady;

	[SerializeField]
	private UISprite _cooltime;

	[SerializeField]
	private UILabel _cooltimeSecondLabel;

	[SerializeField]
	private UISpriteSelector _reqLevelSprite;

	[SerializeField]
	private UIGame3DWidget _uiEffect_All;

	[SerializeField]
	private Transform _skillReservedRoot;

	[SerializeField]
	private UILabel _statusLabel;

	[SerializeField]
	private UISpriteSelector _gaugeSelect;

	private int _creatureUid;

	private int _skillSlotIndex;

	private GetResultOfCanUseSkill _syncCanUseSkill;

	private bool _skillPressed;

	private bool _isSkillReady;

	private bool _isReservedActive;

	private ClickIconCallback _onClickAction;

	private SkillTooltipTextInfo _TooltipTextInfo;

	private bool _showingTooltip;

	private Holder _receiverHolder = new Holder();

	private float _lastUpdateReqMp;

	private Helper.SkillBasicInfo _initialSkillInfo;

	private bool _isSoulweaponSkill;

	public Helper.SkillBasicInfo SkillInfo { get; private set; }

	public int SkillSlotIndex => _skillSlotIndex;

	public int SkillIndex { get; private set; }

	private BattleFacade.CreatureCacheData _creatureCache => SingletonAuto<BattleFacade>.instance.GetCreatureCache(_creatureUid);

	public bool Open(int creatureUid, int skillSlotIndex, Helper.SkillBasicInfo skillInfo, ClickIconCallback onClick, bool isSoulWeaponSkill)
	{
		UIHelper.SetLabelText(_statusLabel, string.Empty);
		_skillPressed = false;
		_isSkillReady = false;
		_isReservedActive = false;
		_creatureUid = creatureUid;
		_skillSlotIndex = skillSlotIndex;
		_onClickAction = onClick;
		_initialSkillInfo = skillInfo;
		_isSoulweaponSkill = isSoulWeaponSkill;
		if (_creatureCache.Uid == 0)
		{
			return false;
		}
		UIHelper.SetBundleTextureGrayscale(_iconBundleTex, isGrayscale: false);
		InitSkillInfo(skillInfo, skillInfo.TableIndex);
		CreatureVerifier verifier = new CreatureVerifier(_creatureUid);
		_receiverHolder.AddByCreatureUid<CallbackMessage<BeginFireSkill>>(verifier, OnMessageFast, _creatureUid);
		return true;
	}

	private void InitSkillInfo(Helper.SkillBasicInfo skillInfo, int skillIndex)
	{
		SkillInfo = skillInfo;
		SkillIndex = skillIndex;
		if (SkillInfo.TableData != null)
		{
			SkillUseResult skillUseResult = ((_syncCanUseSkill == null) ? SkillUseResult.Fail : _syncCanUseSkill.SkillUseResult);
			if (_isSoulweaponSkill)
			{
				_syncCanUseSkill = new GetResultOfCanUseSkill(_creatureUid, skillInfo.TableData, new SkillUseOption
				{
					CheckLevel = false,
					CheckMp = false
				}, GetResultOfCanUseSkill.SkillType.SoulWeapon);
			}
			else
			{
				_syncCanUseSkill = new GetResultOfCanUseSkill(_creatureUid, skillInfo.TableData);
			}
			_syncCanUseSkill.SkillUseResult = skillUseResult;
			SetSkillIcon(skillIndex);
			UIHelper.EnableGameObject(_reqLevelSprite, SkillInfo.IsLocked);
			if (SkillInfo.IsLocked)
			{
				StopAllEffect();
				_reqLevelSprite.SelectInt(_skillSlotIndex);
			}
			SetupTooltipText();
			UpdateReqMp();
		}
	}

	private bool SetSkillIcon(int skillIndex)
	{
		HeroInfo heroInfoFromCreatureUid = CreatureManager.GetHeroInfoFromCreatureUid(_creatureUid);
		string skillIconPath = UIHelper.GetSkillIconPath(skillIndex, heroInfoFromCreatureUid);
		UIHelper.SetBundleTexture(_iconBundleTex, skillIconPath);
		return true;
	}

	private void StopAllEffect()
	{
		UIHelper.SetSpriteFillAmount(_cooltime, 0f);
		UIHelper.EnableGameObject(_cooltimeSecondLabel, enable: false);
		UIHelper.EnableGameObject(_skillReservedRoot, enable: false);
		_uiEffect_All.Stop(immediatelyClear: true);
	}

	public void OnMessageFast(CallbackMessage<BeginFireSkill> message)
	{
		if (!message.IsValidMessage() || message.Callback.CreatureUid != _creatureUid || message.Callback.SkillSlotIndex != _skillSlotIndex)
		{
			return;
		}
		if (message.Callback.SkillId == SkillInfo.TableIndex)
		{
			_uiEffect_All.PlayEntry(0, null);
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
					_uiEffect_All.PlayEntry(0, null);
				}
				break;
			}
			nextIndex = data.NextIndex;
		}
	}

	public void SetReplaceSkillData(ReplaceSkillData replaceSkill, Helper.SkillBasicInfo skillInfo)
	{
		int skillIndex = replaceSkill.SkillIndex;
		if (skillIndex != SkillIndex)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
			if (data != null && (replaceSkill.Type != EReplaceSkillType.NextActiveSkill || data.HasTriggerFlag(SkillTriggerFlag.HideChangedSkillInfo)))
			{
				InitSkillInfo(skillInfo, skillIndex);
			}
		}
		if (_uiEffect_All.IsPlayEntry(1) && replaceSkill.Type != EReplaceSkillType.NextActiveSkill)
		{
			_uiEffect_All.StopEntry(1, immediatelyClear: true);
		}
		else if (!_uiEffect_All.IsPlayEntry(1) && replaceSkill.Type == EReplaceSkillType.NextActiveSkill)
		{
			_uiEffect_All.PlayEntry(1, null);
		}
	}

	public void Close()
	{
		_receiverHolder.Clear();
		StopAllEffect();
	}

	public void OnPressSkill()
	{
		if (!SkillInfo.IsLocked)
		{
			_skillPressed = true;
		}
	}

	public void OnReleaseSkill()
	{
		if (!SkillInfo.IsLocked)
		{
			_skillPressed = false;
		}
	}

	public void OnClickSkill()
	{
		if (!SkillInfo.IsLocked && _onClickAction != null)
		{
			_onClickAction(_creatureUid, _skillSlotIndex);
			_isReservedActive = true;
		}
	}

	public void OnShowTooltip()
	{
		SkillIconTooltip.Open(_widgetThis, _TooltipTextInfo);
		_showingTooltip = true;
	}

	public void OnHideTooltip()
	{
		SkillIconTooltip.Hide(_widgetThis);
		_showingTooltip = false;
	}

	public bool IsShowingTooltip()
	{
		return _showingTooltip;
	}

	public void ChainUpdate()
	{
		if (!_syncCanUseSkill.IsWaiting)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadSync(_syncCanUseSkill);
		}
		UpdateButton();
		if (Time.time - _lastUpdateReqMp > 0.5f)
		{
			_lastUpdateReqMp = Time.time;
			UpdateReqMp();
		}
	}

	private void UpdateButton()
	{
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (!_isReservedActive && battleCache.IsBattleStarted)
		{
			_isReservedActive = true;
		}
		RefreshButtonStatus();
	}

	private bool RefreshButtonStatus()
	{
		if (SkillInfo.IsLocked)
		{
			UIHelper.SetWidgetColor(_iconBundleTex.UITEX(), Color.white);
			_frameReady.color = Color.gray;
			return false;
		}
		BattleFacade.CreatureCacheData creatureCache = _creatureCache;
		if (creatureCache.Uid == 0 || creatureCache.IsDeadOrResting())
		{
			_frameReady.color = Color.gray;
			base.transform.localScale = Vector3.one;
			StopAllEffect();
			return false;
		}
		SkillUseResult skillUseResult = _syncCanUseSkill.SkillUseResult;
		bool flag = !IsUsingSkill() && skillUseResult != SkillUseResult.Cooltime && skillUseResult != SkillUseResult.NotEnoughMp && skillUseResult != SkillUseResult.NotEnoughStack && skillUseResult != SkillUseResult.UseSkillMaxCount && skillUseResult != SkillUseResult.NotEnoughStateCount;
		bool flag2 = skillUseResult == SkillUseResult.Success;
		bool flag3 = false;
		flag3 = ((!_isSoulweaponSkill) ? (creatureCache.ReservedSkillSlotIndex == _skillSlotIndex) : creatureCache.IsReservedSoulWeaponSkill);
		if (!_isSkillReady && flag)
		{
			_uiEffect_All.PlayEntry(2, null);
		}
		_isSkillReady = flag;
		bool flag4 = creatureCache.AutoAiSkillIndex != 0 && creatureCache.AutoAiSkillIndex == SkillIndex;
		if ((flag3 && _isReservedActive) || flag4)
		{
			if (!_uiEffect_All.IsPlayEntry(3))
			{
				_uiEffect_All.PlayEntry(3, null);
			}
			UIHelper.EnableGameObject(_skillReservedRoot, enable: true);
		}
		else
		{
			if (_uiEffect_All.IsPlayEntry(3))
			{
				_uiEffect_All.StopEntry(3, immediatelyClear: true);
			}
			UIHelper.EnableGameObject(_skillReservedRoot, enable: false);
		}
		_frameReady.color = (flag ? Color.white : Color.gray);
		UIHelper.SetWidgetColor(_iconBundleTex.UITEX(), flag2 ? Color.white : Colors.whiteGray);
		UIHelper.SetSpriteFillAmount(_cooltime, 1f - _syncCanUseSkill.CooltimeFillAmount);
		int num = Mathf.CeilToInt((float)(_syncCanUseSkill.CooltimeDuratiomMs - _syncCanUseSkill.CooltimeElapsedMs) / 1000f);
		if (num > 0)
		{
			UIHelper.EnableGameObject(_cooltimeSecondLabel, enable: true);
			UIHelper.SetLabelText(_cooltimeSecondLabel, num);
		}
		else
		{
			UIHelper.EnableGameObject(_cooltimeSecondLabel, enable: false);
		}
		if (IsUsingSkill())
		{
			UIHelper.SetLabelText(_statusLabel, ExtractSkillUseResult(SkillUseResult.AlreadyUsing));
		}
		else if (skillUseResult == SkillUseResult.NotEnoughStack)
		{
			int soulWeaponStackCount = creatureCache.SoulWeaponStackCount;
			int soulWeaponMaxStackCount = creatureCache.SoulWeaponMaxStackCount;
			UIHelper.SetLabelText(_statusLabel, $"{soulWeaponStackCount} / {soulWeaponMaxStackCount}");
		}
		else
		{
			UIHelper.SetLabelText(_statusLabel, ExtractSkillUseResult(skillUseResult));
		}
		base.transform.localScale = Vector3.one * (_skillPressed ? 1.1f : 1f);
		return flag2;
	}

	private void UpdateReqMp()
	{
		long maxMp = _creatureCache.MaxMp;
		int tableReqMp = _syncCanUseSkill.TableReqMp;
		if (_TooltipTextInfo != null)
		{
			_TooltipTextInfo.InitReqManaGauge(tableReqMp, (int)maxMp);
			_TooltipTextInfo.CooltimeMs = _syncCanUseSkill.TableCooltimeMs;
		}
		int value = ((!SkillInfo.IsLocked) ? Mathf.RoundToInt((float)tableReqMp * 0.001f) : 0);
		if (_gaugeSelect != null)
		{
			bool enable = _gaugeSelect.SelectInt(value);
			UIHelper.EnableGameObject(_gaugeSelect, enable);
		}
	}

	private bool IsUsingSkill()
	{
		if (_creatureCache.UsingFiredSkillIndex == SkillInfo.TableIndex)
		{
			return _creatureCache.UsingSkillSlotIndex == _skillSlotIndex;
		}
		return false;
	}

	private string ExtractSkillUseResult(SkillUseResult skillUseResult)
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
			SkillUseResult.NotEnoughStack => string.Empty, 
			SkillUseResult.UseSkillMaxCount => string.Empty, 
			_ => "!!Not Yet!!", 
		};
	}

	private void SetupTooltipText()
	{
		if (SkillInfo.TableIndex != 0)
		{
			if (_TooltipTextInfo == null)
			{
				_TooltipTextInfo = new SkillTooltipTextInfo();
			}
			_TooltipTextInfo.Reset();
			string value = "Unknown";
			if (SkillInfo.TableData != null)
			{
				value = SkillDescContainer.GetSkillName(SkillInfo.TableData);
			}
			_TooltipTextInfo.Name.Value = value;
			_TooltipTextInfo.Desc.Assign(new SkillDescBuilder(SkillInfo.TableIndex, _creatureCache.HeroInfo, appendLine: false));
			_TooltipTextInfo.TypeText.Assign(new SkillBuilder(SkillInfo, appendLine: false));
			_TooltipTextInfo.SkillLevel = SkillInfo.Level;
			_TooltipTextInfo.isActiveSkill = SkillInfo.TableData.TriggerType == SkillTriggerType.None;
			long maxMp = _creatureCache.MaxMp;
			int tableReqMp = _syncCanUseSkill.TableReqMp;
			_TooltipTextInfo.InitReqManaGauge(tableReqMp, (int)maxMp);
			_TooltipTextInfo.CooltimeMs = _syncCanUseSkill.TableCooltimeMs;
		}
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
