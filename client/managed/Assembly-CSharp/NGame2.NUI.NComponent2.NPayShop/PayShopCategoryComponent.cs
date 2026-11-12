using System;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopCategoryComponent : ButtonListComponent
{
	public new interface IListener
	{
		void OnCheckNewFlag(string category);
	}

	public class NewFlagCheckClass
	{
		public int NewFlagValue;

		public int NewFlagConfirmValue;

		public NewFlagCheckClass(string value)
		{
			string[] array = value.Split(',');
			if (array.Length != 0)
			{
				int.TryParse(array[0], out NewFlagValue);
			}
			if (array.Length > 1)
			{
				int.TryParse(array[1], out NewFlagConfirmValue);
			}
		}

		public void InitNewFlagConfirmValue(DateTime time)
		{
			NewFlagConfirmValue = int.Parse(time.ToString("yyMMdd"));
		}

		public override string ToString()
		{
			return NewFlagValue + "," + NewFlagConfirmValue;
		}

		public bool IsNew(int value)
		{
			if (value == 0)
			{
				return false;
			}
			if (NewFlagValue <= value && NewFlagConfirmValue < value)
			{
				return true;
			}
			return false;
		}

		public void ConfirmNewFlag()
		{
			NewFlagConfirmValue = NewFlagValue;
		}
	}

	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopCategoryComponent";

	public static PrefabAuto<PayShopCategoryComponent> PrefabAuto = new PrefabAuto<PayShopCategoryComponent>("Prefab2/Component/PayShop2/PayShopCategoryComponent");

	public const string Sub_PrefabPath = "Prefab2/Component/PayShop2/PayShopSubCategoryComponent";

	public static PrefabAuto<PayShopCategoryComponent> Sub_PrefabAuto = new PrefabAuto<PayShopCategoryComponent>("Prefab2/Component/PayShop2/PayShopSubCategoryComponent");

	public UIWidget SelectedWidget;

	public UIWidget UnselectedWidget;

	public UILabel Label_Name;

	public UISpriteSelector CategoryImageSelector;

	public UISpriteSelector FlagImageSelector;

	public TweenPosition Tween_On;

	public TweenPosition Tween_Off;

	public GameObject OnFlagRoot;

	public GameObject OffFlagRoot;

	private IListener _listener;

	public NewPayShopGroupData GroupData { get; private set; }

	public PayShopFlagType Flag { get; private set; }

	public static PrefabAuto<PayShopCategoryComponent> GetPrefabAuto(NewPayShopCategoryType category, bool isParent)
	{
		if (!isParent)
		{
			return Sub_PrefabAuto;
		}
		return PrefabAuto;
	}

	public override void OnRelease()
	{
		base.OnRelease();
		Flag = PayShopFlagType.None;
	}

	public void Open(NewPayShopGroupData categoryGroupData, IListener listener)
	{
		GroupData = categoryGroupData;
		_listener = listener;
		string stringDirect = LocalizationManager.GetStringDirect(GroupData.CategoryName);
		SetName(stringDirect);
		if (String_Extension_Method.IsNullOrEmpty(categoryGroupData.ParentCategory) && CategoryImageSelector != null)
		{
			CategoryImageSelector.Select(categoryGroupData.PayShopCategoryType);
		}
		SetFlag(PayShopFlagType.None);
		SprkShopSearch.DecorateCategory(this, categoryGroupData);
	}

	public static string LocalStringKey(string category)
	{
		return typeof(PayShopCategoryComponent).Name + "_" + category + "_NewFlagStatus";
	}

	public static LocalAccountString CreateLocalString(string category)
	{
		return new LocalAccountString(LocalStringKey(category));
	}

	public void SetName(string name)
	{
		UIHelper.SetLabelText(Label_Name, name);
	}

	public override void SetOpen(bool isOpen, bool isInit)
	{
		base.SetOpen(isOpen);
		ToggleUI(isOpen);
		if (isOpen && !isInit)
		{
			CheckNewFlag();
		}
	}

	public void CheckNewFlag()
	{
		if (!GameRoot.Instance.NewPayShopGroupDataContainer.IsParentAndHasChild(GroupData) && (Flag & PayShopFlagType.New) != PayShopFlagType.None)
		{
			Flag &= (PayShopFlagType)(-17);
			NewFlagCheckClass newFlagCheckClass = LoadNewFlagCheckClass(GroupData.PayShopCategoryType);
			newFlagCheckClass.ConfirmNewFlag();
			SaveNewFlagCheckClass(GroupData.PayShopCategoryType, newFlagCheckClass);
			SetFlag(Flag);
			if (_listener != null)
			{
				_listener.OnCheckNewFlag(GroupData.PayShopCategoryType);
			}
		}
	}

	public static NewFlagCheckClass LoadNewFlagCheckClass(string category)
	{
		LocalAccountString localAccountString = CreateLocalString(category);
		if (String_Extension_Method.IsNullOrEmpty(localAccountString.Value))
		{
			return new NewFlagCheckClass("");
		}
		return new NewFlagCheckClass(localAccountString.Value);
	}

	public static void SaveNewFlagCheckClass(string category, NewFlagCheckClass newFlagCheckClass)
	{
		CreateLocalString(category).Value = newFlagCheckClass.ToString();
	}

	private void ToggleUI(bool isOn)
	{
		UIHelper.EnableGameObject(SelectedWidget, isOn);
		UIHelper.EnableGameObject(UnselectedWidget, !isOn);
		if (isOn && Tween_On != null)
		{
			Tween_On.ResetToBeginning();
			Tween_On.SetOnFinished(delegate
			{
				UIHelper.EnableGameObject(Tween_On, enable: true, isForce: true);
			});
			Tween_On.PlayForward();
		}
		if (!isOn && Tween_Off != null)
		{
			Tween_Off.ResetToBeginning();
			Tween_Off.SetOnFinished(delegate
			{
				UIHelper.EnableGameObject(Tween_Off, enable: true, isForce: true);
			});
			Tween_Off.PlayForward();
		}
		if (OnFlagRoot != null && OffFlagRoot != null)
		{
			GameObject gameObject = (isOn ? OnFlagRoot : OffFlagRoot);
			FlagImageSelector.transform.parent = gameObject.transform;
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(FlagImageSelector.transform);
		}
	}

	public void SetFlag(PayShopFlagType flag)
	{
		if (flag == PayShopFlagType.None)
		{
			UIHelper.EnableGameObject(FlagImageSelector, enable: false);
		}
		else
		{
			UIHelper.EnableGameObject(FlagImageSelector, enable: true);
			if ((flag & PayShopFlagType.New) != PayShopFlagType.None)
			{
				FlagImageSelector.Select(PayShopFlagType.New.ToString());
			}
			else if ((flag & PayShopFlagType.Sale) != PayShopFlagType.None)
			{
				FlagImageSelector.Select(PayShopFlagType.Sale.ToString());
			}
			else if ((flag & PayShopFlagType.Free) != PayShopFlagType.None)
			{
				FlagImageSelector.Select(PayShopFlagType.Free.ToString());
			}
		}
		Flag = flag;
	}

	public static void ClearNewFlagValue()
	{
		foreach (NewPayShopGroupData allDatum in GameRoot.Instance.NewPayShopGroupDataContainer.GetAllData())
		{
			NewFlagCheckClass newFlagCheckClass = LoadNewFlagCheckClass(allDatum.PayShopCategoryType);
			newFlagCheckClass.NewFlagConfirmValue = 0;
			newFlagCheckClass.NewFlagValue = 0;
			SaveNewFlagCheckClass(allDatum.PayShopCategoryType, newFlagCheckClass);
		}
	}
}
