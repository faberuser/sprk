using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NAffix;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NPayShop;
using NGame2.NUtil;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PunishmentRaidWindow : RenewResourceWindow, PunishmentRaidComponent.Listener
{
	public interface IListener
	{
		void OpenRaidRewardPopup(int raidGroupIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Raid/PunishmentRaidWindow";

	public Transform MapRoot;

	public UIGame3DWidget BackgroundWidget;

	public UIGame3DWidget BakgroundLoopEffect;

	public UILabel Label_OpenCount;

	public UILabel Label_Guidance;

	private PunishmentGroupData _bossData;

	private int _groupIndex;

	private int _level;

	private IListener _listener;

	private List<PunishmentRaidComponent> _compList = new List<PunishmentRaidComponent>();

	private List<PunishmentBossPortraitComponent> _compPortraitList = new List<PunishmentBossPortraitComponent>();

	private const string OPEN_EFFECT_CODE = "UI.Punishment.{0}.Open";

	private const string LOOP_EFFECT_CODE = "UI.Punishment.{0}.Loop";

	private string _beforeBGM = string.Empty;

	private string _currentBgm = string.Empty;

	public void Open(PunishmentGroupData bossData, IListener listener)
	{
		OpenPunishmentRaidInfo openPunishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(_groupIndex, EPunishmentDungeonType.Boss);
		if (openPunishmentRaidInfo != null && openPunishmentRaidInfo.IsOpen != 0)
		{
			Open(delegate
			{
				_bossData = bossData;
				_groupIndex = bossData.GroupIndex;
				_listener = listener;
				Init();
			}, null);
		}
	}

	public void Open(int raidGroupIndex, IListener listener)
	{
		Open(delegate
		{
			_groupIndex = raidGroupIndex;
			_bossData = GameRoot.Instance.PunishmentGroupDataContainer.GetData(_groupIndex, EPunishmentDungeonType.Boss);
			_listener = listener;
			Init();
		}, null);
	}

	public void Open(int raidGroupIndex, int level, IListener listener)
	{
		Open(delegate
		{
			_groupIndex = raidGroupIndex;
			_bossData = GameRoot.Instance.PunishmentGroupDataContainer.GetData(_groupIndex, EPunishmentDungeonType.Boss);
			_listener = listener;
			_level = level;
			Init();
		}, null);
	}

	private new void Init()
	{
		base.Init();
		SetBackground();
		SetEntryComponent();
		SetOpenLevel();
		PlayBGM();
	}

	public void PlayBGM()
	{
		SingletonAuto<BgmManager2>.instance.PendingRequest = false;
		_currentBgm = _bossData.BossRaidBgm;
		if (!String_Extension_Method.IsNullOrEmpty(_currentBgm))
		{
			_beforeBGM = SingletonAuto<BgmManager2>.instance.GetCurrentEventAssetBgmCode();
			if (_beforeBGM != _currentBgm)
			{
				SingletonAuto<BgmManager2>.instance.Request(_currentBgm, isEvent: true);
			}
		}
	}

	public void SetBackground()
	{
		if (_bossData == null)
		{
			return;
		}
		string[] backgroundPaths = _bossData.BackgroundPaths;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(backgroundPaths))
		{
			return;
		}
		BackgroundWidget.EntryList.Clear();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(backgroundPaths); i++)
		{
			string text = backgroundPaths[i];
			if (!String_Extension_Method.IsNullOrEmpty(text))
			{
				UIGame3DWidget.EntryData entryData = new UIGame3DWidget.EntryData
				{
					ContentsType = UIGame3DWidget.EContentsType.Effect,
					EventCode = text,
					ExtraTransform = new TransformData(538f)
				};
				BackgroundWidget.AddEntryList(entryData);
			}
		}
		BackgroundWidget.PlayNow();
		string stringDirect = LocalizationManager.GetStringDirect(_bossData.BossGuidanceKey);
		UIHelper.SetLabelText(Label_Guidance, stringDirect);
	}

	public void SetEntryComponent()
	{
		IList<PunishmentRaidData> punishmentRaidDatas = GameRoot.Instance.PunishmentRaidDataContainer.GetPunishmentRaidDatas(_groupIndex);
		TransformData extraTransform = (__Extension_Method_ICollection__.IsNullOrEmpty(BackgroundWidget.EntryList) ? new TransformData(538f) : BackgroundWidget.EntryList[0].ExtraTransform);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(punishmentRaidDatas); i++)
		{
			PunishmentRaidData punishmentRaidData = punishmentRaidDatas[i];
			if (punishmentRaidData != null)
			{
				PunishmentRaidComponent punishmentRaidComponent = PunishmentRaidComponent.PrefabAuto.Factory.Create(MapRoot);
				punishmentRaidComponent.Open(punishmentRaidData, this, _groupIndex);
				PunishmentBossPortraitComponent punishmentBossPortraitComponent = (punishmentRaidData.IsBoss ? PunishmentBossPortraitComponent.BossPrefabAuto.Factory.Create(MapRoot) : PunishmentBossPortraitComponent.MonsterPrefabAuto.Factory.Create(MapRoot));
				punishmentBossPortraitComponent.Open(punishmentRaidData);
				bool alreadyClear = true;
				PunishmentRaidInfo punishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.GetPunishmentRaidInfo(_groupIndex, punishmentRaidData.RaidIndex, punishmentRaidData.RaidLevel);
				if (punishmentRaidInfo == null || punishmentRaidInfo.ClearCheckIndex == 0 || String_Extension_Method.IsNullOrEmpty(punishmentRaidInfo.ClearedTime))
				{
					string effectCode = $"UI.Punishment.{punishmentRaidData.EntryEffectCode}.Loop";
					BakgroundLoopEffect.PlayInstantEffect(effectCode, extraTransform, null);
					alreadyClear = false;
				}
				punishmentBossPortraitComponent.SetAlreadyClear(alreadyClear);
				_compPortraitList.Add(punishmentBossPortraitComponent);
				_compList.Add(punishmentRaidComponent);
			}
		}
	}

	private void SetOpenLevel()
	{
		OpenPunishmentRaidInfo openPunishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.Get(_groupIndex, EPunishmentDungeonType.Boss);
		if (openPunishmentRaidInfo != null)
		{
			UIHelper.SetLabelText(Label_OpenCount, openPunishmentRaidInfo.OpenLevel);
		}
	}

	private bool GetFullCondition(int[] triggerIndices)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(triggerIndices); i++)
		{
			PunishmentRaidTriggerData data = GameRoot.Instance.PunishmentRaidTriggerDataContainer.GetData(triggerIndices[i]);
			if (!SingletonAuto<ConditionChecker>.instance.Check(data.Condition))
			{
				return false;
			}
		}
		return true;
	}

	private void ReleaseComp()
	{
		FactoryBase.ReleaseAndClearList(_compList);
		FactoryBase.ReleaseAndClearList(_compPortraitList);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		BakgroundLoopEffect.Stop(immediatelyClear: true);
		BackgroundWidget.Stop(immediatelyClear: true);
		ReleaseComp();
		if (!String_Extension_Method.IsNullOrEmpty(_currentBgm))
		{
			SingletonAuto<BgmManager2>.instance.StopEvent(_currentBgm);
			if (!String_Extension_Method.IsNullOrEmpty(_beforeBGM) && _beforeBGM != _currentBgm)
			{
				SingletonAuto<BgmManager2>.instance.Request(_beforeBGM, isEvent: true);
				_beforeBGM = null;
			}
			_currentBgm = null;
		}
	}

	public void OnClickRaidRewardPopup()
	{
		if (_listener != null)
		{
			_listener.OpenRaidRewardPopup(_groupIndex);
		}
	}

	void PunishmentRaidComponent.Listener.OnClickEnterButton(PunishmentRaidComponent Selected)
	{
		if (Selected.PunishmentRaidData == null)
		{
			return;
		}
		PunishmentRaidData punishmentRaidData = Selected.PunishmentRaidData;
		int raidIndex = punishmentRaidData.RaidIndex;
		int raidLevel = punishmentRaidData.RaidLevel;
		int[] affixIndices = AffixHelper.GetPunishmentAffixIndices(punishmentRaidData);
		PunishmentRaidInfo punishmentRaidInfo = GlobalAutoBehaviour<AccountManager>.instance.OpenPunishmentRaidManager.GetPunishmentRaidInfo(_groupIndex, punishmentRaidData.RaidIndex, punishmentRaidData.RaidLevel);
		if (punishmentRaidInfo != null && punishmentRaidInfo.ClearCheckIndex != 0 && !String_Extension_Method.IsNullOrEmpty(punishmentRaidInfo.ClearedTime))
		{
			return;
		}
		if (!SingletonAuto<ConditionChecker>.instance.Check(punishmentRaidData.CanEnterCondition))
		{
			MessagePopup.OpenInform(LocalizationManager.GetStringDirect("PUNISHMENTRAID_ENTRANCE_BLOCKED "));
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect(punishmentRaidData.EntryLocalKey);
		string message = ((!punishmentRaidData.IsBoss || !GetFullCondition(punishmentRaidData.TriggerIndices)) ? LocalizationManager.GetStringDirect("PUNISHMENTRAID_ENTARANCE_MESSAGE", stringDirect) : LocalizationManager.GetStringDirect(punishmentRaidData.WarningLocalKey));
		RaidData raidData = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
		Action onButton = delegate
		{
			SceneAutoBehaviour<PunishManagment>.instance.OpenPunishmentRaidPartySetting(raidData, _bossData, affixIndices);
		};
		PayShopMessage3ButtonPopup.Open(stringDirect, message, onButton);
	}

	void PunishmentRaidComponent.Listener.OnClickInfoButton(PunishmentRaidComponent Selected)
	{
		if (Selected.PunishmentRaidData != null)
		{
			int raidIndex = Selected.PunishmentRaidData.RaidIndex;
			int raidLevel = Selected.PunishmentRaidData.RaidLevel;
			RaidData data = GameRoot.Instance.RaidDataContainer.GetData(raidIndex, raidLevel);
			SceneAutoBehaviour<RaidManagement>.instance.OpenRaidInfo(data);
		}
	}
}
