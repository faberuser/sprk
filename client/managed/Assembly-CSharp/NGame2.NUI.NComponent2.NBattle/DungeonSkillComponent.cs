using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleThread.NThreadAction;
using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NGame2.NUtil.NStringProxy.NTypeText;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class DungeonSkillComponent : BaseSkillComponent
{
	public delegate void ClickDungeonSkillCallback(TeamType teamType, int skillSlotIndex);

	[Serializable]
	public class DungeonSkillEffectData
	{
		public BattleType BattleType;

		public int BaseStateSkillReady;

		public int EnhancedStateSkillReady;

		public int EnhancedStateSkillLoop;
	}

	public const string PrefabPath = "Prefab2/Component/Battle/DungeonSkillComponent";

	public static PrefabAuto<DungeonSkillComponent> PrefabAuto = new PrefabAuto<DungeonSkillComponent>("Prefab2/Component/Battle/DungeonSkillComponent");

	[Header("[DungeonSkill]")]
	public UISpriteSelector FrameSelector;

	public UISpriteSelector BaseStateGaugeSelector;

	public UISpriteSelector EnhancedStateGaugeSelector;

	public UISprite BaseStateGauge;

	public UISprite EnhancedStateGauge;

	public GameObject UseableCountRoot;

	public UISprite SpriteCountCoolTime;

	public UILabel LabelUseableCount;

	public List<DungeonSkillEffectData> DungeonSkillEffectList;

	private ClickDungeonSkillCallback _onClickDungeonSkillAction;

	private DungeonSkillEffectData _currentSkillEffectData;

	private SkillUseResult _cachedLastSkillReadyState;

	protected override BattleFacade.CreatureCacheData _creatureCache => SingletonAuto<BattleFacade>.instance.GetEnvironmentCreatureCache(TeamType.Left);

	public void Open(int skillSlotIndex, Helper.SkillBasicInfo skillBasicInfo, ClickDungeonSkillCallback onClickDungeonSkillAction, BattleType battleType)
	{
		base.Open(0, skillSlotIndex, skillBasicInfo, null);
		CampaignDungeonSkillData data = GameRoot.Instance.CampaignDungeonSkillDataContainer.GetData(skillBasicInfo.TableIndex);
		if (data != null && _creatureCache != null)
		{
			_cachedLastSkillReadyState = SkillUseResult.NotEnoughStateCount;
			_onClickDungeonSkillAction = onClickDungeonSkillAction;
			SetSkillEffectData(battleType);
			bool existUseableCount = data.MaxUseCount > 0;
			SetSkillUiStyle(battleType, existUseableCount);
			SetSkillInfo(skillBasicInfo, skillBasicInfo.TableIndex);
			SetIconLocked(isLocked: true);
		}
	}

	public override void UpdateExtra()
	{
		BattleFacade.CreatureCacheData creatureCache = _creatureCache;
		if (creatureCache != null)
		{
			DungeonSkillStateInfo dungeonSkillStateInfo = creatureCache.GetDungeonSkillStateInfo(base.SkillSlotIndex);
			if (dungeonSkillStateInfo != null)
			{
				UIHelper.SetLabelText(LabelUseableCount, dungeonSkillStateInfo.UseableSkillCount);
				UIHelper.SetSpriteFillAmount(BaseStateGauge, (float)dungeonSkillStateInfo.BaseStateRatioM * 0.001f);
				UIHelper.SetSpriteFillAmount(EnhancedStateGauge, (float)dungeonSkillStateInfo.EnhancedStateRatioM * 0.001f);
			}
		}
	}

	public override void OnClickSkill()
	{
		SkillUseResult syncSkillUseResult = GetSyncSkillUseResult();
		if (CanUseSkill(syncSkillUseResult) && _onClickDungeonSkillAction != null)
		{
			_onClickDungeonSkillAction(TeamType.Left, base.SkillSlotIndex);
			_isReservedActive = true;
		}
	}

	protected override bool CanUpdate(BattleFacade.CreatureCacheData creatureCache)
	{
		if (creatureCache == null || !creatureCache.HasDungeonSkill || __Extension_Method_ICollection__.COUNT(creatureCache.DungeonSkillStateInfos) == 0)
		{
			return false;
		}
		return true;
	}

	protected override void UpdateEffect(BattleFacade.CreatureCacheData creatureCache)
	{
		SkillUseResult syncSkillUseResult = GetSyncSkillUseResult();
		IsReservedSkill();
		bool flag = IsSkillReady(syncSkillUseResult);
		bool flag2 = CanUseSkill(syncSkillUseResult);
		SetIconLocked(!flag2);
		SetSpriteReadyColor(flag);
		if (_cachedLastSkillReadyState != syncSkillUseResult && flag)
		{
			OnOffSkillReadyEffect(isReady: true, syncSkillUseResult);
		}
		_cachedLastSkillReadyState = syncSkillUseResult;
	}

	protected override void SetSyncCanUseSkill()
	{
		SkillUseResult syncSkillUseResult = GetSyncSkillUseResult();
		SkillUseOption skillUseOption = new SkillUseOption
		{
			CheckLevel = false,
			CheckMp = false,
			CheckStateCount = true
		};
		_syncCanUseSkill = new GetResultOfCanUseSkill(_creatureUid, base.SkillInfo.TableData, skillUseOption, GetResultOfCanUseSkill.SkillType.DungeonSkill)
		{
			SkillUseResult = syncSkillUseResult
		};
	}

	public override void SetReplaceSkillData(ReplaceSkillData replaceSkill, Helper.SkillBasicInfo skillInfo)
	{
		int skillIndex = replaceSkill.SkillIndex;
		if (skillIndex != base.SkillIndex && GameRoot.Instance.SkillDataContainer.GetData(skillIndex) != null && (replaceSkill.Type == EReplaceSkillType.EnhancedDungeonSkill || replaceSkill.Type == EReplaceSkillType.BaseDungeonSkill))
		{
			SetSkillInfo(skillInfo, skillIndex);
		}
	}

	private void OnOffSkillReadyEffect(bool isReady, SkillUseResult skillUseResult)
	{
		if (isReady)
		{
			switch (skillUseResult)
			{
			case SkillUseResult.Success:
				PlayEffect(_currentSkillEffectData.BaseStateSkillReady);
				break;
			case SkillUseResult.EnhancedSuccess:
				PlayEffect(_currentSkillEffectData.EnhancedStateSkillReady);
				PlayEffect(_currentSkillEffectData.EnhancedStateSkillLoop);
				break;
			}
		}
	}

	protected override void OnOffCooTimeGameObject(bool isOn)
	{
		base.OnOffCooTimeGameObject(isOn);
		UIHelper.EnableGameObject(SpriteCountCoolTime, isOn);
	}

	protected override void SetIconLocked(bool isLocked)
	{
		UIHelper.SetWidgetGrayScale(WidgetThis, isLocked);
		if (isLocked)
		{
			StopAllEffect();
		}
	}

	protected override void SetTooltipText()
	{
		if (base.SkillInfo.TableIndex != 0)
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
			if (base.SkillInfo.TableData != null)
			{
				value = SkillDescContainer.GetSkillName(base.SkillInfo.TableData);
			}
			_toolTipTextInfo.Name.Value = value;
			_toolTipTextInfo.Desc.Assign(new SkillDescBuilder(base.SkillInfo.TableIndex, _creatureCache.HeroInfo, appendLine: false));
			_toolTipTextInfo.TypeText.Assign(new SkillBuilder(base.SkillInfo, appendLine: false));
			_toolTipTextInfo.TooltipType = SkillTooltipTextInfo.SkillTooltipType.DungeonSkill;
			_toolTipTextInfo.isActiveSkill = base.SkillInfo.TableData.TriggerType == SkillTriggerType.None;
			_toolTipTextInfo.CooltimeMs = _syncCanUseSkill.TableCooltimeMs;
		}
	}

	protected override string ExtractSkillUseResult(SkillUseResult skillUseResult)
	{
		return skillUseResult switch
		{
			SkillUseResult.NotEnoughStateCount => string.Empty, 
			SkillUseResult.UseSkillMaxCount => string.Empty, 
			SkillUseResult.EnhancedSuccess => string.Empty, 
			_ => base.ExtractSkillUseResult(skillUseResult), 
		};
	}

	private void SetSkillEffectData(BattleType battleType)
	{
		_currentSkillEffectData = DungeonSkillEffectList.Find((DungeonSkillEffectData e) => e.BattleType == battleType);
		if (_currentSkillEffectData == null)
		{
			_currentSkillEffectData = Vespa_Generic_IList_Extension_Method.GetValid(DungeonSkillEffectList, 0);
		}
	}

	private void SetSkillUiStyle(BattleType battleType, bool existUseableCount)
	{
		if (FrameSelector != null)
		{
			bool enable = FrameSelector.SelectEnum(battleType);
			UIHelper.EnableGameObject(FrameSelector, enable);
		}
		if (BaseStateGaugeSelector != null)
		{
			bool enable2 = BaseStateGaugeSelector.SelectEnum(battleType);
			UIHelper.EnableGameObject(BaseStateGaugeSelector, enable2);
			UIHelper.SetSpriteFillAmount(BaseStateGauge, 0f);
		}
		if (EnhancedStateGaugeSelector != null)
		{
			bool enable3 = EnhancedStateGaugeSelector.SelectEnum(battleType);
			UIHelper.EnableGameObject(EnhancedStateGaugeSelector, enable3);
			UIHelper.SetSpriteFillAmount(EnhancedStateGauge, 0f);
		}
		UIHelper.EnableGameObject(UseableCountRoot, existUseableCount);
	}
}
