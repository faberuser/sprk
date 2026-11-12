using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NAffix;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NAffix;

public class BaseAffixPanel : BaseWindow, AffixSelectPopupComponent.IListener, IAffixDataProvider
{
	public interface IListener
	{
		void OnClickAlwaysSave(bool isSave);
	}

	public UIComboBox ComboBoxDifficulty;

	public AffixPanelToggle[] ToggleInfos;

	public UILabel AffixMultipleLabel;

	public UIScrollGrid SelectScrollGrid;

	public UIScrollGrid InfoScrollGrid;

	public UITable InfoTable;

	public GameObject InfoDescRoot;

	public UILabel CountLabel;

	public UILabel TotalPointLabel;

	public UILabel AdvantagePointLabel;

	public UILabel AdvantageCountLabel;

	public UILabel PenaltyPointLabel;

	public UILabel PenaltyCountLabel;

	public UIToggle AlwaysSaveToggle;

	public GameObject AlwaysSaveToogleSelectRoot;

	public UIButton ButtonReset;

	public UIButton ButtonCreateRoom;

	public UIButton ButtonSettingRoom;

	public List<AffixSelectPopupComponent> _listSelectComponent = new List<AffixSelectPopupComponent>();

	public List<AffixSelectPopupInfoComponent> _listInfoComponent = new List<AffixSelectPopupInfoComponent>();

	protected AffixDataContainer _container;

	protected IListener _listener;

	protected CampaignDungeonData _dungeonData;

	protected bool _initComboBoxDiffculty;

	protected EAffixWindowType _windowType;

	protected EAffixFilterType _filterType;

	protected bool _canChange = true;

	protected bool _initCategory;

	protected virtual void SetValues(int chapterIndex, int dungeonindex, bool canChange, EAffixWindowType windowType)
	{
		_dungeonData = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonindex);
		_windowType = windowType;
		_canChange = canChange;
		if (_dungeonData == null)
		{
			VespaDebug.LogError($"Not Exsit CampaginDungeonData {chapterIndex} : {dungeonindex} ");
		}
	}

	protected virtual void Release()
	{
		ReleaseInfoComponent();
		ReleaseSelectComponent();
		ReleaseComboBox();
		_container = null;
		_listener = null;
		_dungeonData = null;
		_canChange = true;
		_initComboBoxDiffculty = false;
		_filterType = EAffixFilterType.None;
		_initCategory = false;
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Release();
	}

	public void InitCategory()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleInfos); i++)
		{
			AffixPanelToggle affixPanelToggle = ToggleInfos[i];
			if (affixPanelToggle != null && !(affixPanelToggle.Toggle == null) && affixPanelToggle.FilterType == EAffixFilterType.All && !affixPanelToggle.Toggle.value)
			{
				affixPanelToggle.Toggle.value = true;
			}
		}
		_initCategory = true;
	}

	public virtual void OnChangedCategory(UIToggle toggle)
	{
		if (toggle == null || !_initCategory)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleInfos); i++)
		{
			AffixPanelToggle affixPanelToggle = ToggleInfos[i];
			if (affixPanelToggle != null && !(affixPanelToggle.Toggle == null) && !(affixPanelToggle.Toggle != toggle))
			{
				SetSelectComponent();
			}
		}
	}

	protected void SetContainer()
	{
		int affixGroupIndex = GetAffixGroupIndex();
		int clearPoint = GetClearPoint();
		if (_container == null)
		{
			_container = new AffixDataContainer(affixGroupIndex, clearPoint);
		}
		else
		{
			_container.ChangeGroupIndex(affixGroupIndex, clearPoint);
		}
	}

	protected void LoadAffixIndices(int[] affixindices)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(affixindices) && _windowType == EAffixWindowType.Create)
		{
			if (_dungeonData != null)
			{
				int chapterIndex = _dungeonData.ChapterIndex;
				int dungeonIndex = _dungeonData.DungeonIndex;
				int dungeonDifficulty = (int)GetDungeonDifficulty();
				int totalCount = GetTotalCount();
				affixindices = AffixHelper.LoadLocalStorageAffixIndices(chapterIndex, dungeonIndex, dungeonDifficulty, totalCount);
				_container.SelectAffixIndices(affixindices);
			}
		}
		else
		{
			_container.SelectAffixIndices(affixindices);
		}
	}

	protected EAffixFilterType GetCurrentFilterType()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ToggleInfos); i++)
		{
			AffixPanelToggle affixPanelToggle = ToggleInfos[i];
			if (affixPanelToggle != null)
			{
				UIToggle toggle = affixPanelToggle.Toggle;
				if (toggle != null && toggle.value)
				{
					return affixPanelToggle.FilterType;
				}
			}
		}
		return EAffixFilterType.All;
	}

	EAffixCompState IAffixDataProvider.GetAffixCompState(AffixData affixData)
	{
		if (_container == null)
		{
			return EAffixCompState.Disable;
		}
		return _container.GetAffixCompState(affixData);
	}

	protected IList<AffixData> GetAffixDataList(EAffixFilterType filterType = EAffixFilterType.All)
	{
		if (_container == null)
		{
			return null;
		}
		return _container.GetFilteredList(filterType);
	}

	protected List<AffixData> GetSelectAffixDatatList()
	{
		if (_container == null)
		{
			return null;
		}
		return _container.GetSelectList();
	}

	protected int GetTotalPoint()
	{
		if (_container == null)
		{
			return 0;
		}
		int clearPoint = _container.GetClearPoint();
		int sumPoint = _container.GetSumPoint();
		float affixMultiple = GetAffixMultiple();
		return (int)((float)(clearPoint + sumPoint) * affixMultiple);
	}

	protected int GetSelectSumPoint()
	{
		if (_container == null)
		{
			return 0;
		}
		return _container.GetSumPoint();
	}

	protected int GetAdvantagePoint()
	{
		if (_container == null)
		{
			return 0;
		}
		return _container.GetAdvantagePoint();
	}

	protected int GetPenaltyPoint()
	{
		if (_container == null)
		{
			return 0;
		}
		return _container.GetPenaltyPoint();
	}

	protected int GetSelectCount()
	{
		if (_container == null)
		{
			return 0;
		}
		return _container.GetSelectCount();
	}

	protected int GetTotalCount()
	{
		if (_container == null)
		{
			return 0;
		}
		return _container.GetTotalCount();
	}

	protected int GetAdvantageCount()
	{
		if (_container == null)
		{
			return 0;
		}
		return _container.GetAdvantageCount();
	}

	protected int GetPenaltyCount()
	{
		if (_container == null)
		{
			return 0;
		}
		return _container.GetPenaltyCount();
	}

	public int[] GetAffixIndices()
	{
		if (_container == null)
		{
			return null;
		}
		List<AffixData> selectList = _container.GetSelectList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(selectList))
		{
			return null;
		}
		int[] array = new int[__Extension_Method_ICollection__.COUNT(selectList)];
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(selectList); i++)
		{
			AffixData affixData = selectList[i];
			if (affixData != null)
			{
				array[i] = affixData.Index;
			}
		}
		return array;
	}

	protected void SetSelectComponent()
	{
		EAffixFilterType currentFilterType = GetCurrentFilterType();
		if (_filterType == currentFilterType)
		{
			return;
		}
		IList<AffixData> affixDataList = GetAffixDataList(currentFilterType);
		int num = __Extension_Method_ICollection__.COUNT(affixDataList);
		int num2 = __Extension_Method_ICollection__.COUNT(_listSelectComponent);
		int diff = num - num2;
		CreateOrReleaseComponents(_listSelectComponent, diff, AffixSelectPopupComponent.PrefabAuto, SelectScrollGrid);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(affixDataList) && i < __Extension_Method_ICollection__.COUNT(_listSelectComponent); i++)
		{
			AffixData affixData = affixDataList[i];
			AffixSelectPopupComponent affixSelectPopupComponent = _listSelectComponent[i];
			if (affixData != null && !(affixSelectPopupComponent == null))
			{
				affixSelectPopupComponent.Open(this, this, affixData);
			}
		}
		UIHelper.RepositionGrid(SelectScrollGrid);
	}

	protected void ReleaseSelectComponent()
	{
		FactoryBase.ReleaseAndClearList(_listSelectComponent);
	}

	protected void RefreshSelectComponents()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listSelectComponent); i++)
		{
			AffixSelectPopupComponent affixSelectPopupComponent = _listSelectComponent[i];
			if (!(affixSelectPopupComponent == null))
			{
				affixSelectPopupComponent.Refresh();
			}
		}
	}

	protected void SetInfoComponent()
	{
		if (InfoTable == null)
		{
			return;
		}
		List<AffixData> selectAffixDatatList = GetSelectAffixDatatList();
		int num = __Extension_Method_ICollection__.COUNT(selectAffixDatatList);
		int num2 = __Extension_Method_ICollection__.COUNT(_listInfoComponent);
		int diff = num - num2;
		Transform parentTm = InfoTable.transform;
		if (num == 0)
		{
			RepositionInfoScrollGrid();
		}
		UIHelper.EnableGameObject(InfoDescRoot, num == 0);
		CreateOrReleaseComponents(_listInfoComponent, diff, AffixSelectPopupInfoComponent.PrefabAuto2, parentTm);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(selectAffixDatatList) && i < __Extension_Method_ICollection__.COUNT(_listInfoComponent); i++)
		{
			AffixData affixData = selectAffixDatatList[i];
			if (affixData != null)
			{
				AffixSelectPopupInfoComponent affixSelectPopupInfoComponent = _listInfoComponent[i];
				if (!(affixSelectPopupInfoComponent == null))
				{
					affixSelectPopupInfoComponent.Open(this, this, affixData);
				}
			}
		}
		InfoTable.Reposition();
		InfoTable.repositionNow = true;
	}

	protected void RepositionInfoScrollGrid()
	{
		if (InfoScrollGrid != null)
		{
			UIHelper.RepositionGrid(InfoScrollGrid);
		}
	}

	protected void ReleaseInfoComponent()
	{
		RepositionInfoScrollGrid();
		FactoryBase.ReleaseAndClearList(_listInfoComponent);
	}

	protected virtual void InitDifficultyComp()
	{
		if (!(ComboBoxDifficulty == null) && !_initComboBoxDiffculty)
		{
			_initComboBoxDiffculty = true;
			for (int i = 0; i <= 3; i++)
			{
				ComboBoxDifficulty.AddItem(i, "");
			}
			ComboBoxDifficulty.OnChanged = null;
			ComboBoxDifficulty.SelectFirstItem();
			ComboBoxDifficulty.OnChanged = OnChangedDifficulty;
			ComboBoxDifficulty.Reposition();
		}
	}

	protected void ReleaseComboBox()
	{
		if (ComboBoxDifficulty != null)
		{
			ComboBoxDifficulty.Clear();
		}
	}

	protected virtual void OnChangedDifficulty(int difficulty)
	{
	}

	protected int GetComboboxCurrentKey()
	{
		if (ComboBoxDifficulty == null)
		{
			return 0;
		}
		return ComboBoxDifficulty.CurrentKey;
	}

	protected void SetInfoLabels()
	{
		float affixMultiple = GetAffixMultiple();
		UIHelper.SetLabelText(AffixMultipleLabel, $"x {affixMultiple:F1}");
		int totalPoint = GetTotalPoint();
		int selectSumPoint = GetSelectSumPoint();
		UIHelper.SetLabelText(TotalPointLabel, $"{totalPoint} [3BE794]({selectSumPoint})[-]");
		int totalCount = GetTotalCount();
		int selectCount = GetSelectCount();
		UIHelper.SetLabelText(CountLabel, $"{selectCount}/{totalCount}");
		int advantagePoint = GetAdvantagePoint();
		int advantageCount = GetAdvantageCount();
		UIHelper.SetLabelText(AdvantageCountLabel, advantageCount);
		UIHelper.SetLabelText(AdvantagePointLabel, string.Format($"({advantagePoint}p)"));
		int penaltyPoint = GetPenaltyPoint();
		int penaltyCount = GetPenaltyCount();
		UIHelper.SetLabelText(PenaltyCountLabel, penaltyCount);
		UIHelper.SetLabelText(PenaltyPointLabel, string.Format($"({penaltyPoint}p)"));
	}

	protected void InitAlwaysSaveToggle()
	{
		bool enable = _windowType == EAffixWindowType.Create && _canChange;
		if (AlwaysSaveToggle != null)
		{
			UIHelper.EnableToggle(AlwaysSaveToggle, enable: true, grayscale: true);
			AlwaysSaveToggle.value = GetLocalStarogeAlwaysSave();
			ShowToggleSelectRoot(AlwaysSaveToggle, AlwaysSaveToogleSelectRoot);
		}
		UIHelper.EnableToggle(AlwaysSaveToggle, enable, grayscale: true);
	}

	protected void ShowToggleSelectRoot(UIToggle toggle, GameObject selectRoot)
	{
		if (!(toggle == null) && !(selectRoot == null))
		{
			bool value = toggle.value;
			UIHelper.EnableGameObject(selectRoot, value);
		}
	}

	protected virtual bool GetLocalStarogeAlwaysSave()
	{
		return AffixHelper.GetAlwaysSave();
	}

	public void SaveOrResetAffixIndices()
	{
		if (_windowType == EAffixWindowType.Create && _dungeonData != null)
		{
			int chapterIndex = _dungeonData.ChapterIndex;
			int dungeonIndex = _dungeonData.DungeonIndex;
			int dungeonDifficulty = (int)GetDungeonDifficulty();
			int totalCount = GetTotalCount();
			int[] affixIndices = (GetLocalStarogeAlwaysSave() ? GetAffixIndices() : null);
			AffixHelper.SaveAffixIndices(chapterIndex, dungeonIndex, dungeonDifficulty, affixIndices, totalCount);
		}
	}

	public void InitButtons()
	{
		UIHelper.EnableGameObject(ButtonCreateRoom, _windowType == EAffixWindowType.Create || _windowType == EAffixWindowType.Challenge);
		UIHelper.EnableGameObject(ButtonSettingRoom, _windowType == EAffixWindowType.Setting);
		UIHelper.EnableButton(ButtonCreateRoom, _canChange, grayscaleIfDisable: true);
		UIHelper.EnableButton(ButtonSettingRoom, _canChange, grayscaleIfDisable: true);
		UIHelper.EnableButton(ButtonReset, _canChange, grayscaleIfDisable: true);
	}

	protected virtual DungeonDifficultyType GetDungeonDifficulty()
	{
		return DungeonDifficultyType.Easy;
	}

	protected virtual int GetAffixGroupIndex()
	{
		if (_dungeonData == null)
		{
			return 0;
		}
		DungeonDifficultyType dungeonDifficulty = GetDungeonDifficulty();
		return _dungeonData.GetAffixGroupIndex(dungeonDifficulty);
	}

	protected virtual float GetAffixMultiple()
	{
		if (_dungeonData == null)
		{
			return 0f;
		}
		DungeonDifficultyType dungeonDifficulty = GetDungeonDifficulty();
		return (float)_dungeonData.GetAffixMultiple(dungeonDifficulty) / 1000f;
	}

	protected virtual int GetClearPoint()
	{
		if (_dungeonData == null)
		{
			return 0;
		}
		DungeonDifficultyType dungeonDifficulty = GetDungeonDifficulty();
		return _dungeonData.GetClearPoint(dungeonDifficulty);
	}

	protected void CreateOrReleaseComponents<T>(List<T> listComp, int diff, Action createAction) where T : Component
	{
		if (listComp == null || createAction == null || diff == 0)
		{
			return;
		}
		int num = System.Math.Abs(diff);
		for (int i = 0; i < num; i++)
		{
			if (diff < 0)
			{
				int num2 = listComp.Count - 1;
				if (num2 < 0)
				{
					break;
				}
				T comp = listComp[num2];
				listComp.RemoveAt(num2);
				FactoryBase.ReleaseComponent(comp);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(createAction);
			}
		}
	}

	protected void CreateOrReleaseComponents<T>(List<T> listComp, int diff, PrefabAuto<T> prefabAuto, IGrid grid) where T : Component
	{
		if (prefabAuto == null || grid == null)
		{
			return;
		}
		CreateOrReleaseComponents(listComp, diff, delegate
		{
			T val = UIHelper.AddGridItem(grid, prefabAuto);
			if (!(val == null))
			{
				listComp.Add(val);
			}
		});
	}

	protected void CreateOrReleaseComponents<T>(List<T> listComp, int diff, PrefabAuto<T> prefabAuto, Transform parentTm) where T : Component
	{
		if (prefabAuto == null || parentTm == null)
		{
			return;
		}
		CreateOrReleaseComponents(listComp, diff, delegate
		{
			T val = prefabAuto.Factory.Create(parentTm);
			if (!(val == null))
			{
				listComp.Add(val);
			}
		});
	}

	protected virtual void OnClickSelectPopupComp(AffixSelectPopupComponent caller)
	{
		AffixData affixData = caller.GetAffixData();
		_container.Select(affixData);
		RefreshSelectComponents();
	}

	void AffixSelectPopupComponent.IListener.OnClickComponent(AffixSelectPopupComponent caller)
	{
		if (_canChange)
		{
			OnClickSelectPopupComp(caller);
		}
	}

	public void OnChangedAlwaysSaveToggle(UIToggle toggle)
	{
		if (!(toggle == null))
		{
			ShowToggleSelectRoot(toggle, AlwaysSaveToogleSelectRoot);
			if (_windowType == EAffixWindowType.Create)
			{
				AffixHelper.SetAlwaysSave(toggle.value);
			}
		}
	}

	public void OnClickReset()
	{
		_container.ResetSelectAffix();
		RefreshSelectComponents();
		SetInfoComponent();
		SetInfoLabels();
	}
}
