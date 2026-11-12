using System.Collections.Generic;
using NGame2.NEventTrigger;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NSoulWeapon;
using NGameAsset;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

public class ContentUIGodkingTrialDungeonList : BaseWindow, ContentUIGodkingTrialDungeonListComponent.IListener
{
	public const string PrefabPath = "Resources/Prefab2/Window/GodkingTrial/ContentUIGodkingTrialDungeonList";

	public UISimpleGrid SimpleGridDungeonList;

	private List<ContentUIGodkingTrialDungeonListComponent> _compList = new List<ContentUIGodkingTrialDungeonListComponent>();

	public const string UI_GodKing_BGM = "EntryMenu.KainAscensionBGM";

	private string _beforeBGM = string.Empty;

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_compList);
	}

	public override void Open()
	{
		base.Open();
		OpenTrialofGodking triggerEvent = new OpenTrialofGodking();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
		Init();
		RestoredPortal.GodkingWindowBackground(this);
	}

	private void Init()
	{
		SetDungeonList();
		PlayBGM();
	}

	private void SetDungeonList()
	{
		IList<GodkingTrialGroupData> allData = GameRoot.Instance.GodkingTrialGroupDataContainer.GetAllData();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			GodkingTrialGroupData godkingTrialGroupData = allData[i];
			if (godkingTrialGroupData != null)
			{
				ContentUIGodkingTrialDungeonListComponent contentUIGodkingTrialDungeonListComponent = UIHelper.AddGridItem(SimpleGridDungeonList, ContentUIGodkingTrialDungeonListComponent.PrefabAuto);
				contentUIGodkingTrialDungeonListComponent.Open(godkingTrialGroupData, this);
				_compList.Add(contentUIGodkingTrialDungeonListComponent);
			}
		}
		SimpleGridDungeonList.RepositionGrid();
	}

	private void PlayBGM()
	{
		SingletonAuto<BgmManager2>.instance.PendingRequest = false;
		_beforeBGM = SingletonAuto<BgmManager2>.instance.GetCurrentEventAssetBgmCode();
		if (_beforeBGM != "EntryMenu.KainAscensionBGM")
		{
			SingletonAuto<BgmManager2>.instance.Request("EntryMenu.KainAscensionBGM", isEvent: true);
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		if (SceneAutoBehaviour<SoulWeaponManagement>.isValidInstance && SceneAutoBehaviour<SoulWeaponManagement>.instance.IsOpened<SoulWeaponLobby>())
		{
			RestoredPortal.GodkingReturnToPortal();
			return;
		}
		if (SceneAutoBehaviour<EntryMenuManagement>.isValidInstance && SceneAutoBehaviour<EntryMenuManagement>.instance.IsOpenedAny())
		{
			RestoredPortal.GodkingReturnToPortal();
			return;
		}
		SingletonAuto<BgmManager2>.instance.StopEvent("EntryMenu.KainAscensionBGM");
		if (_beforeBGM != "EntryMenu.KainAscensionBGM")
		{
			SingletonAuto<BgmManager2>.instance.Request(_beforeBGM, isEvent: true);
		}
		RestoredPortal.GodkingReturnToPortal();
	}

	void ContentUIGodkingTrialDungeonListComponent.IListener.OnOpening(ContentUIGodkingTrialDungeonListComponent thisComp)
	{
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(_compList); i < num; i++)
		{
			ContentUIGodkingTrialDungeonListComponent contentUIGodkingTrialDungeonListComponent = _compList[i];
			if (!(contentUIGodkingTrialDungeonListComponent == null) && !(contentUIGodkingTrialDungeonListComponent == thisComp))
			{
				contentUIGodkingTrialDungeonListComponent.LockingGate();
				contentUIGodkingTrialDungeonListComponent.UpdateButtonState();
			}
		}
	}

	void ContentUIGodkingTrialDungeonListComponent.IListener.OnClosing(ContentUIGodkingTrialDungeonListComponent thisComp)
	{
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(_compList); i < num; i++)
		{
			ContentUIGodkingTrialDungeonListComponent contentUIGodkingTrialDungeonListComponent = _compList[i];
			if (!(contentUIGodkingTrialDungeonListComponent == null) && !(contentUIGodkingTrialDungeonListComponent == thisComp))
			{
				contentUIGodkingTrialDungeonListComponent.UnlockingGate();
				contentUIGodkingTrialDungeonListComponent.UpdateButtonState();
			}
		}
	}
}
