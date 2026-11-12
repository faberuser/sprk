using NNguiExtension;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class PartsTypeIconSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private PartsTypeIconSubComponent _this;

		private EquipPartType _partType;

		public override bool IsActive()
		{
			return _partType != EquipPartType.None;
		}

		public override void Release()
		{
			_partType = EquipPartType.None;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(EquipPartType partType)
		{
			if (_partType == partType)
			{
				return;
			}
			_partType = partType;
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
				_this.SetPartType(_partType);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/PartsTypeIconSubComponent";

	public static PrefabAuto<PartsTypeIconSubComponent> PrefabAuto = new PrefabAuto<PartsTypeIconSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UISpriteSelector PartTypeSelector;

	public void SetPartType(EquipPartType partType)
	{
		PartTypeSelector.SelectEnum(partType);
	}
}
