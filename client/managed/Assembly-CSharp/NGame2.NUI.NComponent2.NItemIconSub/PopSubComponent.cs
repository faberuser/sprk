using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class PopSubComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClickPop(ItemIconComponent itemIconComponent);
	}

	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private PopSubComponent _this;

		private Listener _listener;

		public string LabelText { get; private set; }

		public override bool IsActive()
		{
			return !String_Extension_Method.IsNullOrEmpty(LabelText);
		}

		public override void Release()
		{
			LabelText = string.Empty;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(string text, Listener listener = null)
		{
			if (LabelText == text)
			{
				return;
			}
			LabelText = text;
			if (_listener == null)
			{
				_listener = listener;
			}
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
				_this.SetText(LabelText);
			}
		}

		private void CreateComponent()
		{
			_this = PrefabAuto.Factory.Create(Owner.RootWidget.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_this.transform);
			_this._owner = Owner;
			_this._listener = _listener;
			RefreshInfo();
		}

		private void ReleaseComponent()
		{
			if (_this != null)
			{
				_this._owner = null;
				_this._listener = null;
				FactoryBase.SafeReleaseComponent(ref _this);
			}
		}
	}

	private Listener _listener;

	private ItemIconComponent _owner;

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/PopSubComponent";

	public static PrefabAuto<PopSubComponent> PrefabAuto = new PrefabAuto<PopSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UILabel Label_Text;

	public void SetText(string text)
	{
		UIHelper.SetLabelText(Label_Text, text);
	}

	public void OnClickPop()
	{
		if (_listener != null)
		{
			_listener.OnClickPop(_owner);
		}
	}
}
