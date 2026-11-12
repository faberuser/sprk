using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class EventBonusSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private EventBonusSubComponent _this;

		public bool IsVisible { get; private set; }

		public override bool IsActive()
		{
			return IsVisible;
		}

		public override void Release()
		{
			IsVisible = false;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(bool isVisible)
		{
			if (IsVisible != isVisible)
			{
				IsVisible = isVisible;
				if (!IsVisible)
				{
					ReleaseComponent();
				}
			}
		}

		public override void ChainUpdate()
		{
			if (IsVisible && Owner.gameObject.activeInHierarchy && _this == null)
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/EventBonusSubComponent";

	public static PrefabAuto<EventBonusSubComponent> PrefabAuto = new PrefabAuto<EventBonusSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;
}
