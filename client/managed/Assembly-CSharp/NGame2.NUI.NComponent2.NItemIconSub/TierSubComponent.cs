using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class TierSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private TierSubComponent _this;

		private int _tier;

		private bool _valance;

		public override bool IsActive()
		{
			return _tier > 0;
		}

		public override void Release()
		{
			_tier = 0;
			_valance = false;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(int tier, bool isValance = false)
		{
			if (_tier == tier)
			{
				return;
			}
			_tier = tier;
			_valance = isValance;
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
				_this.SetTier(_tier, _valance);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/TierSubComponent";

	public static PrefabAuto<TierSubComponent> PrefabAuto = new PrefabAuto<TierSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UISprite Sprite_Tire;

	public UISprite Sprite_TechnoMagic_Tire;

	public UILabel Label_Tier;

	public UILabel Label_TechnoMagic_Tier;

	public void SetTier(int tier, bool valance = false)
	{
		UIHelper.SetLabelText(Label_Tier, $"T{tier}");
		UIHelper.EnableGameObject(Sprite_Tire, !valance);
		UIHelper.EnableGameObject(Sprite_TechnoMagic_Tire, valance);
		UIHelper.EnableGameObject(Label_Tier, !valance);
		UIHelper.EnableGameObject(Label_TechnoMagic_Tier, valance);
	}
}
