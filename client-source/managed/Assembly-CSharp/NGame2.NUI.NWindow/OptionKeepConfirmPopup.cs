using System;
using System.Collections.Generic;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class OptionKeepConfirmPopup : BaseWindow, OptionGradeComponent.Listener, OptionSellComponent.Listener
{
	public class LocalSettingData
	{
		private LocalAccountInt _tierSelect;

		private LocalAccountInt _starSelect;

		private LocalAccountBool _enhancedEquip;

		private LocalAccountBool _enhanceStone;

		private LocalAccountBool _equipRuneItem;

		private LocalAccountBool _EnchantedItem;

		private Dictionary<int, LocalAccountBool> _gradeSelectDic = new Dictionary<int, LocalAccountBool>();

		private const int Version = 2;

		private bool IsKeep;

		public int TierSelect
		{
			get
			{
				return _tierSelect.Value;
			}
			set
			{
				_tierSelect.Value = value;
			}
		}

		public int StarSelect
		{
			get
			{
				return _starSelect.Value;
			}
			set
			{
				_starSelect.Value = value;
			}
		}

		public bool EnhancedEquip
		{
			get
			{
				return _enhancedEquip.Value;
			}
			set
			{
				_enhancedEquip.Value = value;
			}
		}

		public bool EnhanceStone
		{
			get
			{
				return _enhanceStone.Value;
			}
			set
			{
				_enhanceStone.Value = value;
			}
		}

		public bool EquipRuneItem
		{
			get
			{
				if (_equipRuneItem == null)
				{
					return false;
				}
				return _equipRuneItem.Value;
			}
			set
			{
				if (_equipRuneItem != null)
				{
					_equipRuneItem.Value = value;
				}
			}
		}

		public bool EnchantedItem
		{
			get
			{
				if (_EnchantedItem == null)
				{
					return false;
				}
				return _EnchantedItem.Value;
			}
			set
			{
				if (_EnchantedItem != null)
				{
					_EnchantedItem.Value = value;
				}
			}
		}

		public void Init()
		{
			string text = typeof(OptionKeepConfirmPopup).FullName + (IsKeep ? "Keep" : "Bring") + 2;
			_tierSelect = new LocalAccountInt(0, text + "_tierSelect");
			_starSelect = new LocalAccountInt(0, text + "_starSelect");
			_enhancedEquip = new LocalAccountBool(i_default: false, text + "_enhancedEquip");
			_enhanceStone = new LocalAccountBool(i_default: false, text + "_enhanceStone");
			_equipRuneItem = new LocalAccountBool(i_default: false, text + "_equipRuneItem");
			_EnchantedItem = new LocalAccountBool(i_default: false, text + "_EnchantedItem");
			if (GameRoot.isValidInstance && GameRoot.Instance.OptionSellDataContainer != null)
			{
				OptionSellData[] categoryData = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.Grade);
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(categoryData); i++)
				{
					OptionSellData optionSellData = categoryData[i];
					LocalAccountBool value = new LocalAccountBool(i_default: false, text + "_gradeSelect" + optionSellData.Value);
					_gradeSelectDic.Add(optionSellData.Value, value);
				}
			}
		}

		public LocalSettingData(bool isKeep)
		{
			IsKeep = isKeep;
		}

		public bool GetGradeSelect(int grade)
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_gradeSelectDic, grade, null)?.Value ?? false;
		}

		public void SetGradeSelect(int grade, bool isSelect)
		{
			LocalAccountBool vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_gradeSelectDic, grade, null);
			if (vALUE != null)
			{
				vALUE.Value = isSelect;
			}
		}
	}

	public interface Listener
	{
		void RequestItemInfos(LocalSettingData data, List<int> gradeList, int minTier, int minStar, List<EquipItemInfo> equipItemInfos, bool isKeep);

		bool IsEquipItemExist(int option, bool isGrade);

		void OnClickKeep(IEnumerable<EquipItemInfo> equipItemInfos);

		void OnClickBring(IEnumerable<EquipItemInfo> equipItemInfos);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/OptionKeepConfirmPopup";

	public UISimpleGrid GradeGrid2;

	public UISimpleGrid TierHead;

	public UISimpleGrid TierGrid;

	public UISimpleGrid StarHead;

	public UISimpleGrid StarGrid;

	public UIPopupRoot TierRoot;

	public UIPopupRoot StarRoot;

	public UIGrid OptionGrid;

	public UISprite SpriteEnhancedEquip;

	public UISprite SpriteEnhanceStone;

	public UISprite SpriteEquipRuneItem;

	public UISprite SpriteEnchantedItem;

	public UILabel LabelNumItem;

	private LocalSettingData localSetting;

	private Listener _listener;

	private bool _isKeep;

	protected List<EquipItemInfo> _equipItemInfos = new List<EquipItemInfo>();

	protected List<OptionGradeComponent> _gradeList = new List<OptionGradeComponent>();

	protected OptionSellComponent _tierHead;

	protected List<OptionSellComponent> _tierList = new List<OptionSellComponent>();

	protected OptionSellComponent _starHead;

	protected List<OptionSellComponent> _starList = new List<OptionSellComponent>();

	private LocalSettingData LocalSetting
	{
		get
		{
			if (localSetting == null)
			{
				localSetting = new LocalSettingData(_isKeep);
				localSetting.Init();
			}
			return localSetting;
		}
		set
		{
			localSetting = value;
		}
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_gradeList);
		FactoryBase.SafeReleaseComponent(ref _tierHead);
		FactoryBase.SafeReleaseComponent(ref _starHead);
		FactoryBase.ReleaseAndClearList(_tierList);
		FactoryBase.ReleaseAndClearList(_starList);
	}

	public void Init(Listener listener, bool isKeep)
	{
		_listener = listener;
		_isKeep = isKeep;
		localSetting = LocalSetting;
		InitUI();
	}

	private void InitUI()
	{
		UIHelper.EnableGameObject(SpriteEnhancedEquip, LocalSetting.EnhancedEquip);
		UIHelper.EnableGameObject(SpriteEnhanceStone, LocalSetting.EnhanceStone);
		UIHelper.EnableGameObject(SpriteEquipRuneItem, LocalSetting.EquipRuneItem);
		UIHelper.EnableGameObject(SpriteEnchantedItem, LocalSetting.EnchantedItem);
		if (OptionGrid != null)
		{
			OptionGrid.Reposition();
		}
		UIHelper.SetLabelText(LabelNumItem, "0");
		OptionSellData[] categoryData = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.Tier);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(categoryData))
		{
			return;
		}
		OptionSellData[] categoryData2 = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.Grade);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(categoryData2))
		{
			return;
		}
		OptionSellData[] categoryData3 = GameRoot.Instance.OptionSellDataContainer.GetCategoryData(OptionSellCategory.Star);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(categoryData3))
		{
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(categoryData, LocalSetting.TierSelect))
			{
				LocalSetting.TierSelect = 0;
			}
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(categoryData3, LocalSetting.StarSelect))
			{
				LocalSetting.StarSelect = 0;
			}
			Array.Sort(categoryData2, delegate(OptionSellData a, OptionSellData b)
			{
				ItemGradeData data = GameRoot.Instance.ItemGradeDataContainer.GetData(a.Value);
				ItemGradeData data2 = GameRoot.Instance.ItemGradeDataContainer.GetData(b.Value);
				byte b2 = data?.Score ?? 1;
				byte value = data2?.Score ?? 1;
				return b2.CompareTo(value);
			});
			for (int num = 0; num < __Extension_Method_ICollection__.COUNT(categoryData2); num++)
			{
				OptionSellData optionSellData = categoryData2[num];
				bool gradeSelect = LocalSetting.GetGradeSelect(optionSellData.Value);
				OptionGradeComponent optionGradeComponent = UIHelper.AddGridItem(GradeGrid2, OptionGradeComponent.PrefabAuto);
				optionGradeComponent.Open(this, optionSellData, gradeSelect);
				_gradeList.Add(optionGradeComponent);
			}
			UIHelper.RepositionGrid(GradeGrid2);
			int width = 0;
			UIWidget component = TierHead.GetComponent<UIWidget>();
			if (component != null)
			{
				width = component.width;
			}
			_tierHead = UIHelper.AddGridItem(TierHead, OptionSellComponent.PrefabAuto);
			_tierHead.Open(this, categoryData[LocalSetting.TierSelect], onHead: true, width);
			for (int num2 = 0; num2 < categoryData.Length; num2++)
			{
				OptionSellComponent optionSellComponent = UIHelper.AddGridItem(TierGrid, OptionSellComponent.PrefabAuto);
				optionSellComponent.Open(this, categoryData[num2], onHead: false, width);
				_tierList.Add(optionSellComponent);
			}
			UIHelper.RepositionGrid(TierHead);
			UIHelper.RepositionGrid(TierGrid);
			UIWidget component2 = StarHead.GetComponent<UIWidget>();
			if (component2 != null)
			{
				width = component2.width;
			}
			_starHead = UIHelper.AddGridItem(StarHead, OptionSellComponent.PrefabAuto);
			_starHead.Open(this, categoryData3[LocalSetting.StarSelect], onHead: true, width);
			for (int num3 = 0; num3 < categoryData3.Length; num3++)
			{
				OptionSellComponent optionSellComponent2 = UIHelper.AddGridItem(StarGrid, OptionSellComponent.PrefabAuto);
				optionSellComponent2.Open(this, categoryData3[num3], onHead: false, width);
				_starList.Add(optionSellComponent2);
			}
			UIHelper.RepositionGrid(StarHead);
			UIHelper.RepositionGrid(StarGrid);
			UpdateList();
		}
	}

	protected virtual void UpdateList()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < _gradeList.Count; i++)
		{
			OptionGradeComponent optionGradeComponent = _gradeList[i];
			if (optionGradeComponent.IsChecked)
			{
				list.Add(optionGradeComponent.OptionData.Value);
			}
		}
		int minTier = ((!(_tierHead != null)) ? 1 : _tierHead.GetValue());
		int minStar = ((_starHead != null) ? _starHead.GetValue() : 0);
		_listener.RequestItemInfos(LocalSetting, list, minTier, minStar, _equipItemInfos, _isKeep);
		UIHelper.SetLabelText(LabelNumItem, _equipItemInfos.Count.ToString());
		UpdateList(_equipItemInfos);
	}

	protected virtual void UpdateList(List<EquipItemInfo> equipItemInfos)
	{
		int num = 0;
		int num2 = 0;
		foreach (EquipItemInfo equipItemInfo in equipItemInfos)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfo.ItemIndex);
			if (data != null)
			{
				num += data.SellGold;
				num2 += GlobalAutoBehaviour<DropBonusEventManager>.instance.GetCheckEquipSellEventBonus(data);
			}
		}
		SetLabelStatusEquipEventBonus();
	}

	private void SetLabelStatusEquipEventBonus()
	{
		GlobalAutoBehaviour<DropBonusEventManager>.instance.IsEffectiveEvent(EventBonusType.BonusEquipSellReward);
	}

	public void OnClickEnhancedEquip()
	{
		LocalSetting.EnhancedEquip = !LocalSetting.EnhancedEquip;
		UIHelper.EnableGameObject(SpriteEnhancedEquip, LocalSetting.EnhancedEquip);
		UpdateList();
	}

	public void OnClickEnhanceStone()
	{
		LocalSetting.EnhanceStone = !LocalSetting.EnhanceStone;
		UIHelper.EnableGameObject(SpriteEnhanceStone, LocalSetting.EnhanceStone);
		UpdateList();
	}

	public void OnClickEquipRuneItem()
	{
		LocalSetting.EquipRuneItem = !LocalSetting.EquipRuneItem;
		UIHelper.EnableGameObject(SpriteEquipRuneItem, LocalSetting.EquipRuneItem);
		UpdateList();
	}

	public void OnClickEnchantedItem()
	{
		LocalSetting.EnchantedItem = !LocalSetting.EnchantedItem;
		UIHelper.EnableGameObject(SpriteEnchantedItem, LocalSetting.EnchantedItem);
		UpdateList();
	}

	public void OnClickConfirm()
	{
		if (_isKeep)
		{
			_listener.OnClickKeep(_equipItemInfos);
		}
		else
		{
			_listener.OnClickBring(_equipItemInfos);
		}
	}

	public void OnClickClose()
	{
		Close();
	}

	void OptionGradeComponent.Listener.OnClick(OptionSellData optionData, bool isChecked)
	{
		LocalSetting.SetGradeSelect(optionData.Value, isChecked);
		UpdateList();
	}

	List<int> OptionSellComponent.Listener.GetSelectedGrade()
	{
		List<int> list = new List<int>();
		foreach (OptionGradeComponent grade in _gradeList)
		{
			if (LocalSetting.GetGradeSelect(grade.OptionData.Value))
			{
				list.Add(grade.OptionData.Value);
			}
		}
		return list;
	}

	void OptionSellComponent.Listener.OnClick(OptionSellData optionData, bool onHead)
	{
		if (onHead)
		{
			switch (optionData.Category)
			{
			case OptionSellCategory.Tier:
				TierRoot.TogglePopup();
				break;
			case OptionSellCategory.Star:
				StarRoot.TogglePopup();
				break;
			}
			return;
		}
		switch (optionData.Category)
		{
		case OptionSellCategory.Tier:
		{
			int num2 = _tierList.FindIndex((OptionSellComponent sellComponent) => sellComponent.OptionData == optionData);
			if (num2 >= 0)
			{
				LocalSetting.TierSelect = num2;
			}
			_tierHead.Refresh(optionData);
			break;
		}
		case OptionSellCategory.Star:
		{
			int num = _starList.FindIndex((OptionSellComponent sellComponent) => sellComponent.OptionData == optionData);
			if (num >= 0)
			{
				LocalSetting.StarSelect = num;
			}
			_starHead.Refresh(optionData);
			break;
		}
		}
		UpdateList();
	}
}
