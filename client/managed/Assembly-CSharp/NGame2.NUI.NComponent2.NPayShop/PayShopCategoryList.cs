using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NHelper.NPayShop;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopCategoryList : MonoBehaviour, ButtonListGroup.IListener, PayShopCategoryComponent.IListener
{
	public interface IListener
	{
		void SelectCategory(string key);
	}

	public ButtonListGroup CategoryListGroup;

	private IListener _listener;

	private string _selectedMainKey = "";

	private string _selectedSubKey = "";

	private List<PayShopCategoryComponent> _payShopCategoryComponents = new List<PayShopCategoryComponent>();

	private string _selectedKey
	{
		get
		{
			if (String_Extension_Method.IsNullOrEmpty(_selectedSubKey))
			{
				return _selectedMainKey;
			}
			return _selectedSubKey;
		}
	}

	public void Open(IListener listener)
	{
		_listener = listener;
		InitCategoryButton();
	}

	public void SelectCategory(string category)
	{
		CategoryListGroup.ChagneButton(category);
		NewPayShopGroupData data = GameRoot.Instance.NewPayShopGroupDataContainer.GetData(category);
		if (data != null)
		{
			if (String_Extension_Method.IsNullOrEmpty(data.ParentCategory))
			{
				_selectedMainKey = category;
				return;
			}
			_selectedMainKey = data.ParentCategory;
			_selectedSubKey = category;
		}
	}

	public void ClearAll()
	{
		if (!String_Extension_Method.IsNullOrEmpty(_selectedKey))
		{
			PayShopCategoryComponent payShopCategoryComponent = FindCategory(_selectedKey);
			if (payShopCategoryComponent != null)
			{
				payShopCategoryComponent.CheckNewFlag();
			}
		}
		_selectedMainKey = "";
		_selectedSubKey = "";
		FactoryBase.ReleaseAndClearList(_payShopCategoryComponents);
		CategoryListGroup.Clear();
	}

	private void InitCategoryButton()
	{
		CategoryListGroup.Init(this);
		NewPayShopGroupData[] array = GameRoot.Instance.NewPayShopGroupDataContainer.GetAllData().ToArray();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(array))
		{
			Array.Sort(array);
			NewPayShopGroupData[] array2 = array;
			foreach (NewPayShopGroupData newPayShopGroupData in array2)
			{
				if (newPayShopGroupData.IsEnable && CheckGroupDataEnable(newPayShopGroupData) && (String_Extension_Method.IsNullOrEmpty(newPayShopGroupData.ParentCategory) || GameRoot.Instance.NewPayShopGroupDataContainer.GetData(newPayShopGroupData.ParentCategory).IsEnable))
				{
					PrefabAuto<PayShopCategoryComponent> prefabAuto = PayShopCategoryComponent.GetPrefabAuto(newPayShopGroupData.EPayShopCategoryType, String_Extension_Method.IsNullOrEmpty(newPayShopGroupData.ParentCategory));
					AddButton(newPayShopGroupData, prefabAuto);
				}
			}
		}
		CategoryListGroup.LinkParent();
		CategoryListGroup.ResetPosition();
		CategoryListGroup.Reposition();
	}

	private bool CheckGroupDataEnable(NewPayShopGroupData groupData)
	{
		if (!groupData.IsEnable)
		{
			return false;
		}
		if (groupData.EnableCondition == EGroupEnableConditionType.ExistBuyableLegendCostume && !(groupData.PayShopCategoryType != "LegendCostume"))
		{
			return PayShopHelper.ExistBuyableLegendCostume();
		}
		return true;
	}

	public PayShopCategoryComponent AddButton(NewPayShopGroupData groupData, PrefabAuto<PayShopCategoryComponent> prefabAuto)
	{
		PayShopCategoryComponent payShopCategoryComponent = CategoryListGroup.AddButton(groupData.PayShopCategoryType, groupData.ParentCategory, prefabAuto);
		payShopCategoryComponent.Open(groupData, this);
		_payShopCategoryComponents.Add(payShopCategoryComponent);
		return payShopCategoryComponent;
	}

	void ButtonListGroup.IListener.OnClickButton(string key)
	{
		NewPayShopGroupData data = GameRoot.Instance.NewPayShopGroupDataContainer.GetData(key);
		if (data != null)
		{
			if (String_Extension_Method.IsNullOrEmpty(data.ParentCategory))
			{
				if (GameRoot.Instance.NewPayShopGroupDataContainer.IsParentAndHasChild(data))
				{
					if (!(_selectedMainKey != key))
					{
						return;
					}
					NewPayShopGroupData[] datasWithSameParent = GameRoot.Instance.NewPayShopGroupDataContainer.GetDatasWithSameParent(key);
					if (datasWithSameParent == null || datasWithSameParent.Length == 0)
					{
						_selectedMainKey = key;
						return;
					}
					Array.Sort(datasWithSameParent, delegate(NewPayShopGroupData a, NewPayShopGroupData b)
					{
						int num2 = 0;
						num2 = a.Sort.CompareTo(b.Sort);
						return (num2 != 0) ? num2 : a.PayShopCategoryType.CompareTo(b.PayShopCategoryType);
					});
					NewPayShopGroupData[] array = datasWithSameParent;
					foreach (NewPayShopGroupData newPayShopGroupData in array)
					{
						if (newPayShopGroupData != null && CheckGroupDataEnable(newPayShopGroupData))
						{
							CategoryListGroup.OnClickButton(newPayShopGroupData.PayShopCategoryType);
							return;
						}
					}
					_selectedMainKey = key;
					return;
				}
				_selectedMainKey = key;
				_selectedSubKey = "";
			}
			else
			{
				_selectedSubKey = key;
			}
		}
		if (_listener != null)
		{
			_listener.SelectCategory(key);
		}
	}

	public void SetCategoryFlag(string category, PayShopFlagType flag)
	{
		PayShopCategoryComponent payShopCategoryComponent = FindCategory(category);
		if (payShopCategoryComponent != null)
		{
			payShopCategoryComponent.SetFlag(flag);
		}
	}

	private PayShopCategoryComponent FindCategory(string category)
	{
		return _payShopCategoryComponents.Find((PayShopCategoryComponent comp) => comp.GroupData.PayShopCategoryType == category);
	}

	void PayShopCategoryComponent.IListener.OnCheckNewFlag(string category)
	{
		NewPayShopGroupData data = GameRoot.Instance.NewPayShopGroupDataContainer.GetData(category);
		if (String_Extension_Method.IsNullOrEmpty(data.ParentCategory))
		{
			return;
		}
		NewPayShopGroupData[] datasWithSameParent = GameRoot.Instance.NewPayShopGroupDataContainer.GetDatasWithSameParent(data.ParentCategory);
		PayShopFlagType payShopFlagType = PayShopFlagType.None;
		NewPayShopGroupData[] array = datasWithSameParent;
		foreach (NewPayShopGroupData newPayShopGroupData in array)
		{
			PayShopCategoryComponent payShopCategoryComponent = FindCategory(newPayShopGroupData.PayShopCategoryType);
			if (!(payShopCategoryComponent == null))
			{
				payShopFlagType |= payShopCategoryComponent.Flag;
			}
		}
		FindCategory(data.ParentCategory).SetFlag(payShopFlagType);
	}
}
