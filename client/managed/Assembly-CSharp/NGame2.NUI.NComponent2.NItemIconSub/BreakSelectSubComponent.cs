using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NHelper;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class BreakSelectSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private BreakSelectSubComponent _this;

		private bool _isSelected;

		public override bool IsActive()
		{
			return _isSelected;
		}

		public override void Release()
		{
			_isSelected = false;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(bool isSelected)
		{
			if (_isSelected != isSelected)
			{
				_isSelected = isSelected;
				if (!_isSelected)
				{
					ReleaseComponent();
				}
			}
		}

		public override void ChainUpdate()
		{
			if (_isSelected && Owner.gameObject.activeInHierarchy && _this == null)
			{
				CreateComponent();
			}
			if (_this != null)
			{
				ProxyBase.RearrangeDepth(Owner, _this.WidgetRoot);
			}
		}

		public override void RefreshInfo()
		{
			if (!(_this == null))
			{
				if (!GetBreakInfo(out var oItemCode, out var oMinCount, out var oMaxCount))
				{
					_this.SetItemAndCount(string.Empty, 0, 0);
				}
				else
				{
					_this.SetItemAndCount(oItemCode, oMinCount, oMaxCount);
				}
			}
		}

		private bool GetBreakInfo(out string oItemCode, out int oMinCount, out int oMaxCount)
		{
			oItemCode = string.Empty;
			oMinCount = 0;
			oMaxCount = 0;
			ItemData itemData = Owner.ItemData;
			if (itemData == null)
			{
				return false;
			}
			if (itemData.Type == ItemType.Equip)
			{
				EquipItemData equipItemData = Owner.EquipItemData;
				if (equipItemData == null)
				{
					return false;
				}
				EquipItemInfo equipItemInfo = Owner.EquipItemInfo;
				if (equipItemInfo == null)
				{
					return false;
				}
				EquipItemBreakData data = GameRoot.Instance.EquipItemBreakDataContainer.GetData(equipItemData.Tier, equipItemData.DetailIndex, itemData.Code, equipItemInfo.Star, equipItemData.SetIndex);
				if (data == null)
				{
					return false;
				}
				oItemCode = data.ItemCode1;
				oMinCount = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetBreakCountEquipBreakEventBonus(oItemCode, data.MinItemCount1);
				oMaxCount = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetBreakCountEquipBreakEventBonus(oItemCode, data.MaxItemCount1);
				if (oMinCount == 0 && oMaxCount == 0)
				{
					return false;
				}
				return true;
			}
			if (itemData.Type == ItemType.Rune)
			{
				RuneItemBreakData data2 = GameRoot.Instance.RuneItemBreakDataContainer.GetData(itemData.Grade);
				if (data2 == null)
				{
					return false;
				}
				int num = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(itemData.Index)?.Count ?? 1;
				oItemCode = data2.ItemCode1;
				oMinCount = data2.MinItemCount1 * num;
				oMaxCount = data2.MaxItemCount1 * num;
				if (oMinCount == 0 && oMaxCount == 0)
				{
					return false;
				}
				return true;
			}
			if (itemData.Type == ItemType.PunishmentRune)
			{
				PunishmentRuneItemBreakData data3 = GameRoot.Instance.PunishmentRuneItemBreakDataContainer.GetData(RuneHelper.GetPunishmentRuneItemBreakDefaultIndex());
				if (data3 == null)
				{
					return false;
				}
				oItemCode = data3.ItemCode1;
				oMinCount = data3.MinItemCount1;
				oMaxCount = data3.MaxItemCount1;
				if (oMinCount == 0 && oMaxCount == 0)
				{
					return false;
				}
				return true;
			}
			if (itemData.Type == ItemType.HousingObject)
			{
				EquipItemBreakData data4 = GameRoot.Instance.EquipItemBreakDataContainer.GetData(0, 0, itemData.Code);
				if (data4 == null)
				{
					return false;
				}
				int num2 = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(itemData.Index)?.Count ?? 1;
				oItemCode = data4.ItemCode1;
				oMinCount = data4.MinItemCount1 * num2;
				oMaxCount = data4.MaxItemCount1 * num2;
				if (oMinCount == 0 && oMaxCount == 0)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		private void CreateComponent()
		{
			_this = PrefabAuto.Factory.Create(Owner.RootWidget.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_this.transform);
			RefreshInfo();
		}

		private void ReleaseComponent()
		{
			FactoryBase.SafeReleaseComponent(ref _this);
		}
	}

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/BreakSelectSubComponent";

	public static PrefabAuto<BreakSelectSubComponent> PrefabAuto = new PrefabAuto<BreakSelectSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UIBundleLoader BundleTex_BreakItem;

	public UILabel Label_BreakCount;

	public void SetItemAndCount(string itemCode, int minCount, int maxCount)
	{
		string text = ((minCount != maxCount) ? (minCount + " ~ " + maxCount) : minCount.ToString());
		string itemIconPath = UIHelper.GetItemIconPath(itemCode);
		UIHelper.SetBundleTexture(BundleTex_BreakItem, itemIconPath);
		UIHelper.SetLabelText(Label_BreakCount, text);
	}
}
