using System.Collections;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTower;

public class PitDungeonInfo : BaseWindow, IInspectorGuiDrawer
{
	public interface Listener
	{
		void OnClickBattleReady();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Tower/PitDungeonInfo";

	[HideInInspector]
	public BaseBoss3dView Boss3dView;

	private Listener _listener;

	private SceneChanger _sceneChanger;

	public TowerData TowerData { get; private set; }

	public TowerFloorData FloorData { get; private set; }

	public bool Init(TowerData towerData, TowerFloorData floorData, int hpPercentage, Listener listener)
	{
		_listener = listener;
		TowerData = towerData;
		FloorData = floorData;
		int chapterIndex = FloorData.ChapterIndex;
		int dungeonIndex = FloorData.DungeonIndex;
		Boss3dView.Open(chapterIndex, dungeonIndex, "PitBoss", CreatureStock.ELoadLevel.IncludeFacial);
		return true;
	}

	public void onOpened()
	{
		Boss3dView.Init();
		if (_sceneChanger != null)
		{
			_sceneChanger.Close();
			_sceneChanger = null;
		}
	}

	public ITask SceneChangeTask()
	{
		return new SequenceTask().ADDTASK(new DelegateTask(delegate
		{
			_sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		})).ADDTASK(new WaitTask(() => !_sceneChanger.IsCompleteAnimation()));
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		Boss3dView.Update();
	}

	public override void OnSystemBackButton()
	{
		VespaCoroutine.Start(coClose(this));
	}

	private static IEnumerator coClose(PitDungeonInfo This)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(5f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		if (This != null && This.IsOpen())
		{
			This.Close();
		}
		sceneChanger.Close();
	}

	private void Clear()
	{
		Boss3dView.Clear();
	}

	public void OnClickBattleReady()
	{
		if (_listener != null)
		{
			_listener.OnClickBattleReady();
		}
	}
}
