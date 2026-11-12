using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroLimitBreakExpItemUsePopup : BaseWindow
{
	public class OpenData
	{
		public class InputItemData
		{
			public int ItemIndex { get; set; }

			public int MaxCount { get; set; }

			public long AddExp { get; set; }
		}

		public InputItemData[] InputItemDatas { get; set; }

		public long CurrentExp { get; set; }

		public long TotalExp { get; set; }

		public Action<Dictionary<int, int>> OnUse { get; set; }
	}

	[Serializable]
	public class MaterialBindingInfo
	{
		public GameObject Root;

		public GameObject IconRoot;

		public UILabel NameLabel;

		public UIInput NumberInput;

		public UIButton MinusButton;

		public UIButton PlusButton;

		private HeroLimitBreakExpItemUsePopup _popup;

		private ItemIconComponent _itemIconComponent;

		public OpenData.InputItemData InputItemData { get; private set; }

		public void Init(OpenData.InputItemData inputItemData, HeroLimitBreakExpItemUsePopup popup)
		{
			if (inputItemData == null)
			{
				UIHelper.EnableGameObject(Root, enable: false);
				return;
			}
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(inputItemData.ItemIndex);
			if (data == null)
			{
				UIHelper.EnableGameObject(Root, enable: false);
				return;
			}
			Close();
			InputItemData = inputItemData;
			_popup = popup;
			UIHelper.EnableGameObject(Root, enable: true);
			NumberInput.value = 0.ToString();
			NumberInput.onSubmit.Add(new EventDelegate(delegate
			{
				_popup.ValidateAllMaterialCount(this);
				RefreshItemCount();
			}));
			if ((bool)IconRoot)
			{
				_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(IconRoot.transform);
				_itemIconComponent.ResetPosition();
				_itemIconComponent.Open(data);
			}
			if ((bool)NameLabel)
			{
				UIHelper.SetLabelText(NameLabel, data.Name);
			}
			if ((bool)MinusButton)
			{
				MinusButton.onClick.Clear();
				MinusButton.onClick.Add(new EventDelegate(delegate
				{
					SetNumber(Mathf.Clamp(GetNumber() - 1, 0, inputItemData.MaxCount));
					_popup.ValidateAllMaterialCount(this);
					RefreshItemCount();
				}));
			}
			if ((bool)PlusButton)
			{
				PlusButton.onClick.Clear();
				PlusButton.onClick.Add(new EventDelegate(delegate
				{
					SetNumber(Mathf.Clamp(GetNumber() + 1, 0, inputItemData.MaxCount));
					_popup.ValidateAllMaterialCount(this);
					RefreshItemCount();
				}));
			}
			RefreshItemCount();
		}

		public void SetNumber(int number)
		{
			if ((bool)NumberInput)
			{
				NumberInput.value = number.ToString();
				RefreshItemCount();
			}
		}

		public int GetNumber()
		{
			int result = 0;
			string text = ((NumberInput != null) ? NumberInput.value : null);
			if (!string.IsNullOrEmpty(text))
			{
				int.TryParse(text, out result);
			}
			return result;
		}

		public void RefreshItemCount()
		{
			if ((bool)_itemIconComponent)
			{
				int num = Math.Max(InputItemData.MaxCount - GetNumber(), 0);
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(InputItemData.ItemIndex);
				bool flag = data != null && data.ItemMaxCap != 0 && num > data.ItemMaxCap;
				_itemIconComponent.SetItemCount_anycount(num, flag ? Color.red : Color.white);
			}
			_popup.RefreshExpGauge();
		}

		public void Close()
		{
			FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroLimitBreakExpItemUsePopup";

	[SerializeField]
	private MaterialBindingInfo[] _materials;

	[SerializeField]
	private UIClipSprite _expGaugeSprite;

	[SerializeField]
	private UILabel _expGaugeLabel;

	private OpenData _openData;

	public void Open(OpenData openData)
	{
		base.Open();
		_openData = openData;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_materials); i++)
		{
			OpenData.InputItemData inputItemData = ((i < __Extension_Method_ICollection__.COUNT(openData.InputItemDatas)) ? openData.InputItemDatas[i] : null);
			_materials[i].Init(inputItemData, this);
		}
		RefreshExpGauge();
	}

	private void ValidateAllMaterialCount(MaterialBindingInfo mainMaterial)
	{
		long selectedExp = 0L;
		ValidateSingleMaterialCount(mainMaterial, makeMax: false, ref selectedExp);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_materials); i++)
		{
			MaterialBindingInfo materialBindingInfo = _materials[i];
			if (materialBindingInfo != mainMaterial)
			{
				ValidateSingleMaterialCount(materialBindingInfo, makeMax: false, ref selectedExp);
			}
		}
	}

	private void ValidateSingleMaterialCount(MaterialBindingInfo material, bool makeMax, ref long selectedExp)
	{
		if (material != null)
		{
			int val = (int)Math.Ceiling((double)(_openData.TotalExp - _openData.CurrentExp - selectedExp) / (double)material.InputItemData.AddExp);
			val = Math.Min(val, material.InputItemData.MaxCount);
			int num = material.GetNumber();
			if (num > val || makeMax)
			{
				num = val;
				material.SetNumber(num);
			}
			else if (num < 0)
			{
				num = 0;
				material.SetNumber(num);
			}
			selectedExp += num * material.InputItemData.AddExp;
		}
	}

	private void RefreshExpGauge()
	{
		long currentUseExp = GetCurrentUseExp();
		float fillAmount = ((_openData.TotalExp > 0) ? ((float)currentUseExp / (float)_openData.TotalExp) : 0f);
		UIHelper.SetSpriteFillAmount(_expGaugeSprite, fillAmount);
		if (currentUseExp >= _openData.TotalExp)
		{
			UIHelper.SetLabelText(_expGaugeLabel, "MAX");
		}
		else
		{
			UIHelper.SetLabelText(_expGaugeLabel, currentUseExp + "/" + _openData.TotalExp);
		}
	}

	private long GetCurrentUseExp()
	{
		long num = _openData.CurrentExp;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_materials); i++)
		{
			OpenData.InputItemData inputItemData = ((i < __Extension_Method_ICollection__.COUNT(_openData.InputItemDatas)) ? _openData.InputItemDatas[i] : null);
			if (inputItemData != null)
			{
				int number = _materials[i].GetNumber();
				long num2 = inputItemData.AddExp * number;
				num += num2;
			}
		}
		return num;
	}

	public bool IsMaxUseExp()
	{
		return GetCurrentUseExp() >= _openData.TotalExp;
	}

	public override void OnClosing()
	{
		base.OnClosing();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_materials); i++)
		{
			_materials[i].Close();
		}
	}

	public void OnClickMax()
	{
		long selectedExp = 0L;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_materials); i++)
		{
			MaterialBindingInfo materialBindingInfo = _materials[i];
			if (materialBindingInfo != null)
			{
				ValidateSingleMaterialCount(materialBindingInfo, makeMax: true, ref selectedExp);
			}
		}
	}

	public void OnClickYes()
	{
		if (_openData.OnUse != null)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_materials); i++)
			{
				MaterialBindingInfo materialBindingInfo = _materials[i];
				OpenData.InputItemData inputItemData = ((i < __Extension_Method_ICollection__.COUNT(_openData.InputItemDatas)) ? _openData.InputItemDatas[i] : null);
				if (inputItemData != null)
				{
					int number = materialBindingInfo.GetNumber();
					if (number > 0)
					{
						dictionary[inputItemData.ItemIndex] = number;
					}
				}
			}
			_openData.OnUse(dictionary);
		}
		Close();
	}

	public override void OnSystemBackButton()
	{
		Close();
	}
}
