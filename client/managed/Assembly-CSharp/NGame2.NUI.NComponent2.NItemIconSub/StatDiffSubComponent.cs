using NNguiExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class StatDiffSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private StatDiffSubComponent _this;

		public bool IsVisible { get; private set; }

		public CompareEquipItem CompareType { get; private set; }

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

		public void Set(bool isVisible, CompareEquipItem compareType)
		{
			if (IsVisible == isVisible && CompareType == compareType)
			{
				return;
			}
			IsVisible = isVisible;
			CompareType = compareType;
			if (_this != null)
			{
				if (!IsVisible)
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
			if (_this != null)
			{
				_this.SetStatDiff(CompareType);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/StatDiffSubComponent";

	public static PrefabAuto<StatDiffSubComponent> PrefabAuto = new PrefabAuto<StatDiffSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UISpriteSelector CompareSprite;

	public void SetStatDiff(CompareEquipItem compare)
	{
		CompareSprite.SelectEnum(compare);
	}
}
