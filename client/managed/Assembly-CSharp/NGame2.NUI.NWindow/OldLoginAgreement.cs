using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NNotification;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class OldLoginAgreement : BaseWindow
{
	[Serializable]
	public class Agreement
	{
		public UITable TableAgreement;

		public GameObject CheckBox;

		public void SetCheckBox(bool enable)
		{
			UIHelper.EnableGameObject(CheckBox, enable);
		}

		public void SetRepositionNow()
		{
			TableAgreement.repositionNow = true;
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
				Agreement[i]?.SetCheckBox(enable: false);
			}
		}

		public void SetRepositionNow()
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Agreement); i++)
			{
				Agreement[i]?.SetRepositionNow();
			}
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/OldLoginAgreement";

	public string TestCountryCode;

	public UIButton ConfirmButton;

	public Template TemplatesSingleAgreement;

	public Template TemplatesDoubleAgreement;

	public GameObject CheckBox_NightNotiConfirm;

	private bool NightNotiChecked;

	private bool isNightNotiDisagreeShown;

	private Action _onConfirm;

	private Template _activatedTemplate;

	private List<PlainAssetText> _agreementAssetTextList = new List<PlainAssetText>();

	private List<AgreementTextComponent> _agreementTextList = new List<AgreementTextComponent>();

	public void Open(bool accepted, CountryData countryData, Action onConfirm)
	{
		Open(delegate
		{
			Init(accepted, countryData, onConfirm);
		}, null);
	}

	public void Open(bool accepted, string text, Action onConfirm)
	{
		Open(delegate
		{
			Init(accepted, text, onConfirm);
		}, null);
	}

	public void Init(bool accepted, CountryData countryData, Action onConfirm)
	{
		_onConfirm = onConfirm;
		InitAgreementText(countryData, fromResources: false);
		if (accepted || _activatedTemplate == null)
		{
			Vespa_System_Action_Extension_Method.fire(_onConfirm);
		}
		bool enable = true;
		UIHelper.EnableGameObject(CheckBox_NightNotiConfirm, enable);
	}

	public void Init(bool accepted, string text, Action onConfirm)
	{
		_onConfirm = onConfirm;
		InitAgreementText(text);
		if (accepted || _activatedTemplate == null)
		{
			Vespa_System_Action_Extension_Method.fire(_onConfirm);
		}
		bool enable = false;
		UIHelper.EnableGameObject(CheckBox_NightNotiConfirm, enable);
	}

	public void OpenFromResources(bool accepted, CountryData countryData, Action onConfirm)
	{
		Open(delegate
		{
			InitFromResources(accepted, countryData, onConfirm);
		}, null);
	}

	public void InitFromResources(bool accepted, CountryData countryData, Action onConfirm)
	{
		_onConfirm = onConfirm;
		InitAgreementText(countryData, fromResources: true);
		if (accepted || _activatedTemplate == null)
		{
			Vespa_System_Action_Extension_Method.fire(_onConfirm);
		}
		bool enable = true;
		UIHelper.EnableGameObject(CheckBox_NightNotiConfirm, enable);
	}

	public override void Reset()
	{
		base.Reset();
		_onConfirm = null;
		_agreementAssetTextList.ForEach(delegate(PlainAssetText e)
		{
			e.Close();
		});
		_agreementAssetTextList.Clear();
		UIHelper.EnableGameObject(TemplatesDoubleAgreement.Root, enable: false);
		UIHelper.EnableGameObject(TemplatesSingleAgreement.Root, enable: false);
		FactoryBase.ReleaseAndClearList(_agreementTextList);
	}

	private void InitAgreementText(CountryData countryData, bool fromResources)
	{
	}

	private void InitAgreementText(string text)
	{
		int totalAgreementCount = 1;
		_activatedTemplate = GetTemplate(totalAgreementCount);
		if (_activatedTemplate == null)
		{
			VespaDebug.LogError("Failed to find agreement template of count=" + totalAgreementCount);
			return;
		}
		UIHelper.EnableGameObject(_activatedTemplate.Root, enable: true);
		SetAgreementText(totalAgreementCount, 1, text);
		_activatedTemplate.InitCheckbox();
		_activatedTemplate.SetRepositionNow();
		UIHelper.EnableButton(ConfirmButton, enable: false);
	}

	private Template GetTemplate(int totalAgreementCount)
	{
		if (totalAgreementCount == 2)
		{
			return TemplatesDoubleAgreement;
		}
		return TemplatesSingleAgreement;
	}

	private void SetAgreementText(int totalAgreementCount, int agreementNo, string text)
	{
		if (_activatedTemplate == null || agreementNo >= __Extension_Method_ICollection__.COUNT(_activatedTemplate.Agreement))
		{
			return;
		}
		UITable tableAgreement = _activatedTemplate.Agreement[agreementNo].TableAgreement;
		string[] array = text.Split(new char[1] { '\n' });
		foreach (string agreementText in array)
		{
			AgreementTextComponent agreementTextComponent = null;
			agreementTextComponent = ((totalAgreementCount != 2) ? AgreementTextComponent.PrefabSingleAuto.Factory.Create(tableAgreement.transform) : AgreementTextComponent.PrefabAuto.Factory.Create(tableAgreement.transform));
			if (agreementTextComponent != null)
			{
				agreementTextComponent.Open(agreementText);
				_agreementTextList.Add(agreementTextComponent);
			}
		}
		tableAgreement.Reposition();
	}

	private string GetAgreementPath(CountryData countryData, int agreementIndex, bool fromResources)
	{
		return string.Empty;
	}

	private void UpdateAllChecked()
	{
		if (_activatedTemplate == null)
		{
			return;
		}
		bool enable = true;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_activatedTemplate.Agreement); i++)
		{
			GameObject checkBox = _activatedTemplate.Agreement[i].CheckBox;
			if ((bool)checkBox && !UIHelper.IsEnabledGameObjet(checkBox))
			{
				enable = false;
			}
		}
		UIHelper.EnableButton(ConfirmButton, enable);
	}

	public void OnClickConfirm()
	{
		VespaCoroutine.Start(coOnClickConfirm());
	}

	private IEnumerator coOnClickConfirm()
	{
		UIHelper.EnableButton(ConfirmButton, enable: false);
		if (!NightNotiChecked && !isNightNotiDisagreeShown)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_NIGHT_DISAGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
			yield return new WaitForSeconds(1f);
		}
		Vespa_System_Action_Extension_Method.fire(_onConfirm);
		NotificationDefaultSetting();
	}

	public void ToggleCheckboxObject(GameObject senderObject)
	{
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_activatedTemplate.Agreement); i++)
		{
			if (_activatedTemplate.Agreement[i].CheckBox == senderObject)
			{
				if (i == __Extension_Method_ICollection__.COUNT(_activatedTemplate.Agreement) - 1)
				{
					flag = true;
				}
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			VespaDebug.LogError("Invalid checkbox object. senderObject =" + senderObject);
			return;
		}
		UIHelper.ToggleGameObject(senderObject);
		UpdateAllChecked();
		if (flag)
		{
			if (UIHelper.IsEnabledGameObjet(senderObject))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_AGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
			}
			else
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_DISAGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
			}
		}
	}

	public void ToggleNightNotiConfirm(GameObject senderObject)
	{
		UIHelper.ToggleGameObject(senderObject);
		if (UIHelper.IsEnabledGameObjet(senderObject))
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_NIGHT_AGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
			NightNotiChecked = true;
		}
		else
		{
			isNightNotiDisagreeShown = true;
			CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_NIGHT_DISAGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
			NightNotiChecked = false;
		}
	}

	public void NotificationDefaultSetting()
	{
		SingletonAuto<NotificationManager>.instance.SetNotificationDefaultValuesOnce();
		if (NightNotiChecked)
		{
			GameOptionData.Noti_NightNoti = true;
			GameOptionData.Noti_AllNoti = true;
		}
	}
}
