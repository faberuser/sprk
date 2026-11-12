using System.Collections.Generic;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LawRelatedPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/LawRelatedPopup";

	public const string NewPrefabPath = "Resources/Prefab2/Window/PayShop/PayShopLawRelatedPopup";

	public UILabel TitleLabel;

	public UILabel MessageLabel;

	public UIScrollView ScrollView;

	private CountryData _countryData;

	private CountryServerData _countryServerData;

	private List<PlainAssetText> _agreementAssetTextList = new List<PlainAssetText>();

	private List<GameObject> _labelList = new List<GameObject>();

	public override void Reset()
	{
		base.Reset();
		if (ScrollView != null)
		{
			ScrollView.ResetPosition();
		}
		_agreementAssetTextList.ForEach(delegate(PlainAssetText e)
		{
			e.Close();
		});
		_agreementAssetTextList.Clear();
		_labelList.ForEach(delegate(GameObject e)
		{
			VespaObject.DestroyGo(e);
		});
		_labelList.Clear();
	}

	public static void Open(CountryData countryData, string title, int messageIndex)
	{
		WindowPrefab<LawRelatedPopup> windowPrefab = new WindowPrefab<LawRelatedPopup>("Resources/Prefab2/Window/LawRelatedPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(countryData, title, messageIndex);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void NewOpen(CountryData countryData, string title, int messageIndex)
	{
		WindowPrefab<LawRelatedPopup> windowPrefab = new WindowPrefab<LawRelatedPopup>("Resources/Prefab2/Window/PayShop/PayShopLawRelatedPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(countryData, title, messageIndex);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(CountryData countryData, string title, int messageIndex)
	{
		base.Open();
		_countryData = countryData;
		Init(title, messageIndex, countryData.Agreement);
	}

	public static void Open(CountryServerData countryServerData, string title, int messageIndex)
	{
		WindowPrefab<LawRelatedPopup> windowPrefab = new WindowPrefab<LawRelatedPopup>("Resources/Prefab2/Window/LawRelatedPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(countryServerData, title, messageIndex);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void NewOpen(CountryServerData countryServerData, string title, int messageIndex)
	{
		WindowPrefab<LawRelatedPopup> windowPrefab = new WindowPrefab<LawRelatedPopup>("Resources/Prefab2/Window/PayShop/PayShopLawRelatedPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(countryServerData, title, messageIndex);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(CountryServerData countryServerData, string title, int messageIndex)
	{
		base.Open();
		_countryServerData = countryServerData;
		Init(title, messageIndex, countryServerData.Law);
	}

	private void Init(string title, int messageIndex, string[] agreement, bool fromResources = true)
	{
		UIHelper.SetLabelText(TitleLabel, title);
		if (__Extension_Method_ICollection__.COUNT(agreement) == 0 || !Vespa_Generic_IList_Extension_Method.IsValidIndex(agreement, messageIndex))
		{
			VespaDebug.LogError("Agreement error : agreement count is 0.");
			return;
		}
		string agreementPath = GetAgreementPath(agreement, messageIndex, fromResources);
		if (fromResources)
		{
			TextAsset textAsset = VespaResource.Load<TextAsset>(agreementPath);
			SetupContents(textAsset.text);
			return;
		}
		PlainAssetText plainAssetText = new PlainAssetText();
		plainAssetText.Open(agreementPath, PlainAssetText.EExtensionType.txt, reload: true);
		plainAssetText.SetCallback(this, delegate(string text)
		{
			SetupContents(text);
		});
		_agreementAssetTextList.Add(plainAssetText);
	}

	private string GetAgreementPath(string[] agreement, int agreementIndex, bool fromResources)
	{
		string empty = string.Empty;
		if (agreementIndex >= __Extension_Method_ICollection__.COUNT(agreement))
		{
			return empty;
		}
		if (fromResources)
		{
			return "DefaultTableData/DefaultAgreements/" + agreement[agreementIndex];
		}
		return "Agreement/" + agreement[agreementIndex];
	}

	private void SetupContents(string message)
	{
		if (!string.IsNullOrEmpty(message))
		{
			UIHelper.EnableGameObject(MessageLabel, enable: false);
			Vector3 localPosition = MessageLabel.transform.localPosition;
			int num = 0;
			string[] array = message.Split(new char[1] { '\n' });
			foreach (string text in array)
			{
				GameObject gameObject = Object.Instantiate(MessageLabel.gameObject);
				Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, MessageLabel.transform.parent);
				gameObject.SetActive(value: true);
				gameObject.transform.localPosition = localPosition + Vector3.down * num;
				UILabel component = gameObject.GetComponent<UILabel>();
				UIHelper.SetLabelText(component, text);
				num += (int)component.printedSize.y;
				_labelList.Add(gameObject);
			}
		}
	}

	public void OnClickClose()
	{
		Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}
}
