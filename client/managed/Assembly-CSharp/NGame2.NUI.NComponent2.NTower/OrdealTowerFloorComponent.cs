using System;
using NGame2.NUI.NHelper.NTower;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NTower;

public class OrdealTowerFloorComponent : MonoBehaviour
{
	public interface Listener
	{
		void OnClickFloor(OrdealTowerFloorComponent component);

		void OnDragComponent();

		void OnDragComponentEnd();
	}

	[Serializable]
	public class OrdealTransformRoot
	{
		public Transform Root;

		public OrdealTowerFloorState State;
	}

	[Serializable]
	public class ChallengeTransformRoot
	{
		public Transform Root;

		public ChallengeTowerFloorState State;
	}

	public const string PrefabPath = "Prefab2/Component/Tower/OrdealTowerFloorComponent";

	public const string ChallengePrefabPath = "Prefab2/Component/Tower/ChallengeTowerFloorComponent";

	public static PrefabAuto<OrdealTowerFloorComponent> PrefabAuto = new PrefabAuto<OrdealTowerFloorComponent>("Prefab2/Component/Tower/OrdealTowerFloorComponent");

	public static PrefabAuto<OrdealTowerFloorComponent> ChallengePrefabAuto = new PrefabAuto<OrdealTowerFloorComponent>("Prefab2/Component/Tower/ChallengeTowerFloorComponent");

	public bool IsUnderGround;

	private Listener _listener;

	public ChallengeTransformRoot[] ChallengeTransformRoots;

	public OrdealTransformRoot[] OrdealTransformRoots;

	public UISprite Selected;

	public UILabel FloorLabel;

	public UILabel CompletedFloorLabel;

	public int Floor { get; private set; }

	public int CompleteFloor { get; private set; }

	public bool Cleared => Floor <= CompleteFloor;

	public bool Disabled => Floor > CompleteFloor + 1;

	public bool IsMaxFloor { get; private set; }

	public int FloorCountPerPage { get; private set; }

	public void Open(Listener listener, int floor, int completeFloor, int floorPerPage, bool isMaxFloor = false, bool isUnderGround = false)
	{
		_listener = listener;
		Floor = floor;
		CompleteFloor = completeFloor;
		FloorCountPerPage = floorPerPage;
		IsMaxFloor = isMaxFloor;
		IsUnderGround = isUnderGround;
		InitFloorText();
	}

	public void RefreshSelection(int selectedFloor)
	{
		OrdealTowerFloorState ordealTowerFloorState = OrdealTowerFloorState.Cleared;
		if (!Cleared)
		{
			ordealTowerFloorState = ((!Disabled) ? OrdealTowerFloorState.Opened : OrdealTowerFloorState.OpenedGray);
		}
		OrdealTransformRoot[] ordealTransformRoots = OrdealTransformRoots;
		foreach (OrdealTransformRoot ordealTransformRoot in ordealTransformRoots)
		{
			UIHelper.EnableGameObject(ordealTransformRoot.Root, ordealTransformRoot.State == ordealTowerFloorState);
		}
		UIHelper.EnableGameObject(Selected, Floor == selectedFloor);
	}

	private bool IsBossFloor(int floor)
	{
		return floor % 10 == 0;
	}

	private void InitFloorText()
	{
		string localizedFloorText = TowerHelper.GetLocalizedFloorText(Floor, IsUnderGround);
		UIHelper.EnableGameObject(FloorLabel, Floor > CompleteFloor);
		UIHelper.EnableGameObject(CompletedFloorLabel, Floor <= CompleteFloor);
		UIHelper.SetLabelText(FloorLabel, localizedFloorText);
		UIHelper.SetLabelText(CompletedFloorLabel, localizedFloorText);
	}

	public void RefreshSelectionMaze(int selectedFloor)
	{
		ChallengeTowerFloorState challengeTowerFloorState = ChallengeTowerFloorState.Cleared;
		if (!Cleared)
		{
			challengeTowerFloorState = ((!Disabled) ? ChallengeTowerFloorState.OpenedNormal : ChallengeTowerFloorState.OpenedNormalGray);
		}
		ChallengeTransformRoot[] challengeTransformRoots = ChallengeTransformRoots;
		foreach (ChallengeTransformRoot challengeTransformRoot in challengeTransformRoots)
		{
			UIHelper.EnableGameObject(challengeTransformRoot.Root, challengeTransformRoot.State == challengeTowerFloorState);
		}
		UIHelper.EnableGameObject(Selected, Floor == selectedFloor);
	}

	public void SetCompletedFloor(int floor)
	{
		CompleteFloor = floor;
	}

	public void RefreshSelectionChallenge(int selectedFloor)
	{
		ChallengeTowerFloorState challengeTowerFloorState = ChallengeTowerFloorState.Cleared;
		if (!Cleared)
		{
			challengeTowerFloorState = ((!IsBossFloor(Floor)) ? ((!Disabled) ? ChallengeTowerFloorState.OpenedNormal : ChallengeTowerFloorState.OpenedNormalGray) : ((!IsMaxFloor) ? (Disabled ? ChallengeTowerFloorState.OpenedSemiBossGray : ChallengeTowerFloorState.OpenedSemiBoss) : (Disabled ? ChallengeTowerFloorState.OpenedBossGray : ChallengeTowerFloorState.OpenedBoss)));
		}
		ChallengeTransformRoot[] challengeTransformRoots = ChallengeTransformRoots;
		foreach (ChallengeTransformRoot challengeTransformRoot in challengeTransformRoots)
		{
			UIHelper.EnableGameObject(challengeTransformRoot.Root, challengeTransformRoot.State == challengeTowerFloorState);
		}
		UIHelper.EnableGameObject(Selected, Floor == selectedFloor);
	}

	public void OnClickFloor()
	{
		if (_listener != null)
		{
			_listener.OnClickFloor(this);
		}
	}

	public void OnDragComponent()
	{
		if (_listener != null)
		{
			_listener.OnDragComponent();
		}
	}

	public void OnDragComponentEnd()
	{
		if (_listener != null)
		{
			_listener.OnDragComponentEnd();
		}
	}
}
