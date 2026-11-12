using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class ItemLockedSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private ItemLockedSubComponent _this;

		private bool _isVisible;

		public override bool IsActive()
		{
			return _isVisible;
		}

		public override void Release()
		{
			_isVisible = false;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(bool isVisible)
		{
			if (_isVisible != isVisible)
			{
				_isVisible = isVisible;
				if (!_isVisible)
				{
					ReleaseComponent();
				}
			}
		}

		public override void ChainUpdate()
		{
			if (_isVisible && Owner.gameObject.activeInHierarchy && _this == null)
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/ItemLockedSubComponent";

	public static PrefabAuto<ItemLockedSubComponent> PrefabAuto = new PrefabAuto<ItemLockedSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;
}
