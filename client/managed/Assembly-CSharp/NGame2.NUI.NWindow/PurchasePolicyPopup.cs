using System.Collections.Generic;
using NGame2.NUI.NManager;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PurchasePolicyPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/PurchasePolicyPopup";

	public const string NewPrefabPath = "Resources/Prefab2/Window/PayShop/PayShopPurchasePolicyPopup";

	public UILabel MessageLabel;

	public UIScrollView ScrollView;

	private List<GameObject> _labelList = new List<GameObject>();

	public override void Reset()
	{
		base.Reset();
		if (ScrollView != null)
		{
			ScrollView.ResetPosition();
		}
		_labelList.ForEach(delegate(GameObject e)
		{
			VespaObject.DestroyGo(e);
		});
		_labelList.Clear();
	}

	public static void OpenPopup()
	{
		WindowPrefab<PurchasePolicyPopup> windowPrefab = new WindowPrefab<PurchasePolicyPopup>("Resources/Prefab2/Window/PurchasePolicyPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal();
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void NewOpenPopup()
	{
		WindowPrefab<PurchasePolicyPopup> windowPrefab = new WindowPrefab<PurchasePolicyPopup>("Resources/Prefab2/Window/PayShop/PayShopPurchasePolicyPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal();
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal()
	{
		base.Open();
		string text = VespaResource.Load<TextAsset>("DefaultTableData/DefaultPurchasePolicy/KR_PurchasePolicy").text;
		if (!string.IsNullOrEmpty(text))
		{
			UIHelper.EnableGameObject(MessageLabel, enable: false);
			Vector3 localPosition = MessageLabel.transform.localPosition;
			int num = 0;
			string[] array = text.Split(new char[1] { '\n' });
			foreach (string text2 in array)
			{
				GameObject gameObject = Object.Instantiate(MessageLabel.gameObject);
				Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, MessageLabel.transform.parent);
				gameObject.SetActive(value: true);
				gameObject.transform.localPosition = localPosition + Vector3.down * num;
				UILabel component = gameObject.GetComponent<UILabel>();
				UIHelper.SetLabelText(component, text2);
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
