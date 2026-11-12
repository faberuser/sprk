using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class RuneSelectItemFilterOptionList : BaseWindow, RuneSelectItemFilterMenuComponent.Listener
{
	public interface IListener
	{
		void OnClickConfirm(List<Enum> selectFilterOption, Action onComplete);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/RuneSelectItemFilterOptionList";

	public UIButton ButtonResetFilter;

	public UIScrollGrid GridOptionList;

	private int _selectAbleCount;

	private IListener _listener;

	private List<Enum> _selectFilterOption = new List<Enum>();

	private IncreaseDic<int, Enum> _dicRuneAvailableStat = new IncreaseDic<int, Enum>();

	private List<RuneSelectItemFilterMenuComponent> _listOptionComponent = new List<RuneSelectItemFilterMenuComponent>();

	public void Open(IListener listener, List<Enum> alreadySelectFilterOption, int selectAbleCount)
	{
		Open(delegate
		{
			Init(listener, alreadySelectFilterOption, selectAbleCount);
		}, null);
	}

	private void Init(IListener listener, List<Enum> alreadySelectFilterOption, int selectAbleCount)
	{
		_listener = listener;
		_selectAbleCount = selectAbleCount;
		SetAlreadySelectFilterOption(alreadySelectFilterOption);
		SetRuneStatList();
		SetStatList();
		RefreshFilterOptionList();
		UpdateResetFilterButton();
		ScrollToSelectedComponent();
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	private void SetAlreadySelectFilterOption(List<Enum> alreadySelectFilterOption)
	{
		_selectFilterOption.Clear();
		if (__Extension_Method_ICollection__.COUNT(alreadySelectFilterOption) > 0)
		{
			_selectFilterOption.AddRange(alreadySelectFilterOption);
		}
	}

	private void SetRuneStatList()
	{
		IList<ClientStatDisplayScoreData> allData = GameRoot.Instance.ClientStatDisplayScoreDataContainer.GetAllData();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			ClientStatDisplayScoreData clientStatDisplayScoreData = allData[i];
			if (clientStatDisplayScoreData.ShowOptionIndex == 0)
			{
				continue;
			}
			if (StatTypeChecker.IsContainEqualStatGroup(clientStatDisplayScoreData.StatType))
			{
				StatType statType = StatTypeChecker.ReverseStatType(clientStatDisplayScoreData.StatType);
				if (_dicRuneAvailableStat.ContainsValue(statType))
				{
					continue;
				}
			}
			if (!_dicRuneAvailableStat.ContainsKey(clientStatDisplayScoreData.ShowOptionIndex))
			{
				_dicRuneAvailableStat.Add(clientStatDisplayScoreData.ShowOptionIndex, clientStatDisplayScoreData.StatType);
			}
		}
		IList<RelatedStatData> allData2 = GameRoot.Instance.RelatedStatDataContainer.GetAllData();
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(allData2); j++)
		{
			RelatedStatData relatedStatData = allData2[j];
			if (relatedStatData != null)
			{
				string optionName = relatedStatData.ChangedStatType.ToString();
				EquipOptionData dataByOptionName = GameRoot.Instance.EquipOptionDataContainer.GetDataByOptionName(optionName);
				if (dataByOptionName != null && !_dicRuneAvailableStat.ContainsKey(dataByOptionName.ShowOptionIndex))
				{
					_dicRuneAvailableStat.Add(dataByOptionName.ShowOptionIndex, relatedStatData.ChangedStatType);
				}
			}
		}
		_dicRuneAvailableStat.SortByKey((int lhs, int rhs) => lhs.CompareTo(rhs));
	}

	private void SetStatList()
	{
		FactoryBase.ReleaseAndClearList(_listOptionComponent);
		for (int i = 0; i < _dicRuneAvailableStat.Count; i++)
		{
			RuneSelectItemFilterMenuComponent runeSelectItemFilterMenuComponent = UIHelper.AddGridItem(GridOptionList, RuneSelectItemFilterMenuComponent.PrefabAuto);
			if (runeSelectItemFilterMenuComponent == null)
			{
				return;
			}
			Enum valueByIndex = _dicRuneAvailableStat.GetValueByIndex(i);
			runeSelectItemFilterMenuComponent.Open(this, valueByIndex, isFilter: false);
			_listOptionComponent.Add(runeSelectItemFilterMenuComponent);
		}
		UIHelper.RepositionGrid(GridOptionList);
	}

	private void ScrollToSelectedComponent()
	{
		RuneSelectItemFilterMenuComponent runeSelectItemFilterMenuComponent = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listOptionComponent); i++)
		{
			RuneSelectItemFilterMenuComponent runeSelectItemFilterMenuComponent2 = _listOptionComponent[i];
			if (runeSelectItemFilterMenuComponent2 != null && _selectFilterOption.Contains(runeSelectItemFilterMenuComponent2.FilterType))
			{
				runeSelectItemFilterMenuComponent = runeSelectItemFilterMenuComponent2;
				break;
			}
		}
		if (runeSelectItemFilterMenuComponent != null)
		{
			GridOptionList.ScrollTo(runeSelectItemFilterMenuComponent, 0f, 0f);
		}
	}

	private void Clear()
	{
		_dicRuneAvailableStat.Clear();
		_selectFilterOption.Clear();
		FactoryBase.ReleaseAndClearList(_listOptionComponent);
	}

	private void UpdateResetFilterButton()
	{
		bool enable = __Extension_Method_ICollection__.COUNT(_selectFilterOption) > 0;
		UIHelper.EnableButton(ButtonResetFilter, enable, grayscaleIfDisable: true);
	}

	private void RefreshFilterOptionList()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listOptionComponent); i++)
		{
			RuneSelectItemFilterMenuComponent runeSelectItemFilterMenuComponent = _listOptionComponent[i];
			if (runeSelectItemFilterMenuComponent != null)
			{
				bool filter = _selectFilterOption.Contains(runeSelectItemFilterMenuComponent.FilterType);
				runeSelectItemFilterMenuComponent.SetFilter(filter);
			}
		}
	}

	public void OnClickConfirm()
	{
		if (_listener != null)
		{
			_listener.OnClickConfirm(_selectFilterOption, Close);
		}
	}

	public void OnClickResetFilter()
	{
		_selectFilterOption.Clear();
		RefreshFilterOptionList();
		UpdateResetFilterButton();
	}

	void RuneSelectItemFilterMenuComponent.Listener.OnClickFilter(Enum filterType, bool isFilter)
	{
		if (isFilter)
		{
			if (!_selectFilterOption.Contains(filterType))
			{
				_selectFilterOption.Add(filterType);
			}
		}
		else
		{
			_selectFilterOption.Remove(filterType);
		}
		if (__Extension_Method_ICollection__.COUNT(_selectFilterOption) > _selectAbleCount)
		{
			_selectFilterOption.Remove(filterType);
		}
		RefreshFilterOptionList();
		UpdateResetFilterButton();
	}
}
