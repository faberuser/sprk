using NNguiExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class ItemBaseSubComponent : LinkFactoryComponent, InitOnCreate
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private ItemBaseSubComponent _this;

		private string _itemCode;

		private byte _grade;

		public override bool IsActive()
		{
			return !String_Extension_Method.IsNullOrEmpty(_itemCode);
		}

		public override void Release()
		{
			_itemCode = string.Empty;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(string itemCode, byte grade)
		{
			if (_itemCode == itemCode && _grade == grade)
			{
				return;
			}
			_itemCode = itemCode;
			_grade = grade;
			if (_this != null)
			{
				if (IsActive())
				{
					RefreshInfo();
				}
				else
				{
					ReleaseComponent();
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
				_this.SetItemInfo(_itemCode, _grade);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/ItemBaseSubComponent";

	public static PrefabAuto<ItemBaseSubComponent> PrefabAuto = new PrefabAuto<ItemBaseSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UIBundleLoader BundleTex_ItemIcon;

	public UISpriteSelector SpriteSelect_Grade;

	public UISprite Sprite_Disable;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		if (BundleTex_ItemIcon != null)
		{
			BundleTex_ItemIcon.AssetPath = string.Empty;
		}
	}

	public void SetItemInfo(string itemCode, byte grade)
	{
		SetIcon(UIHelper.GetItemIconPath(itemCode));
		SetGrade(grade);
	}

	public void SetInfo(string path, byte grade)
	{
		SetIcon(path);
		SetGrade(grade);
	}

	private void SetIcon(string path)
	{
		if (!(BundleTex_ItemIcon == null))
		{
			UIHelper.SetBundleTexture(BundleTex_ItemIcon, path);
		}
	}

	private void SetGrade(byte grade)
	{
		if (!(SpriteSelect_Grade == null))
		{
			SpriteSelect_Grade.SelectInt(grade);
		}
	}

	public void SetDisable(bool enable)
	{
		UIHelper.EnableGameObject(Sprite_Disable, enable);
	}
}
