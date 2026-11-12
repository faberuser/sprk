using System;
using System.Collections.Generic;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI3.NEmbed;

public class Embed_ItemGrid_Comp : MonoBehaviour, ItemIconComponent.Listener
{
	public interface IListener
	{
		void OnSelect(ItemIconComponent itemIconComponent);
	}

	[Header("-- [NoItem 알림창]")]
	public UILabel Label_NoItemNotice;

	[Header("-- [아이템 그리드]")]
	public UIScrollGrid ItemGrid;

	private IListener _listener;

	private List<ItemIconComponent> _compList = new List<ItemIconComponent>();

	public void InitUi(IListener listener, string noItemNoticeString)
	{
		_listener = listener;
		UIHelper.SetLabelText(Label_NoItemNotice, noItemNoticeString);
	}

	public void ForEach(Action<ItemIconComponent> action)
	{
		if (action != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_compList); i++)
			{
				ItemIconComponent obj = _compList[i];
				action(obj);
			}
		}
	}

	public void AddItem(EquipItemInfo info)
	{
		ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
		if (!itemIconComponent.Open(info, this))
		{
			FactoryBase.ReleaseComponent(itemIconComponent);
			return;
		}
		_compList.Add(itemIconComponent);
		UIHelper.EnableGameObject(Label_NoItemNotice, enable: false);
	}

	public void Clear()
	{
		FactoryBase.ReleaseAndClearList(_compList);
		UIHelper.EnableGameObject(Label_NoItemNotice, enable: true);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (_listener != null)
		{
			_listener.OnSelect(itemIconComponent);
		}
	}
}
