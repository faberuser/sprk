using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NEventForge;

public class EventForgeGetRewardGuideComponent : UIScrollItemBehaviour
{
	public struct Info
	{
		public int ReinforcementLevel { get; set; }

		public int CurrentReinforcementLevel { get; set; }

		public bool IsLast { get; set; }
	}

	public const string PrefabPath = "Prefab2/Component/EventForge/EventForgeGetRewardGuideComponent";

	public static PrefabAuto<EventForgeGetRewardGuideComponent> PrefabAuto = new PrefabAuto<EventForgeGetRewardGuideComponent>("Prefab2/Component/EventForge/EventForgeGetRewardGuideComponent");

	public Transform CurrentPointTransform;

	public Transform NextPointTranform;

	public Transform SelectPointTransform;

	public Transform LineTransform;

	private int _reinforcementLevel = -1;

	public void Open(Info info)
	{
		Init(info);
	}

	private void Init(Info? info)
	{
		if (info.HasValue && info.HasValue)
		{
			_reinforcementLevel = info.Value.ReinforcementLevel;
			int currentReinforcementLevel = info.Value.CurrentReinforcementLevel;
			EnableCurrentPoint(_reinforcementLevel <= currentReinforcementLevel);
			EnableNextPoint(_reinforcementLevel == currentReinforcementLevel + 1);
			EnableSelectPoint(_reinforcementLevel == currentReinforcementLevel);
			EnableLine(info.Value.IsLast);
		}
	}

	private void EnableCurrentPoint(bool isCurrent)
	{
		UIHelper.EnableGameObject(CurrentPointTransform, isCurrent);
	}

	private void EnableNextPoint(bool isNext)
	{
		UIHelper.EnableGameObject(NextPointTranform, isNext);
	}

	private void EnableSelectPoint(bool isSelect)
	{
		UIHelper.EnableGameObject(SelectPointTransform, isSelect);
	}

	public void EnableSelectPoint(int selectReinforcementLevel)
	{
		EnableSelectPoint(_reinforcementLevel == selectReinforcementLevel);
	}

	private void EnableLine(bool isLast)
	{
		UIHelper.EnableGameObject(LineTransform, !isLast);
	}
}
