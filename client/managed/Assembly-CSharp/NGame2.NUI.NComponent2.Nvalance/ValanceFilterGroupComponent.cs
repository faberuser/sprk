using System;
using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.Nvalance;

public class ValanceFilterGroupComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener : ValanceFilterComponent.IListener
	{
		void OnClickFoldOut(bool isFold);
	}

	public const string PrefabPath = "Prefab2/Component/Valance/ValanceFilterGroupComponent";

	public static PrefabAuto<ValanceFilterGroupComponent> PrefabAuto = new PrefabAuto<ValanceFilterGroupComponent>("Prefab2/Component/Valance/ValanceFilterGroupComponent");

	public UIWidget Widget;

	public UILabel LabelName;

	public UISimpleGrid SimpleGrid;

	public UISimpleGrid SimpleLongGrid;

	public TweenRotation Tween;

	private bool _isFoldOut;

	private bool _isLongComponent;

	private Vector2 _initWidgetSize;

	private List<ValanceFilterComponent> _listComp = new List<ValanceFilterComponent>();

	private IListener _listener;

	void InitOnCreate.OnCreate()
	{
		if (_initWidgetSize == Vector2.zero)
		{
			_initWidgetSize = new Vector2(Widget.width, Widget.height);
		}
	}

	void InitOnCreate.OnRelease()
	{
		Close();
	}

	public void Open(ValanceFilterGroupData groupData, IListener listener)
	{
		_listener = listener;
		_isFoldOut = true;
		SetFilter(groupData);
		SetLabel(groupData.LocalKey);
		SetTween(_isFoldOut);
	}

	public void SetFilter(ValanceFilterGroupData groupData)
	{
		if (groupData.GroupType == EValanceFilterGroupType.TechnoMagic || groupData.GroupType == EValanceFilterGroupType.Craft)
		{
			IList<ValanceEquipFilterData> dataListByGroupIndex = GameRoot.Instance.ValanceEquipFilterDataContainer.GetDataListByGroupIndex(groupData.Index);
			SetFilter(dataListByGroupIndex, groupData.IsLongComponent);
		}
		else if (groupData.GroupType == EValanceFilterGroupType.Creature)
		{
			IList<ValanceCreatureFilterData> dataListByGroupIndex2 = GameRoot.Instance.ValanceCreatureFilterDataContainer.GetDataListByGroupIndex(groupData.Index);
			SetFilter(dataListByGroupIndex2, groupData.IsLongComponent);
		}
	}

	public void SetTween(bool isForward)
	{
		if (!(Tween == null))
		{
			if (isForward)
			{
				Tween.PlayForward();
			}
			else
			{
				Tween.PlayReverse();
			}
		}
	}

	public void SetLabel(string key)
	{
		string stringDirect = LocalizationManager.GetStringDirect(key);
		UIHelper.SetLabelText(LabelName, stringDirect);
	}

	private UISimpleGrid GetCurrentGrid()
	{
		if (_isLongComponent)
		{
			return SimpleLongGrid;
		}
		return SimpleGrid;
	}

	private void SetFilter<T>(IList<T> infos, bool isLong) where T : IValanceFilterInfo
	{
		ReleaseComp();
		_isLongComponent = isLong;
		UISimpleGrid currentGrid = GetCurrentGrid();
		if (currentGrid == null)
		{
			return;
		}
		for (int i = 0; i < infos.Count; i++)
		{
			T val = infos[i];
			if (val != null)
			{
				ValanceFilterComponent valanceFilterComponent = ((!isLong) ? UIHelper.AddGridItem(currentGrid, ValanceFilterComponent.PrefabAuto) : UIHelper.AddGridItem(currentGrid, ValanceFilterComponent.PrefabAuto2));
				if (!(valanceFilterComponent == null))
				{
					valanceFilterComponent.Open(val, _listener);
					_listComp.Add(valanceFilterComponent);
				}
			}
		}
		FoldOut(_isFoldOut);
		UIHelper.RepositionGrid(currentGrid);
	}

	public void Reset()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listComp); i++)
		{
			ValanceFilterComponent valanceFilterComponent = _listComp[i];
			if (valanceFilterComponent != null)
			{
				valanceFilterComponent.SetSelected(isSelect: false);
			}
		}
	}

	public void SetEnable(bool isEnable)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listComp); i++)
		{
			ValanceFilterComponent valanceFilterComponent = _listComp[i];
			if (valanceFilterComponent != null)
			{
				valanceFilterComponent.SetEnable(isEnable);
			}
		}
	}

	private void ReleaseComp()
	{
		FactoryBase.ReleaseAndClearList(_listComp);
	}

	public void Close()
	{
		_isFoldOut = true;
		ReleaseComp();
	}

	public void FoldOut(bool isFoldOut)
	{
		if (Widget == null)
		{
			return;
		}
		UISimpleGrid currentGrid = GetCurrentGrid();
		if (!(currentGrid == null))
		{
			if (isFoldOut)
			{
				int num = Mathf.CeilToInt((float)__Extension_Method_ICollection__.COUNT(_listComp) / (float)currentGrid.Grid.maxPerLine);
				Widget.height = (int)_initWidgetSize.y + num * (int)currentGrid.Grid.cellHeight;
			}
			else
			{
				Widget.height = (int)_initWidgetSize.y;
			}
			NGUITools.UpdateWidgetCollider(Widget.gameObject);
			UIHelper.EnableGameObject(currentGrid, isFoldOut);
		}
	}

	public void OnClickFoldOut()
	{
		if (_listener != null)
		{
			_isFoldOut = !_isFoldOut;
			FoldOut(_isFoldOut);
			SetTween(_isFoldOut);
			_listener.OnClickFoldOut(_isFoldOut);
		}
	}

	public void ForFilterComponent(Action<ValanceFilterComponent> doAction)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listComp); i++)
		{
			ValanceFilterComponent valanceFilterComponent = _listComp[i];
			if (!(valanceFilterComponent == null))
			{
				doAction(valanceFilterComponent);
			}
		}
	}
}
