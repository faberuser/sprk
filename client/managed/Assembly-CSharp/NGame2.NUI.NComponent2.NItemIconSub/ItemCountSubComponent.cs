using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class ItemCountSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private ItemCountSubComponent _this;

		private string _countText;

		private Color _color = Color.white;

		public override bool IsActive()
		{
			return !String_Extension_Method.IsNullOrEmpty(_countText);
		}

		public override void Release()
		{
			_countText = string.Empty;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(string countText, Color color)
		{
			if (_countText == countText && _color == color)
			{
				return;
			}
			_countText = countText;
			_color = color;
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
				_this.SetCountText(_countText, _color);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/ItemCountSubComponent";

	public static PrefabAuto<ItemCountSubComponent> PrefabAuto = new PrefabAuto<ItemCountSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UILabel Label_Count;

	public void SetCountText(string countText, Color color)
	{
		UIHelper.SetLabelText(Label_Count, countText);
		UIHelper.SetLabelColor(Label_Count, color);
	}
}
