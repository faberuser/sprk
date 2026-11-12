using System;
using NGame2.NUI.NWindow.NTower;
using NShared;

namespace NGame2.NUI.NManager.NTower;

public class LabyrinthImpl : TowerManagement.IImplement, LabyrinthWindow.Listener
{
	private readonly TowerManagement _management;

	public LabyrinthImpl(TowerManagement management)
	{
		_management = management;
	}

	public void Open(TowerData towerData, TowerInfo towerInfo, int showFloor, bool showClearEffect, bool returnFromBattle, Action onOpen)
	{
		_management.AddTask(_management.TaskFactory.Run(delegate(LabyrinthWindow window)
		{
			window.Init(towerData, towerInfo, showFloor, this);
		}), _management.TaskFactory.Run((LabyrinthWindow window) => window.OpenAsync(immediately: false, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onOpen);
		})));
	}

	void LabyrinthWindow.Listener.OnEnterFloor(LabyrinthWindow caller, TowerFloorData floorData)
	{
		if (floorData != null)
		{
			int completedFloor = _management.GetCompletedFloor();
			if (floorData.Floor <= completedFloor + 1)
			{
				_management.InitFloorData(floorData);
				_management.OpenBattlePartySetting();
			}
		}
	}
}
