using NGame2.NUI.NHelper.NTower;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NTower;

public class LabyrinthFloorComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
		void OnClickFloor(LabyrinthFloorComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/Tower/LabyrinthFloorComponent";

	public static PrefabAuto<LabyrinthFloorComponent> PrefabAuto = new PrefabAuto<LabyrinthFloorComponent>("Prefab2/Component/Tower/LabyrinthFloorComponent");

	public Transform NotYetRoot;

	public Transform CurrentRoot;

	public Transform CompletedRoot;

	public Transform SelectedWidget;

	public UILabel NotYetFloorLabel;

	public UILabel CurrentFloorLabel;

	public UILabel CompletedFloorLabel;

	private Listener _listener;

	public TowerData TowerData { get; private set; }

	public TowerFloorData FloorData { get; private set; }

	public int CompletedFloor { get; private set; }

	public bool IsCompleted => CompletedFloor >= FloorData.Floor;

	public void Open(Listener listener, TowerData towerData, TowerFloorData floorData, int completedFloor)
	{
		_listener = listener;
		TowerData = towerData;
		FloorData = floorData;
		CompletedFloor = completedFloor;
		int num = CompletedFloor + 1;
		UIHelper.EnableGameObject(NotYetRoot, floorData.Floor > num);
		UIHelper.EnableGameObject(CurrentRoot, floorData.Floor == num);
		UIHelper.EnableGameObject(CompletedRoot, floorData.Floor < num);
		InitFloorNumber();
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	private void InitFloorNumber()
	{
		string localizedFloorText = TowerHelper.GetLocalizedFloorText(FloorData.Floor);
		UIHelper.SetLabelText(NotYetFloorLabel, localizedFloorText);
		UIHelper.SetLabelText(CurrentFloorLabel, localizedFloorText);
		UIHelper.SetLabelText(CompletedFloorLabel, localizedFloorText);
	}

	public void SetSelected(bool selected)
	{
		UIHelper.EnableGameObject(SelectedWidget, selected);
	}

	public void OnClickFloor()
	{
		if (_listener != null)
		{
			_listener.OnClickFloor(this);
		}
	}
}
