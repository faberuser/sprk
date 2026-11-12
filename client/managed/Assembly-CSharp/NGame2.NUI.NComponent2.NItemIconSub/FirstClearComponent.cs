using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class FirstClearComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private FirstClearComponent _this;

		private bool _isFirstClear;

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

		public override bool IsActive()
		{
			return _isFirstClear;
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public override void RefreshInfo()
		{
			if (_this != null)
			{
				if (!IsActive())
				{
					Release();
				}
			}
			else if (IsActive())
			{
				CreateComponent();
			}
		}

		public override void Release()
		{
			_isFirstClear = false;
			ReleaseComponent();
		}

		public void Set(bool isFirstClear)
		{
			if (_isFirstClear != isFirstClear)
			{
				_isFirstClear = isFirstClear;
				RefreshInfo();
			}
		}

		private void CreateComponent()
		{
			_this = PrefabAuto.Factory.Create(Owner.RootWidget.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_this.transform);
			RefreshInfo();
		}

		public void ReleaseComponent()
		{
			FactoryBase.SafeReleaseComponent(ref _this);
		}
	}

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/FirstClearComponent";

	public static PrefabAuto<FirstClearComponent> PrefabAuto = new PrefabAuto<FirstClearComponent>(PrefabPath);

	public UIWidget WidgetRoot;
}
