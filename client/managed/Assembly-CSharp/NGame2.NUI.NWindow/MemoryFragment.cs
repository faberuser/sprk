using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class MemoryFragment : BaseWindow, MemoryFragmentTabComponent.IListener, WorldMapManagement.IAvatarHider
{
	public interface Listener
	{
		void OnEnterMemoryFragmentEntryMenu(string enterNPCCode);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MemoryFragment/MemoryFragment";

	private Listener _listener;

	private List<MemoryFragmentTabComponent> _memoryFragmentTabComponentList = new List<MemoryFragmentTabComponent>();

	private List<MemoryFragmentData> _memoryFragmentDataList = new List<MemoryFragmentData>();

	private int _normalHeroDungeonCnt;

	public UIScrollGrid MemoryFragmentTabGrid;

	public UIBundleLoader HeroTex;

	public UILabel DungeonTitle;

	public UILabel DungeonDesc;

	public MemoryFragmentData CurrentMemoryFragmentData { get; set; }

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		ClearMemoryFragmentTabComponentList();
	}

	public override void ReOpen()
	{
		if (SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.IgnoreCloseNextStep)
		{
			SceneAutoBehaviour<MemoryFragmentManagement>.instance.CurrentMemoryFragmentIndex = -1;
			SceneAutoBehaviour<EntryMenuManagement>.instance.ClearPrevNodeData();
			GlobalAutoBehaviour<GlobalManagement>.instance.UnregisterOpenedWindowStack(this);
			return;
		}
		base.ReOpen();
		if (SceneAutoBehaviour<EntryMenuManagement>.instance.EntryMenuOpenType == EntryMenuManagement.OpenType.UnNodeData)
		{
			VespaCoroutine.Start(coReOpen());
		}
	}

	private IEnumerator coReOpen()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenu(chapterIndex, dungeonIndex, skipAni: true, playNPCSound: false, isIgnoreTrigger: false, delegate
		{
		});
		while (!SceneAutoBehaviour<EntryMenuManagement>.instance.IsLoadingEnterEntryMenu)
		{
			yield return null;
		}
		while (!SceneAutoBehaviour<EntryMenuManagement>.instance.IsEntryMenuInit)
		{
			yield return null;
		}
		SceneAutoBehaviour<MemoryFragmentManagement>.instance.OpenMemoryFragment(CurrentMemoryFragmentData);
		SceneAutoBehaviour<MemoryFragmentManagement>.instance.CurrentMemoryFragmentIndex = -1;
		SceneAutoBehaviour<EntryMenuManagement>.instance.ClearPrevNodeData();
		SceneAutoBehaviour<MemoryFragmentManagement>.instance.EndSceneChanger();
	}

	public void Open(Listener listener, MemoryFragmentData currentMemoryFragmentData = null)
	{
		if (listener == null)
		{
			VespaDebug.LogError("Listener is null");
		}
		base.Open();
		base.gameObject.SetActive(value: true);
		_listener = listener;
		ClearMemoryFragmentTabComponentList();
		CreateMemoryFragmentTab();
		SetCurrentHeroDungeonInfo(currentMemoryFragmentData);
		SceneAutoBehaviour<MemoryFragmentManagement>.instance.EndSceneChanger();
	}

	private void SetCurrentHeroDungeonInfo(MemoryFragmentData currentMemoryFragmentData)
	{
		if (currentMemoryFragmentData != null)
		{
			CurrentMemoryFragmentData = currentMemoryFragmentData;
			SetHeroDungeonInfo(CurrentMemoryFragmentData);
		}
		else if (_memoryFragmentTabComponentList.Count > 0)
		{
			SetHeroDungeonInfo(_memoryFragmentTabComponentList[0]._memoryFragmentData);
		}
	}

	private void CreateMemoryFragmentTab()
	{
		SetMemoryFragmentTabList();
		AddMemoryFragmentTab();
		UIHelper.RepositionGrid(MemoryFragmentTabGrid);
	}

	private void SetMemoryFragmentTabList()
	{
		IList<MemoryFragmentData> allData = GameRoot.Instance.MemoryFragmentDataContainer.GetAllData();
		_normalHeroDungeonCnt = 0;
		foreach (MemoryFragmentData item in allData)
		{
			if (!GetIsCleardDungeon(item))
			{
				_memoryFragmentDataList.Insert(_normalHeroDungeonCnt, item);
				_normalHeroDungeonCnt++;
			}
			else
			{
				_memoryFragmentDataList.Add(item);
			}
		}
	}

	private bool GetIsCleardDungeon(MemoryFragmentData memoryFragmentData)
	{
		NPCData data = GameRoot.Instance.NPCDataContainer.GetData(memoryFragmentData.OpenNpcCode);
		if (data == null)
		{
			return false;
		}
		List<NPCFriendlyPointData> pointDataList = GameRoot.Instance.NPCFriendlyPointDataContainer.GetPointDataList(data.HeroIndex);
		if (pointDataList == null)
		{
			return false;
		}
		int num = pointDataList.Max((NPCFriendlyPointData x) => x.Step);
		NPCFriendlyPointData data2 = GameRoot.Instance.NPCFriendlyPointDataContainer.GetData(data.HeroIndex, num);
		NPCFriendlyInfo nPCFriendlyInfo = GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Get(data.HeroIndex);
		if (nPCFriendlyInfo == null)
		{
			return false;
		}
		if ((nPCFriendlyInfo?.Step ?? 0) == num && nPCFriendlyInfo.EndRewardCount >= data2.EndRewardRepeatCount)
		{
			return true;
		}
		return false;
	}

	private void AddMemoryFragmentTab()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_memoryFragmentDataList); i++)
		{
			MemoryFragmentData memoryFragmentData = _memoryFragmentDataList[i];
			if (memoryFragmentData != null)
			{
				bool isCleared = false;
				if (_normalHeroDungeonCnt <= i)
				{
					isCleared = true;
				}
				MemoryFragmentTabComponent memoryFragmentTabComponent = UIHelper.AddGridItem(MemoryFragmentTabGrid, MemoryFragmentTabComponent.PrefabAuto);
				memoryFragmentTabComponent.Open(this, memoryFragmentData, isCleared);
				_memoryFragmentTabComponentList.Add(memoryFragmentTabComponent);
			}
		}
	}

	void MemoryFragmentTabComponent.IListener.OnClickTab(MemoryFragmentTabComponent caller)
	{
		MemoryFragmentData memoryFragmentData = caller._memoryFragmentData;
		if (memoryFragmentData == null)
		{
			VespaDebug.LogError("memoryFragmentData(tableData) is null");
		}
		else
		{
			SetHeroDungeonInfo(memoryFragmentData);
		}
	}

	private void SetHeroDungeonInfo(MemoryFragmentData memoryFragmentData)
	{
		if (memoryFragmentData == null)
		{
			VespaDebug.LogError("memoryFragmentData(tableData) is null");
			return;
		}
		CurrentMemoryFragmentData = memoryFragmentData;
		UIHelper.SetBundleTexture(HeroTex, memoryFragmentData.PortraitPath);
		UIHelper.SetLabelText(DungeonTitle, LocalizationManager.GetStringDirect(memoryFragmentData.DungeonName));
		UIHelper.SetLabelText(DungeonDesc, LocalizationManager.GetStringDirect(memoryFragmentData.DungeonDesc));
		foreach (MemoryFragmentTabComponent memoryFragmentTabComponent in _memoryFragmentTabComponentList)
		{
			if (memoryFragmentTabComponent._memoryFragmentData == memoryFragmentData)
			{
				memoryFragmentTabComponent.SelectToggle(isSelect: true);
			}
		}
	}

	public void OnClickEnterHeroDungeon()
	{
		string openNpcCode = CurrentMemoryFragmentData.OpenNpcCode;
		if (String_Extension_Method.IsNullOrEmpty(openNpcCode))
		{
			VespaDebug.LogError("currentNPCCode is null");
			return;
		}
		if (_listener == null)
		{
			VespaDebug.LogError("Listener is null");
			return;
		}
		SceneAutoBehaviour<MemoryFragmentManagement>.instance.CurrentMemoryFragmentIndex = CurrentMemoryFragmentData.Index;
		_listener.OnEnterMemoryFragmentEntryMenu(openNpcCode);
	}

	private void ClearMemoryFragmentTabComponentList()
	{
		FactoryBase.ReleaseAndClearList(_memoryFragmentTabComponentList);
		_memoryFragmentDataList.Clear();
	}

	public override void OnClosing()
	{
		CurrentMemoryFragmentData = null;
		base.OnClosing();
	}

	bool WorldMapManagement.IAvatarHider.IsOpen()
	{
		return base.IsOpen();
	}

	WorldMapManagement.AvatarHideFlag WorldMapManagement.IAvatarHider.GetAvatarHideFlag()
	{
		return WorldMapManagement.AvatarHideFlag.HideAll;
	}
}
