using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class ItemSlotIndexSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private ItemSlotIndexSubComponent _this;

		private int _slotIndex;

		private EquipItemSetGroupType _type;

		public override bool IsActive()
		{
			return _slotIndex != 0;
		}

		public override void Release()
		{
			_slotIndex = 0;
			_type = EquipItemSetGroupType.None;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(int slotIndex, EquipItemSetGroupType type = EquipItemSetGroupType.None)
		{
			if (_slotIndex == slotIndex)
			{
				return;
			}
			_slotIndex = slotIndex;
			_type = type;
			if (_this != null)
			{
				if (!IsActive())
				{
					ReleaseComponent();
				}
				else
				{
					RefreshInfo();
				}
			}
		}

		public override void ChainUpdate()
		{
			if (IsActive() && Owner.gameObject.activeInHierarchy && _this == null)
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
			if (_this != null)
			{
				_this.SetSlotIndex(_slotIndex, _type);
			}
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/ItemSlotIndexSubComponent";

	public static PrefabAuto<ItemSlotIndexSubComponent> PrefabAuto = new PrefabAuto<ItemSlotIndexSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UILabel Label_Count;

	public UILabel Label_Type;

	public void SetSlotIndex(int slotIndex, EquipItemSetGroupType type = EquipItemSetGroupType.None)
	{
		UIHelper.SetLabelText(Label_Count, slotIndex);
		UIHelper.EnableGameObject(Label_Type, type != EquipItemSetGroupType.None);
		UIHelper.SetLabelText(Label_Type, type.ToString());
	}
}
