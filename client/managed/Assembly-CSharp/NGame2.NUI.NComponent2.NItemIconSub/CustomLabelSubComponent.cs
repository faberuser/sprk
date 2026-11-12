using NNguiExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class CustomLabelSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private CustomLabelSubComponent _this;

		public string LabelText { get; private set; }

		public Color LabelColor { get; private set; }

		public bool IsSoulWeapon { get; private set; }

		public override bool IsActive()
		{
			return !String_Extension_Method.IsNullOrEmpty(LabelText);
		}

		public override void Release()
		{
			LabelText = string.Empty;
			LabelColor = Color.white;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(string text, Color color, bool isSoulWeapon)
		{
			if (LabelText == text && LabelColor == color)
			{
				return;
			}
			LabelText = text;
			LabelColor = color;
			IsSoulWeapon = isSoulWeapon;
			if (_this != null)
			{
				if (String_Extension_Method.IsNullOrEmpty(LabelText))
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
			if (!String_Extension_Method.IsNullOrEmpty(LabelText) && Owner.gameObject.activeInHierarchy && _this == null)
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
				_this.SetCustomLabel(LabelText, LabelColor, IsSoulWeapon);
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

	public const string PrefabPath = "Prefab2/Component/ItemIconSub/CustomLabelSubComponent";

	public const string PrefabPathHeroPortSmall = "Prefab2/Component/HeroPortaitSub/CustomLabelOnHeroPortraitSmallSubComonentn";

	public static PrefabAuto<CustomLabelSubComponent> PrefabAuto = new PrefabAuto<CustomLabelSubComponent>("Prefab2/Component/ItemIconSub/CustomLabelSubComponent");

	public static PrefabAuto<CustomLabelSubComponent> PrefabAutoHeroSmall = new PrefabAuto<CustomLabelSubComponent>("Prefab2/Component/ItemIconSub/CustomLabelSubComponent");

	public UIWidget WidgetRoot;

	public UILabel Label_Custom;

	public UISpriteSelector SelectSpriteSelector;

	public void SetCustomLabel(string text, Color color, bool isSoulWeapon)
	{
		UIHelper.SetLabelColor(Label_Custom, color);
		UIHelper.SetLabelText(Label_Custom, text);
		SetSelectSpriteSelector(isSoulWeapon);
	}

	private void SetSelectSpriteSelector(bool isSoulWeapon)
	{
		if (SelectSpriteSelector != null)
		{
			SelectSpriteSelector.Select(isSoulWeapon ? "SoulWeapon" : "Normal");
		}
	}
}
