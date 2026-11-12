using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class ItemRunePageSubComponent : LinkFactoryComponent, InitOnCreate
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private ItemRunePageSubComponent _this;

		private int SlotIndex;

		public override bool IsActive()
		{
			return SlotIndex > 0;
		}

		public override void Release()
		{
			SlotIndex = 0;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(int slotIndex)
		{
			if (SlotIndex == slotIndex)
			{
				return;
			}
			SlotIndex = slotIndex;
			if (_this != null)
			{
				if (IsActive())
				{
					RefreshInfo();
				}
				else
				{
					ReleaseComponent();
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
				_this.SetSlotIndex(SlotIndex);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/ItemRunePageSubComponent";

	public static PrefabAuto<ItemRunePageSubComponent> PrefabAuto = new PrefabAuto<ItemRunePageSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UILabel Label_Index;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void SetSlotIndex(int slotIndex)
	{
		UIHelper.SetLabelText(Label_Index, slotIndex);
	}
}
