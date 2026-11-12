using System;
using NGame2.NAccount;
using NGame2.NContentsDefine;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI3.NMaxCap;

public class ItemMaxCapComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	[Serializable]
	public class ItemIconUi
	{
		public UIWidget IconRoot;

		private ItemIconComponent _itemIconComponent;

		public void InitUi(int itemIndex)
		{
			ResetUi();
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
			if (data != null)
			{
				_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(IconRoot.transform);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
				_itemIconComponent.Open(data);
			}
		}

		public void ResetUi()
		{
			FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		}
	}

	[Serializable]
	public class MaxCapTextUi
	{
		public UITable Table_Info;

		public UILabel Label_Name;

		public UILabel Label_Count;

		public UILabel Label_MaxCap;

		public void InitUi(int itemIndex)
		{
			if (!AccountManager.IsLogin())
			{
				return;
			}
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
			if (data != null)
			{
				int maxCapCount = ItemMaxCap.GetMaxCapCount(itemIndex);
				int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(itemIndex);
				string text = Vespa_Int_Extension_Method.ToCommaString(itemCount);
				int num = maxCapCount;
				string text2 = Vespa_Int_Extension_Method.ToCommaString(num);
				UIHelper.SetLabelText(Label_Name, data.Name);
				UIHelper.SetLabelText(Label_Count, text);
				UIHelper.SetLabelText(Label_MaxCap, text2);
				if (Table_Info != null)
				{
					Table_Info.Reposition();
				}
			}
		}
	}

	public const string PrefabPath = "Prefab3/MaxCap/ItemMaxCapComponent";

	public static PrefabAuto<ItemMaxCapComponent> PrefabAuto = new PrefabAuto<ItemMaxCapComponent>("Prefab3/MaxCap/ItemMaxCapComponent");

	[Header("---[ 아이콘 ]")]
	public ItemIconUi ItemIcon;

	[Header("---[ 갯수 표기 ]")]
	public MaxCapTextUi MaxCapText;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		ItemIcon.ResetUi();
	}

	public void Open(int itemIndex)
	{
		ItemIcon.InitUi(itemIndex);
		MaxCapText.InitUi(itemIndex);
	}
}
