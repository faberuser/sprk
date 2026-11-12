using NNguiExtension;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class TechnoMagicClassSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private TechnoMagicClassSubComponent _this;

		private CreatureTagType _classType;

		public void Set(CreatureTagType classType)
		{
			_classType = classType;
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
			return _classType != CreatureTagType.None;
		}

		public override void Release()
		{
			_classType = CreatureTagType.None;
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
				_this.Set(_classType);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/TechnoMagicClassSubComponent";

	public static PrefabAuto<TechnoMagicClassSubComponent> PrefabAuto = new PrefabAuto<TechnoMagicClassSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UISpriteSelector UISpriteSelector;

	public void Set(CreatureTagType classType)
	{
		SetClassType(classType);
	}

	public void SetClassType(CreatureTagType classType)
	{
		if (UISpriteSelector != null)
		{
			UISpriteSelector.SelectEnum(classType);
		}
	}
}
