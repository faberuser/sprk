using System;
using System.Collections.Generic;
using NGame2.NUI.NManager;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MessageOpenCS : BaseWindow
{
	[Serializable]
	public class Agreement
	{
		public UILabel Label;

		public GameObject CheckBox;

		public UIScrollView ScrollView;

		public void SetCheckBox(bool enable)
		{
			UIHelper.EnableGameObject(CheckBox, enable);
		}

		public void SetRepositionNow()
		{
			ScrollView.ResetPosition();
		}
	}

	[Serializable]
	public class Template
	{
		public Transform Root;

		public Agreement[] Agreement;

		public void InitCheckbox()
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Agreement); i++)
			{
				Agreement agreement = Agreement[i];
				if (agreement != null)
				{
					agreement.ScrollView.MoveAbsolute(Vector3.zero);
					agreement.SetCheckBox(enable: false);
				}
			}
		}

		public void SetRepositionNow()
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Agreement); i++)
			{
				Agreement[i]?.SetRepositionNow();
			}
		}

		public void SetLabel(string text)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Agreement); i++)
			{
				Agreement agreement = Agreement[i];
				if (agreement != null)
				{
					UIHelper.SetLabelText(agreement.Label, text);
				}
			}
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/MessagePopupCS2";

	private static GlobalWindowPrefab<MessageOpenCS> _windowPrefab = new GlobalWindowPrefab<MessageOpenCS>("Resources/Prefab2/Window/MessagePopup/MessagePopupCS2", GlobalUIRoot.ELayerType.GlobalUI);

	[Header("[MessageOpenCS]")]
	public UILabel Title;

	public UILabel CheckBoxTitleLabel;

	public UIButton ConfirmButton;

	public UIButton CloseButton;

	public GameObject CategoryRoot;

	public UIComboBox ComboBoxCategory;

	public Template TemplatesSingleAgreement;

	private Action<int> _onConfirm;

	private Template _activatedTemplate;

	private int _selectedCategoryIndex = -1;

	private List<CSMailData> _csMailDataList = new List<CSMailData>();

	private void Start()
	{
		if ((bool)ComboBoxCategory.Grid)
		{
			ComboBoxCategory.Grid.onCustomSort = csMailDataIndexSort;
		}
	}

	private int csMailDataIndexSort(Transform a, Transform b)
	{
		int num = 0;
		num = LeftMenu.NullCompare(a, b);
		if (num != 0)
		{
			return num;
		}
		UIComboBoxItem component = a.GetComponent<UIComboBoxItem>();
		UIComboBoxItem component2 = b.GetComponent<UIComboBoxItem>();
		num = LeftMenu.NullCompare(component, component2);
		if (num != 0)
		{
			return num;
		}
		int key = component.Key;
		int key2 = component2.Key;
		num = LeftMenu.NullCompare(key, key2);
		if (num != 0)
		{
			return num;
		}
		return key2.CompareTo(key);
	}

	public static void OpenCS2(string title, string message, Action<int> onYes, Action onNo)
	{
		WindowPrefab<MessageOpenCS> windowPrefab = new WindowPrefab<MessageOpenCS>("Resources/Prefab2/Window/MessagePopup/MessagePopupCS2", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.Open(accepted: false, title, message, onYes);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void OpenGlobal(string title, string message, Action<int> onYes, Action onNo, string changedCheckBoxTitle = "", bool isHideComboBox = false)
	{
		_windowPrefab.instance.Open(accepted: false, title, message, onYes, changedCheckBoxTitle, isHideComboBox);
	}

	public void Open(bool accepted, string title, string text, Action<int> onConfirm, string changedCheckBoxTitle = "", bool isHideComboBox = false)
	{
		Open(delegate
		{
			Init(accepted, title, text, onConfirm, changedCheckBoxTitle, isHideComboBox);
		}, null);
	}

	public void Init(bool accepted, string title, string text, Action<int> onConfirm, string changedCheckBoxTitle = "", bool isHideComboBox = false)
	{
		_onConfirm = onConfirm;
		UIHelper.SetLabelText(Title, title);
		InitAgreementText(text);
		string text2 = (String_Extension_Method.IsNullOrEmpty(changedCheckBoxTitle) ? LocalizationManager.GetStringDirect("CS_POPUP_AGREE") : changedCheckBoxTitle);
		UIHelper.SetLabelText(CheckBoxTitleLabel, text2);
		if (isHideComboBox)
		{
			UIHelper.EnableGameObject(CategoryRoot, enable: false);
			UIHelper.EnableGameObject(ComboBoxCategory.gameObject, enable: false);
			_selectedCategoryIndex = 0;
			ComboBoxCategory.Clear();
			Vespa_Generic_ICollection_Extension_Method.CLEAR(_csMailDataList);
		}
		else
		{
			UIHelper.EnableGameObject(CategoryRoot, enable: true);
			UIHelper.EnableGameObject(ComboBoxCategory.gameObject, enable: true);
			InitComboBoxCategory();
		}
		if (accepted || _activatedTemplate == null)
		{
			Vespa_System_Action_Extension_Method.fire(_onConfirm, _selectedCategoryIndex);
		}
	}

	public override void Reset()
	{
		base.Reset();
		_onConfirm = null;
		UIHelper.EnableGameObject(TemplatesSingleAgreement.Root, enable: false);
	}

	private void InitAgreementText(string text)
	{
		int num = 1;
		_activatedTemplate = TemplatesSingleAgreement;
		if (_activatedTemplate == null)
		{
			VespaDebug.LogError("Failed to find agreement template of count=" + num);
			return;
		}
		UIHelper.EnableGameObject(_activatedTemplate.Root, enable: true);
		_activatedTemplate.SetLabel(text);
		_activatedTemplate.InitCheckbox();
		_activatedTemplate.SetRepositionNow();
		UIHelper.EnableButton(ConfirmButton, enable: false);
	}

	private void InitComboBoxCategory()
	{
		_selectedCategoryIndex = -1;
		ComboBoxCategory.Clear();
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_csMailDataList);
		IList<CSMailData> allData = GameRoot.Instance.CSMailDataContainer.GetAllData();
		_csMailDataList.AddRange(allData);
		_csMailDataList.Sort((CSMailData x, CSMailData y) => x.Index.CompareTo(y.Index));
		foreach (CSMailData csMailData in _csMailDataList)
		{
			ComboBoxCategory.AddItem(csMailData.Index - 1, LocalizationManager.GetStringDirect(csMailData.CSMailTitle));
		}
		ComboBoxCategory.OnChanged = OnChangeSelectedCategory;
		ComboBoxCategory.Reposition();
	}

	private void OnChangeSelectedCategory(int value)
	{
		_selectedCategoryIndex = ComboBoxCategory.CurrentKey + 1;
		UpdateConfirmButton();
	}

	private void UpdateConfirmButton()
	{
		if (_activatedTemplate == null)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_activatedTemplate.Agreement); i++)
		{
			GameObject checkBox = _activatedTemplate.Agreement[i].CheckBox;
			if ((bool)checkBox && !UIHelper.IsEnabledGameObjet(checkBox))
			{
				flag = false;
			}
		}
		bool enable = flag && _selectedCategoryIndex != -1;
		UIHelper.EnableButton(ConfirmButton, enable);
	}

	public void OnClickConfirm()
	{
		UIHelper.EnableButton(ConfirmButton, enable: false);
		Vespa_System_Action_Extension_Method.fire(_onConfirm, _selectedCategoryIndex);
		base.Close();
	}

	public void OnClickClose()
	{
		base.Close();
	}

	public void ToggleCheckboxObject(GameObject senderObject)
	{
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_activatedTemplate.Agreement); i++)
		{
			if (_activatedTemplate.Agreement[i].CheckBox == senderObject)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			VespaDebug.LogError("Invalid checkbox object. senderObject =" + senderObject);
			return;
		}
		UIHelper.ToggleGameObject(senderObject);
		UpdateConfirmButton();
	}
}
