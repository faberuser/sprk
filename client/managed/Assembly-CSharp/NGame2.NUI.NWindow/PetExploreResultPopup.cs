using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PetExploreResultPopup : BaseRewardPopup
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Pet/Popup/PetExploreResultPopup";

	public GameObject AnimationRoot;

	public UILabel LabelExploreName;

	public UILabelSelector LabelSelectorDeckName;

	public UIGrid PetGrid;

	public UILabel LabelPenaltyTime;

	public Transform TransfromRewardRoot;

	private ItemIconComponent _itemIconComponent;

	private List<PetExploreResultComponent> _listPetExploreResultComponent = new List<PetExploreResultComponent>();

	private PetExploreInfo _petExploreInfo;

	public static void Open(PetExploreInfo petExploreInfo, PopupOpenData popupOpenData, Action onClose)
	{
		WindowPrefab<PetExploreResultPopup> windowPrefab = new WindowPrefab<PetExploreResultPopup>("Resources/Prefab2/Window/Pet/Popup/PetExploreResultPopup", GameUIRoot.ELayerType.UI);
		if (windowPrefab != null && !windowPrefab.IsOpen())
		{
			windowPrefab.instance.OpenInteral(petExploreInfo, popupOpenData, onClose);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	public void OpenInteral(PetExploreInfo petExploreInfo, PopupOpenData popupOpenData, Action onClose)
	{
		if (petExploreInfo != null && popupOpenData != null)
		{
			_petExploreInfo = petExploreInfo;
			OpenWithData(popupOpenData, isGlobal: false, onClose);
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ReleasePetExploreResultComponent();
		ReleaseItemiconComponent();
	}

	private void ReleasePetExploreResultComponent()
	{
		FactoryBase.ReleaseAndClearList(_listPetExploreResultComponent);
	}

	private void ReleaseItemiconComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
	}

	protected override void OnOpenAction()
	{
		base.OnOpenAction();
		List<int> petIndexList = _petExploreInfo.GetPetIndexList();
		int exploreIndex = _petExploreInfo.ExploreIndex;
		int deckIndex = _petExploreInfo.DeckIndex;
		PetAdventureData data = GameRoot.Instance.PetAdventureDataContainer.GetData(exploreIndex);
		int reqFood = data?.ReqFood ?? 0;
		string exploreNameLabel = ((data != null) ? data.NameKey : string.Empty);
		PetAdventureRewardData data2 = GameRoot.Instance.PetAdventureRewardDataContainer.GetData(exploreIndex);
		int explorePenaltyLabel = data2?.PenaltyTime ?? 0;
		int rewardItemIconComponent = data2?.RewardItemIndex ?? 0;
		SetExploreNameLabel(exploreNameLabel);
		SetExploreDeckName(deckIndex);
		SetExplorePenaltyLabel(explorePenaltyLabel);
		SetPetExploreResultComponent(petIndexList, reqFood);
		SetRewardItemIconComponent(rewardItemIconComponent);
	}

	private void SetExploreNameLabel(string nameKey)
	{
		string stringDirect = LocalizationManager.GetStringDirect(nameKey);
		SetLabel(LabelExploreName, stringDirect);
	}

	private void SetExploreDeckName(int deckIndex)
	{
		if (LabelSelectorDeckName != null)
		{
			LabelSelectorDeckName.Select(deckIndex.ToString());
		}
	}

	private void SetExplorePenaltyLabel(int penaltyTimeSec)
	{
		TimeSpan timeSpan = new TimeSpan(0, 0, penaltyTimeSec);
		string text = $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
		SetLabel(LabelPenaltyTime, text);
	}

	private void SetLabel(UILabel uiLabel, string text)
	{
		bool flag = !String_Extension_Method.IsNullOrEmpty(text);
		UIHelper.EnableGameObject(uiLabel, flag);
		if (flag)
		{
			UIHelper.SetLabelText(uiLabel, text);
		}
	}

	private void SetRewardItemIconComponent(int rewardItemIndex)
	{
		if (TransfromRewardRoot == null)
		{
			ReleaseItemiconComponent();
			return;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(rewardItemIndex);
		if (data == null)
		{
			ReleaseItemiconComponent();
			return;
		}
		if (_itemIconComponent == null)
		{
			_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(TransfromRewardRoot);
		}
		if (_itemIconComponent != null)
		{
			_itemIconComponent.ResetPosition();
			_itemIconComponent.Open(data);
		}
	}

	private void SetPetExploreResultComponent(List<int> petIndexList, int reqFood)
	{
		if (PetGrid == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(petIndexList); i++)
		{
			int petIndex = petIndexList[i];
			PetInfo petInfo = GetPetInfo(petIndex);
			if (petInfo != null)
			{
				PetExploreResultComponent petExploreResultComponent = PetExploreResultComponent.PrefabAuto.Factory.Create(PetGrid.transform);
				if (petExploreResultComponent == null)
				{
					break;
				}
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(petExploreResultComponent.transform);
				petExploreResultComponent.Open(petInfo, reqFood);
				_listPetExploreResultComponent.Add(petExploreResultComponent);
			}
		}
		UIHelper.ResetGridPosition(PetGrid);
	}

	private PetInfo GetPetInfo(int petIndex)
	{
		if (!AccountManager.IsLogin())
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(petIndex);
	}
}
