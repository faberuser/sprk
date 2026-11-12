using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class TechnoMagicIdentifiedSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private TechnoMagicIdentifiedSubComponent _this;

		private bool _identified;

		public void Set(bool identified)
		{
			_identified = identified;
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

		public override bool IsActive()
		{
			return !_identified;
		}

		public override void Release()
		{
			_identified = false;
			ReleaseComponent();
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

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public override void RefreshInfo()
		{
			if (_this != null)
			{
				_this.Set(_identified);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/TechnoMagicIdentifiedSubComponent";

	public static PrefabAuto<TechnoMagicIdentifiedSubComponent> PrefabAuto = new PrefabAuto<TechnoMagicIdentifiedSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UISprite Identified_Sprite;

	public void Set(bool identified)
	{
		if (Identified_Sprite != null)
		{
			UIHelper.EnableGameObject(Identified_Sprite, !identified);
		}
	}
}
