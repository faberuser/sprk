using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class IconMessagePopup : MessagePopup
{
	public new const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/IconMessagePopup";

	public UISimpleGrid IconGrid;

	private List<Component> IconList = new List<Component>();

	public static IconMessagePopup Open(string title, string message1, string message2, Action onConfirm, Action onCancel = null)
	{
		WindowPrefab<IconMessagePopup> windowPrefab = new WindowPrefab<IconMessagePopup>("Resources/Prefab2/Window/MessagePopup/IconMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(title, message1, message2, onConfirm, onCancel);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	protected new void OpenInternal(string title, string message1, string message2, Action onConfirm, Action onCancel = null)
	{
		base.Open();
		_onYes = onConfirm;
		_onNo = onCancel;
		_onClose = onCancel;
		SetMessage(title, TitleLabel);
		SetMessage(message1, MessageLabel);
		SetMessage(message2, MessageLabel2);
		SetOKButtonLabel(LocalizationManager.GetStringDirect("COMMON_MOVE"));
	}

	private void SetMessage(string message, UILabel label)
	{
		if (String_Extension_Method.IsNullOrEmpty(message))
		{
			UIHelper.EnableGameObject(label, enable: false);
			return;
		}
		UIHelper.EnableGameObject(label, enable: true);
		UIHelper.SetLabelText(label, message);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(IconList);
	}

	public void AddItem(int itemIndex, int count)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data != null)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(IconGrid, ItemIconComponent.PrefabAuto);
			if (!(itemIconComponent == null))
			{
				itemIconComponent.Open(data);
				itemIconComponent.SetItemCount(count);
				IconList.Add(itemIconComponent);
				UIHelper.RepositionGrid(IconGrid);
			}
		}
	}

	public void AddEquipItem(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(IconGrid, ItemIconComponent.PrefabAuto);
			if (!(itemIconComponent == null))
			{
				itemIconComponent.Open(equipItemInfo, null);
				IconList.Add(itemIconComponent);
				UIHelper.RepositionGrid(IconGrid);
			}
		}
	}

	public void AddHero(int heroIndex, int star = 0, int transcend = 0)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data != null)
		{
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(IconGrid, HeroPortraitComponent.SmallPrefabAuto);
			if (!(heroPortraitComponent == null))
			{
				heroPortraitComponent.Open(data, null, HeroPortraitComponent.EPortraitMode.Icon, star, transcend);
				heroPortraitComponent.ShowCreatureName();
				IconList.Add(heroPortraitComponent);
				UIHelper.RepositionGrid(IconGrid);
			}
		}
	}
}
