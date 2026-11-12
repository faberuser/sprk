using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class KingsPassFrameComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/KingsPass/KingsPassFrameComponent";

	public static PrefabAuto<KingsPassFrameComponent> PrefabAuto = new PrefabAuto<KingsPassFrameComponent>("Prefab2/Component/KingsPass/KingsPassFrameComponent");

	public const string FinalPrefabPath = "Prefab2/Component/KingsPass/FinalKingsPassFrameComponent";

	public static PrefabAuto<KingsPassFrameComponent> FinalPrefabAuto = new PrefabAuto<KingsPassFrameComponent>("Prefab2/Component/KingsPass/FinalKingsPassFrameComponent");

	public UILabel LevelLabel;

	public UISprite LockedSprite;

	public UIBaseGrid KingsPassGrid;

	public UIBaseGrid AdditionalGrid;

	private KingsPassItemComponent.Listener _listener;

	private List<KingsPassItemComponent> _compList = new List<KingsPassItemComponent>();

	private List<KingsPassItemComponent> _additionalCompList = new List<KingsPassItemComponent>();

	public int SeasonIndex { get; private set; }

	public int Level { get; private set; }

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		ReleaseComponent();
	}

	public void Open(int seasonIndex, int level, KingsPassItemComponent.Listener listener)
	{
		SeasonIndex = seasonIndex;
		Level = level;
		_listener = listener;
		Init();
	}

	private void Init()
	{
		SetLevelLabel();
		SetLocked();
		SetKingsPassItemComp();
		SetAdditionalItemComp();
	}

	private void SetLevelLabel()
	{
		string stringDirect = LocalizationManager.GetStringDirect("KINGSPASS_LEVEL_TEXT", Level);
		UIHelper.SetLabelText(LevelLabel, stringDirect);
	}

	private void SetLocked()
	{
		int num = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.KingsPassSeaonInfo?.CurrentLevel ?? 1;
		bool enable = Level > num;
		UIHelper.EnableGameObject(LockedSprite, enable);
	}

	private void SetKingsPassItemComp()
	{
		KingsPassSeasonMissionData dataByRewardKey = GameRoot.Instance.KingsPassSeasonMissionDataContainer.GetDataByRewardKey(SeasonIndex, KingsPassType.KingsPass, Level);
		if (dataByRewardKey != null)
		{
			KingsPassMission kingsPassMission = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Get(new KingsPassMissionKey(dataByRewardKey.SeasonIndex, dataByRewardKey.MissionIndex));
			KingsPassItemComponent kingsPassItemComponent = UIHelper.AddGridItem(KingsPassGrid, KingsPassItemComponent.PrefabAuto);
			kingsPassItemComponent.Open(kingsPassMission, dataByRewardKey, _listener);
			_compList.Add(kingsPassItemComponent);
			UIHelper.RepositionGrid(KingsPassGrid);
		}
	}

	private void SetAdditionalItemComp()
	{
		KingsPassSeasonMissionData dataByRewardKey = GameRoot.Instance.KingsPassSeasonMissionDataContainer.GetDataByRewardKey(SeasonIndex, KingsPassType.PremiumPass, Level);
		if (dataByRewardKey != null)
		{
			KingsPassMission kingsPassMission = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Get(new KingsPassMissionKey(dataByRewardKey.SeasonIndex, dataByRewardKey.MissionIndex));
			KingsPassItemComponent kingsPassItemComponent = UIHelper.AddGridItem(AdditionalGrid, KingsPassItemComponent.PrefabAuto);
			kingsPassItemComponent.Open(kingsPassMission, dataByRewardKey, _listener);
			_additionalCompList.Add(kingsPassItemComponent);
		}
		KingsPassSeasonMissionData dataByRewardKey2 = GameRoot.Instance.KingsPassSeasonMissionDataContainer.GetDataByRewardKey(SeasonIndex, KingsPassType.AdditionalPass, Level);
		if (dataByRewardKey2 != null)
		{
			KingsPassMission kingsPassMission2 = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Get(new KingsPassMissionKey(dataByRewardKey2.SeasonIndex, dataByRewardKey2.MissionIndex));
			KingsPassItemComponent kingsPassItemComponent2 = UIHelper.AddGridItem(AdditionalGrid, KingsPassItemComponent.PrefabAuto);
			kingsPassItemComponent2.Open(kingsPassMission2, dataByRewardKey2, _listener);
			_additionalCompList.Add(kingsPassItemComponent2);
		}
		UIHelper.RepositionGrid(AdditionalGrid);
	}

	private void ReleaseComponent()
	{
		FactoryBase.ReleaseAndClearList(_compList);
		FactoryBase.ReleaseAndClearList(_additionalCompList);
	}

	public void Refresh()
	{
		foreach (KingsPassItemComponent comp in _compList)
		{
			comp.Refresh();
		}
		foreach (KingsPassItemComponent additionalComp in _additionalCompList)
		{
			additionalComp.Refresh();
		}
	}

	public bool EnabledReceive()
	{
		if (CheckEnableRecievedList(_compList))
		{
			return true;
		}
		if (CheckEnableRecievedList(_additionalCompList))
		{
			return true;
		}
		return false;
	}

	private bool CheckEnableRecievedList(List<KingsPassItemComponent> listComp)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(listComp); i++)
		{
			if (CheckEnableRecieved(listComp[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckEnableRecieved(KingsPassItemComponent comp)
	{
		if (comp == null)
		{
			return false;
		}
		return comp.EnabledReceive();
	}
}
