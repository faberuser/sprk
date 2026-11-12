using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class NewMarkSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private NewMarkSubComponent _this;

		private bool _isShow;

		public override bool IsActive()
		{
			return _isShow;
		}

		public override void Release()
		{
			_isShow = false;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(bool isShow)
		{
			if (_isShow != isShow)
			{
				_isShow = isShow;
				if (!_isShow)
				{
					ReleaseComponent();
				}
			}
		}

		public override void ChainUpdate()
		{
			if (_isShow && Owner.gameObject.activeInHierarchy && _this == null)
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/NewMarkSubComponent";

	public static PrefabAuto<NewMarkSubComponent> PrefabAuto = new PrefabAuto<NewMarkSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;
}
