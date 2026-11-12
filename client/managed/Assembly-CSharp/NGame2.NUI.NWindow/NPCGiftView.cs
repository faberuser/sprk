using System;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NRender;
using NGame2.NUI.NGame3DWidget;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class NPCGiftView : NPCRecruitView
{
	public new const string PrefabPath = "Resources/Prefab2/Window/NPCRecruit/NPCGiftView";

	public UIButton ButtonGiveGift;

	public UIButton ButtonTakeReward;

	public UIButton ButtonHelpList;

	public GameObject RootEventRemainTime;

	public UILabel LabelEventRemainTime;

	public Transform TransformSubHero;

	public UIGame3DWidget Game3DWidget;

	private DropBonusEvent _dropBonusEvent;

	private bool _updateEventRemainTime;

	protected RenderFacade _subRenderFacde;

	protected ISound _subSound;

	private CachedHeroInfo _subHeroInfo;

	private LocalStorageBool _showHelpList;

	private string[] _menuCondition;

	private string _helpListCode;

	private IContents _effect;

	public void Open(NPCData npcData, string titleKey, NpcFriendlyType friendlyType, string[] menuCondition, string helpListCode, int subHeroIndex, Listener listener)
	{
		title.Key = titleKey;
		_listener = listener;
		base.NPCData = npcData;
		base._friendlyType = friendlyType;
		_menuCondition = menuCondition;
		_helpListCode = helpListCode;
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(npcData.CreatureIndex);
		if (data != null)
		{
			CachedHeroInfo referenceHero = SceneAutoBehaviour<StatDataCache>.instance.GetReferenceHero(data.Index, 1);
			if (subHeroIndex != 0)
			{
				CachedHeroInfo referenceHero2 = SceneAutoBehaviour<StatDataCache>.instance.GetReferenceHero(subHeroIndex, 1);
				_subHeroInfo = referenceHero2;
			}
			base.NPCFriendlyInfo = GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Get(base.NPCData.CreatureIndex);
			if (base.NPCFriendlyInfo == null)
			{
				base.NPCFriendlyInfo = new NPCFriendlyInfo
				{
					FriendlyPoint = 0,
					Step = 1,
					HeroIndex = base.NPCData.CreatureIndex
				};
			}
			InitAction = (Action)Delegate.Combine(InitAction, new Action(base.HideSpeak));
			InitAction = (Action)Delegate.Combine(InitAction, new Action(InitSubHeroModel));
			InitAction = (Action)Delegate.Combine(InitAction, new Action(InitDropBonusEvent));
			new DelegateTask(OpenAsync(referenceHero, isSelectHero: true)).Commit();
		}
	}

	private void InitHelpList()
	{
		bool flag = !String_Extension_Method.IsNullOrEmpty(_helpListCode);
		UIHelper.EnableGameObject(ButtonHelpList, flag);
		if (flag)
		{
			if (_showHelpList == null)
			{
				_showHelpList = new LocalStorageBool(i_default: false, $"GiftView_{base.NPCData.Code}_{base.NPCData.CreatureIndex}_{_helpListCode}");
			}
			if (!_showHelpList.Value)
			{
				EventGuideListPopup.Open(_helpListCode);
				_showHelpList.Value = true;
			}
		}
	}

	private void InitDropBonusEvent()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_menuCondition))
		{
			EventBonusType valueEnum = ArgParser.GetValueEnum(_menuCondition, 1, EventBonusType.None);
			string valueString = ArgParser.GetValueString(_menuCondition, 2);
			_dropBonusEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(valueEnum, valueString);
			_updateEventRemainTime = _dropBonusEvent != null && RootEventRemainTime != null;
			UIHelper.EnableGameObject(RootEventRemainTime, _updateEventRemainTime);
		}
	}

	private void InitSubHeroModel()
	{
		if (_subHeroInfo != null)
		{
			InitHeroModel(ref _subRenderFacde, _subHeroInfo, TransformSubHero);
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_subRenderFacde != null)
		{
			_subRenderFacde.ChainUpdate();
		}
		if (_subSound != null)
		{
			_subSound.Update();
		}
		UpdateEventRemainTime();
	}

	public override void Close()
	{
		base.Close();
		ReleaseSubHero();
		_dropBonusEvent = null;
		_updateEventRemainTime = false;
	}

	public override void Reset()
	{
		base.Reset();
		ReleaseSubHero();
	}

	private void ReleaseSubHero()
	{
		StopHeroSound(ref _heroSound);
		if ((bool)_subRenderFacde)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(_subRenderFacde);
			_subRenderFacde = null;
		}
	}

	private void UpdateEventRemainTime()
	{
		if (_updateEventRemainTime && Vespa_String_Extension_Method2.TryParseNetworkTime2(_dropBonusEvent.EndTime, out var o_dateTime))
		{
			int num = Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
			string remainTimeLabelText = UIHelper.GetRemainTimeLabelText(num);
			UIHelper.SetLabelText(LabelEventRemainTime, remainTimeLabelText);
		}
	}

	protected override void InitRewardItem(NPCFriendlyInfo npcFriendlyInfo, NPCFriendlyPointData npcFriendlyPointData)
	{
		string text = LocalizationManager.GetString(NPCRecruit.NPC_FRIENDLY_GRADE_REWARD, npcFriendlyPointData.Step);
		UIHelper.SetLabelText(RewardTitleLabel, text);
		FactoryBase.SafeReleaseComponent(ref RewardItemIcon);
		RewardItemIcon = UIHelper.CreateRewardMainItemIconComponent(npcFriendlyPointData.RewardIndex, ItemIconPosition, ItemName);
		bool flag = IsMaxPoint();
		UIHelper.EnableGameObject(ButtonTakeReward, flag);
		UIHelper.EnableGameObject(ButtonGiveGift, !flag);
	}

	public void OnClickOpenHelpList()
	{
		if (!String_Extension_Method.IsNullOrEmpty(_helpListCode))
		{
			EventGuideListPopup.Open(_helpListCode);
		}
	}

	public new void OnClickTakeRewardItem()
	{
		if (_listener == null)
		{
			return;
		}
		NPCFriendlyPointData data = GameRoot.Instance.NPCFriendlyPointDataContainer.GetData(base.NPCFriendlyInfo.HeroIndex, base.NPCFriendlyInfo.Step);
		string effectCode = "";
		if (data != null)
		{
			effectCode = data.EffectCode;
		}
		if (Game3DWidget == null || String_Extension_Method.IsNullOrEmpty(effectCode))
		{
			_listener.OnTakeRewardItem(base.NPCFriendlyInfo.HeroIndex, base.NPCFriendlyInfo.Step, base._friendlyType);
			return;
		}
		TransformData extraTransform = new TransformData();
		extraTransform.Position = new Vector3(0f, 0f, 0f);
		extraTransform.RotationEuler = new Vector3(0f, 0f, 0f);
		extraTransform.Scale = 100f;
		string text = LocalizationManager.GetString(EVENTCHRISTMAS.TITLE_INFO_EVENT_ITEM_LEVEL, base.NPCFriendlyInfo.Step);
		_listener.OnTakeRewardItem(base.NPCFriendlyInfo.HeroIndex, base.NPCFriendlyInfo.Step, text, delegate(Action callbak)
		{
			Game3DWidget.PlayInstantEffect(effectCode, extraTransform, delegate
			{
				Vespa_System_Action_Extension_Method.fire(callbak);
			});
		});
	}

	protected override int CheckReplaceBasicCostume(int costumeIndex)
	{
		return -1;
	}
}
