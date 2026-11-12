using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class SellSelectSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private SellSelectSubComponent _this;

		private bool _isSelected;

		private long _price;

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

		public void Set(bool isSelected, long price)
		{
			if (_isSelected != isSelected)
			{
				_price = price;
				_isSelected = isSelected;
				if (!_isSelected)
				{
					ReleaseComponent();
				}
			}
		}

		public void SetPrice(long price)
		{
			_price = price;
			RefreshInfo();
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
			if (_this != null)
			{
				_this.SetPrice(_price);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/SellSelectSubComponent";

	public static PrefabAuto<SellSelectSubComponent> PrefabAuto = new PrefabAuto<SellSelectSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UILabel Label_SellPrice;

	public void SetPrice(long price)
	{
		UIHelper.SetLabelText(Label_SellPrice, Vespa_Long_Extension_Method.ToCommaString(price));
	}
}
