using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class ItemRatioSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private ItemRatioSubComponent _this;

		private long _ratioPerMille = -1L;

		public override bool IsActive()
		{
			long ratioPerMille = _ratioPerMille;
			int num = 0;
			return ratioPerMille >= num;
		}

		public override void Release()
		{
			int num = -1;
			_ratioPerMille = num;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(long ratioPerMille)
		{
			if (_ratioPerMille == ratioPerMille)
			{
				return;
			}
			_ratioPerMille = ratioPerMille;
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
				_this.SetRatio(_ratioPerMille);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/ItemRatioSubComponent";

	public static PrefabAuto<ItemRatioSubComponent> PrefabAuto = new PrefabAuto<ItemRatioSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UILabel Label_Ratio;

	public void SetRatio(long ratioPermille)
	{
		int num = 10;
		long num2 = ratioPermille / num;
		int num3 = 10;
		long num4 = ratioPermille % num3;
		UIHelper.SetLabelText(text: (num4 != 0L) ? $"{num2}.{num4}%" : $"{num2}%", label: Label_Ratio);
	}
}
