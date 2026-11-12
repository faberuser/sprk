using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ValanceEnhanceResultBase : BaseCinemaWindow
{
	public GameObject HideRoot;

	public UISimpleGrid ItemGrid;

	public UILabel ItemName;

	public UIButton ExitButton;

	private ItemIconComponent _resultItemComp;

	protected bool _isNodeTreeEnd;

	public EquipItemInfo EquipItemInfo { get; protected set; }

	public EquipItemData EquipItemData { get; protected set; }

	public virtual void Init()
	{
		SetResultItem();
	}

	protected override void NodeTreeOpenOnEnd()
	{
		_isNodeTreeEnd = true;
	}

	public override void OnSystemBackButton()
	{
		if (_isNodeTreeEnd)
		{
			base.OnSystemBackButton();
		}
	}

	public void SetResultItem()
	{
		if (_resultItemComp != null)
		{
			FactoryBase.SafeReleaseComponent(ref _resultItemComp);
		}
		_resultItemComp = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
		_resultItemComp.Open(EquipItemInfo, null);
		string text = "";
		if (EquipItemInfo.ItemData != null)
		{
			text = EquipItemInfo.ItemData.Name;
		}
		UIHelper.SetLabelText(ItemName, text);
	}

	private void ReleaseItem()
	{
		EquipItemInfo = null;
		EquipItemData = null;
		FactoryBase.SafeReleaseComponent(ref _resultItemComp);
	}

	public override void Close()
	{
		_isNodeTreeEnd = false;
		ReleaseItem();
		base.Close();
	}
}
