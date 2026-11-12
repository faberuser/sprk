using System.Collections.Generic;
using NGame2.NUI.NComponent2.NHelper;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class RewardPopupPartyRewardComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/RewardPopup/RewardPopupPartyRewardComponent";

	public static PrefabAuto<RewardPopupPartyRewardComponent> PrefabAuto = new PrefabAuto<RewardPopupPartyRewardComponent>("Resources/Prefab2/Component/RewardPopup/RewardPopupPartyRewardComponent");

	public UISimpleGrid RewardItemGrid;

	public List<Component> _rewardList;

	public ItemAnimationContainer _itemAnimationContainer = new ItemAnimationContainer(open3dModelPopup: true);

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	private void Update()
	{
		_itemAnimationContainer.ChainUpdate();
	}
}
