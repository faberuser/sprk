using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class OverlapRewardPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/OverlapRewardPopup";

	public Transform OverlapRewardIconPos;

	public Transform RewardIconPos;

	public UILabel Text;

	private ItemIconComponent _rewardItemIcon;

	private ItemIconComponent _overlapRewardItemIcon;

	public static void Open(int rewardIndex, int overlapRewardIndex)
	{
		WindowPrefab<OverlapRewardPopup> windowPrefab = new WindowPrefab<OverlapRewardPopup>("Resources/Prefab2/Window/MessagePopup/OverlapRewardPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(rewardIndex, overlapRewardIndex);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(int rewardIndex, int overlapRewardIndex)
	{
		base.Open();
		_rewardItemIcon = UIHelper.CreateRewardMainItemIconComponent(rewardIndex, RewardIconPos);
		_overlapRewardItemIcon = UIHelper.CreateRewardMainItemIconComponent(overlapRewardIndex, OverlapRewardIconPos);
		SetText(_overlapRewardItemIcon.ItemData.Name);
	}

	public override void Reset()
	{
		base.Reset();
		Release();
	}

	public void Release()
	{
		FactoryBase.SafeReleaseComponent(ref _rewardItemIcon);
		FactoryBase.SafeReleaseComponent(ref _overlapRewardItemIcon);
	}

	public void SetText(string overlapRewardName)
	{
		string stringDirect = LocalizationManager.GetStringDirect("HERO_OVERLAP_REWARD", overlapRewardName);
		UIHelper.SetLabelText(Text, stringDirect);
	}
}
