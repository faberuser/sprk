using System.Collections.Generic;
using NGame2.NScene.NLobbyScenePrerequisites;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NShared.NUtil;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class MazeTowerInfoPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MazeTowerInfoPopup";

	public UILabel Label_Name;

	public UILabel Label_EnterRestrict;

	public UILabel Label_Info;

	public UILabel Label_InfoDesc;

	public UIBaseGrid BuffIconGrid;

	private TowerData _towerData;

	private MazeInfoData _mazeInfoData;

	private List<SkillIconComponent> _buffIcons = new List<SkillIconComponent>();

	public static void Open(TowerData towerData)
	{
		WindowPrefab<MazeTowerInfoPopup> windowPrefab = new WindowPrefab<MazeTowerInfoPopup>("Resources/Prefab2/Window/MessagePopup/MazeTowerInfoPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(towerData);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_buffIcons);
	}

	public override void OnSystemBackButton()
	{
		Close();
	}

	private void OpenInternal(TowerData towerData)
	{
		base.Open();
		_towerData = towerData;
		_mazeInfoData = GameRoot.Instance.MazeInfoDataContainer.GetData(towerData.TowerIndex);
		SetLabels();
		SetBuffIcons();
	}

	public void SetLabels()
	{
		if (_mazeInfoData != null)
		{
			UIHelper.SetLabelText(Label_Name, LocalizationManager.GetStringDirect(_mazeInfoData.TowerNameKey));
			string text = LocalizationManager.GetString(_mazeInfoData.ClassType);
			UIHelper.SetLabelText(Label_EnterRestrict, LocalizationManager.GetStringDirect("COMMON_REQ", text));
			UIHelper.SetLabelText(Label_Info, LocalizationManager.GetStringDirect(_mazeInfoData.BattleInfoTitleKey));
			UIHelper.SetLabelText(Label_InfoDesc, LocalizationManager.GetStringDirect(_mazeInfoData.BattleInfoDescKey));
		}
	}

	public void SetBuffIcons()
	{
		if (_towerData == null)
		{
			return;
		}
		List<TowerFloorData> floors = GameRoot.Instance.TowerFloorDataContainer.GetFloors(_towerData.TowerIndex);
		int monthFromMillennium = RequestEnterLobbyWrapper.MonthFromMillennium;
		List<int> list = new List<int>();
		foreach (TowerFloorData item in floors)
		{
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(item.ChapterIndex, item.DungeonIndex);
			if (data != null)
			{
				int randomByMonth = RandomTable.GetRandomByMonth(monthFromMillennium, item.DungeonIndex);
				int randomProjectileGroupIndex = data.GetRandomProjectileGroupIndex(randomByMonth);
				if (randomProjectileGroupIndex != -1 && !list.Contains(randomProjectileGroupIndex))
				{
					list.Add(randomProjectileGroupIndex);
				}
			}
		}
		List<int> list2 = new List<int>();
		foreach (int item2 in list)
		{
			foreach (SkillProjectileGroupData.SkillProjectileInfo skillProjectileInfo in GameRoot.Instance.SkillProjectileGroupDataContainer.GetData(item2).GetSkillProjectileInfos())
			{
				SkillProjectileData data2 = GameRoot.Instance.SkillProjectileDataContainer.GetData(skillProjectileInfo.Index);
				if (data2 != null)
				{
					SkillData data3 = GameRoot.Instance.SkillDataContainer.GetData(data2.SkillIndex);
					if (data3 != null && !list2.Contains(data3.Index))
					{
						list2.Add(data3.Index);
					}
				}
			}
		}
		foreach (int item3 in list2)
		{
			SkillIconComponent skillIconComponent = BuffIconGrid.AddGridItem(SkillIconComponent.PrefabAuto);
			skillIconComponent.Open(item3);
			_buffIcons.Add(skillIconComponent);
		}
		BuffIconGrid.RepositionGrid();
	}
}
