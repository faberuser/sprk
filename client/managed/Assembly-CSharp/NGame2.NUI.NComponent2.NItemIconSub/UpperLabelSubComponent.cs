using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class UpperLabelSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private UpperLabelSubComponent _this;

		public string LabelText { get; private set; }

		public Color LabelColor { get; private set; }

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

		public void Set(string text, Color color)
		{
			if (LabelText == text && LabelColor == color)
			{
				return;
			}
			LabelText = text;
			LabelColor = color;
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
				_this.SetText(LabelText, LabelColor);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/UpperLabelSubComponent";

	public static PrefabAuto<UpperLabelSubComponent> PrefabAuto = new PrefabAuto<UpperLabelSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UILabel Label_Text;

	public void SetText(string text, Color color)
	{
		UIHelper.SetLabelColor(Label_Text, color);
		UIHelper.SetLabelText(Label_Text, text);
	}
}
