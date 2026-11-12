using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NCutScene;
using NGame2.NGameSetting;
using NGame2.NGuild;
using NGame2.NScene;
using NGame2.NTutorial;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUI.NWorldMap.NMapLayout.NBuilding;
using NShared;
using NVespa;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class BuildingManager : ITouchDisabler
{
	private enum EUpgradeBuildingState
	{
		None,
		ZoomIn,
		ZoomOut
	}

	private Mediator _myMediator;

	private Transform _root;

	private Coroutine _reservedUpgradeCoroutine;

	private int _reservedUpgradeBuildingIndex;

	private bool _startUpgradeAction;

	private List<IBuilding> _buildingList = new List<IBuilding>();

	private EUpgradeBuildingState _upgaradeState;

	private bool _isShowOrHideTaks;

	bool ITouchDisabler.IsValid => true;

	public void Open(Mediator mediator, Transform root)
	{
		_myMediator = mediator;
		_root = root;
		SetupLayer(GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D).Layer, _root.gameObject);
	}

	public void OnLoadMap()
	{
		CloseInternal();
		RefreshBuild();
	}

	public void OnUnLoadMap()
	{
		CloseInternal();
	}

	public void Close()
	{
		CloseInternal();
	}

	public void CloseInternal()
	{
		_buildingList.ForEach(delegate(IBuilding e)
		{
			e.Close();
		});
		_buildingList.Clear();
	}

	public void ChainUpdate()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_buildingList); i++)
		{
			IBuilding building = _buildingList[i];
			if (building == null)
			{
				continue;
			}
			if (building.ReservedUpgrade)
			{
				MapLayout instance = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
				if (instance != null)
				{
					instance.PropManager.ChangedPropPrefabList();
				}
				UpgradeBuildingInternal(building.Index);
				building.CompleteReservedUpgrade();
			}
			building.Update();
		}
	}

	private IBuilding CreateBuilding(WorldMapBuildData buildData)
	{
		if (buildData == null)
		{
			return null;
		}
		IBuilding building = SingletonAuto<BuildingContainer>.instance.GetBuilding(buildData.BuildingType);
		if (building == null)
		{
			return null;
		}
		GameObject gameObject = CreateBuildingRoot(buildData);
		return building.Create(buildData, gameObject.transform, _myMediator.MapComponent.Layout.Render3dAngle);
	}

	private void RefreshBuild()
	{
		List<IBuilding> list = new List<IBuilding>();
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		IList<WorldMapBuildData> buildDataOfChapter = GameRoot.Instance.WorldMapBuildDataContainer.GetBuildDataOfChapter(chapterIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(buildDataOfChapter); i++)
		{
			WorldMapBuildData worldMapBuildData = buildDataOfChapter[i];
			if (worldMapBuildData != null)
			{
				IBuilding building = CreateBuilding(worldMapBuildData);
				if (building != null)
				{
					list.Add(building);
				}
			}
		}
		_buildingList = list;
	}

	private GameObject CreateBuildingRoot(WorldMapBuildData buildData)
	{
		if (buildData == null)
		{
			return null;
		}
		GameObject gameObject = new GameObject("WorldMap-Buildng");
		SetupTranslateBuildngRoot(buildData.PositionX, buildData.PositionY, gameObject);
		SetupRotateBuildingRoot(buildData.Angle, gameObject);
		SetupScaleBuildingRoot(buildData.Scale, gameObject);
		SetupLayer(_root.gameObject.layer, gameObject);
		return gameObject;
	}

	public void SetupRotateBuildingRoot(int angle, GameObject BuildingRoot)
	{
		Vector3 forward = Quaternion.AngleAxis(angle, Vector3.up) * BuildingRoot.transform.forward;
		BuildingRoot.transform.forward = forward;
	}

	public void SetupTranslateBuildngRoot(float posX, float posY, GameObject buildingRoot)
	{
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(buildingRoot.transform, _root);
		Vector3 position = new Vector3(posX, posY);
		Vector3 position2 = _myMediator.Layout.IndicatorRoot.TransformPoint(position);
		buildingRoot.transform.position = position2;
	}

	public void SetupScaleBuildingRoot(float scale, GameObject buildngRoot)
	{
		buildngRoot.transform.localScale *= scale;
	}

	public void SetupLayer(int layer, GameObject go)
	{
		go.layer = layer;
	}

	public void UpgradeBuildingInternal(int buildingIndex)
	{
		CancelBuildingUpgrade();
		_reservedUpgradeCoroutine = VespaCoroutine.Start(coUpgradeBuilding(buildingIndex));
	}

	private void CancelBuildingUpgrade()
	{
		if (_reservedUpgradeCoroutine != null && _upgaradeState == EUpgradeBuildingState.None)
		{
			VespaCoroutine.Stop(_reservedUpgradeCoroutine);
			IBuilding buildingByIndex = GetBuildingByIndex(_reservedUpgradeBuildingIndex);
			_reservedUpgradeCoroutine = null;
			_reservedUpgradeBuildingIndex = 0;
			buildingByIndex?.UpgradeCancel();
		}
	}

	private bool IsCancelUpgrade(IBuilding building)
	{
		if (building == null)
		{
			return true;
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		if (!GameRoot.Instance.WorldMapBuildDataContainer.IsBuildingContainsByChatper(chapterIndex))
		{
			return true;
		}
		if (!building.HaveUpgradeAnimationKey)
		{
			return true;
		}
		return false;
	}

	private bool IsFocusOnBuilding()
	{
		if (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			return false;
		}
		if (SceneAutoBehaviour<WorldMapManagement>.instance.IsMapLoading)
		{
			return false;
		}
		if (SingletonUpdater<TutorialManager>.isValidInstance)
		{
			if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
			{
				return false;
			}
			if (SingletonUpdater<TutorialManager>.instance.HasNextTurorial)
			{
				return false;
			}
		}
		if (!AchievementClearedPopupQueue.Empty() || AchievementClearedPopupQueue.IsProgressing())
		{
			return false;
		}
		if (!SceneAutoBehaviour<LobbyPopupQueue>.instance.Empty() || SceneAutoBehaviour<LobbyPopupQueue>.instance.IsProgressing())
		{
			return false;
		}
		if (SceneAutoBehaviour<RaidManagement>.isValidInstance && SceneAutoBehaviour<RaidManagement>.instance.IsOpenedAny())
		{
			return false;
		}
		if (SceneAutoBehaviour<GuildSuppressManagement>.isValidInstance && SceneAutoBehaviour<GuildSuppressManagement>.instance.IsOpenedAny())
		{
			return false;
		}
		if (SceneAutoBehaviour<EntryMenuManagement>.instance.IsOpenedAny())
		{
			return false;
		}
		if (SceneAutoBehaviour<CutSceneManager>.isValidInstance && SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying)
		{
			return false;
		}
		if (_upgaradeState != EUpgradeBuildingState.None)
		{
			return false;
		}
		if (_isShowOrHideTaks)
		{
			return false;
		}
		return true;
	}

	private void SetUpgradeState(EUpgradeBuildingState state)
	{
		_upgaradeState = state;
	}

	private IEnumerator coUpgradeBuilding(int buildingIndex)
	{
		IBuilding building = GetBuildingByIndex(buildingIndex);
		if (IsCancelUpgrade(building))
		{
			building.UpgradeCancel();
			_reservedUpgradeCoroutine = null;
			yield break;
		}
		building.UpgradePrepare();
		while (!IsFocusOnBuilding())
		{
			yield return null;
		}
		_reservedUpgradeBuildingIndex = buildingIndex;
		GlobalTopmostWindow.instance.AddTouchDisabler(this);
		bool completeOpenGuildBuilding = false;
		GlobalAutoBehaviour<GuildManager>.instance.RequestOpenGuildBuilding(showUpgradeAnim: false, delegate
		{
			completeOpenGuildBuilding = true;
		});
		while (!completeOpenGuildBuilding)
		{
			yield return null;
		}
		bool zoomInCompleted = false;
		MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		mapLayout.MapViewer.FocusBuildngMode(0.9f, building.Index, delegate
		{
			zoomInCompleted = true;
		});
		SetUpgradeState(EUpgradeBuildingState.ZoomIn);
		while (!zoomInCompleted)
		{
			yield return null;
		}
		float frameLength = 2f;
		float duraiton = 0f;
		building.Upgrade();
		while (duraiton < frameLength)
		{
			duraiton += Time.deltaTime;
			yield return null;
		}
		SetUpgradeState(EUpgradeBuildingState.ZoomOut);
		bool zoomOutCompleted = false;
		mapLayout.MapViewer.ReleaseFocusNode(delegate
		{
			zoomOutCompleted = true;
		});
		while (!zoomOutCompleted)
		{
			yield return null;
		}
		building.UpgradeComplete();
		GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
		SetUpgradeState(EUpgradeBuildingState.None);
		_reservedUpgradeCoroutine = null;
		_reservedUpgradeBuildingIndex = 0;
	}

	public Vector3 GetPositionByIndex(int index)
	{
		return GetBuildingByIndex(index)?.Position ?? Vector3.zero;
	}

	private IBuilding GetBuildingByIndex(int buildingIndex)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_buildingList); i++)
		{
			if (_buildingList[i].Index == buildingIndex)
			{
				return _buildingList[i];
			}
		}
		return null;
	}

	public ITask BuildingShowTask()
	{
		EntryMenuData entryMenu = GameSetting.data.EntryMenu;
		SequenceTask sequenceTask = new SequenceTask();
		sequenceTask.AddTask(coBuildingShowOrHide(isShow: true, entryMenu.BlendTimeLength));
		return sequenceTask;
	}

	public ITask BuildingHideTask(EntryMenuSettingData menuAppearance)
	{
		SequenceTask sequenceTask = new SequenceTask();
		sequenceTask.AddTask(coBuildingShowOrHide(isShow: false, menuAppearance.NpcSlideTimeLength));
		return sequenceTask;
	}

	private IEnumerator coBuildingShowOrHide(bool isShow, float duration)
	{
		_isShowOrHideTaks = true;
		if (duration == 0f)
		{
			float result = (isShow ? 1f : 0f);
			_isShowOrHideTaks = false;
			_buildingList.ForEach(delegate(IBuilding building)
			{
				building.SetExtraAlpha(result);
			});
			yield break;
		}
		float elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += Time.deltaTime;
			float num = Mathf.Clamp01(elapsedTime / duration);
			float offset = (isShow ? num : (1f - num));
			_buildingList.ForEach(delegate(IBuilding building)
			{
				building.SetExtraAlpha(offset);
				building.Update();
			});
		}
		while (!(elapsedTime >= duration));
		_isShowOrHideTaks = false;
	}
}
