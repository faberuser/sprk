using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

[Serializable]
public class RestoreBuffInfo
{
	public Transform BuffItemIconComponentRootTransform;

	public UILabel BuffNameLabel;

	public UILabel BuffCountLabel;

	public UIGame3DWidget UIGame3DWidget;

	private ItemIconComponent _buffItemIconComponent;

	public void Open()
	{
		Init();
	}

	private void Init()
	{
		SetSoulStoneItemIconComponent();
	}

	public void Close()
	{
		if (_buffItemIconComponent != null)
		{
			FactoryBase.SafeReleaseComponent(ref _buffItemIconComponent);
		}
	}

	private void SetSoulStoneItemIconComponent()
	{
		if (BuffItemIconComponentRootTransform == null)
		{
			return;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(5251);
		int itemCount = Helper.GetItemCount(data.Index);
		UIHelper.SetLabelText(BuffNameLabel, LocalizationManager.GetStringDirect(data.Name));
		UIHelper.SetLabelText(BuffCountLabel, itemCount);
		if (_buffItemIconComponent == null)
		{
			_buffItemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(BuffItemIconComponentRootTransform);
			if (_buffItemIconComponent == null)
			{
				return;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_buffItemIconComponent.transform);
			_buffItemIconComponent.Open(data);
		}
		if (!(_buffItemIconComponent != null))
		{
			return;
		}
		_buffItemIconComponent.SetIconGrayScale(itemCount == 0);
		if (itemCount > 0)
		{
			if (UIGame3DWidget != null)
			{
				UIGame3DWidget.Play(null);
			}
		}
		else if (UIGame3DWidget != null)
		{
			UIGame3DWidget.Stop(immediatelyClear: true);
		}
	}
}
