using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class BestTeamComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private BestTeamComponent _this;

		private bool _isBestTeam;

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

		public override bool IsActive()
		{
			return _isBestTeam;
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public override void RefreshInfo()
		{
			if (_this != null)
			{
				if (!IsActive())
				{
					Release();
				}
			}
			else if (IsActive())
			{
				CreateComponent();
			}
		}

		public override void Release()
		{
			_isBestTeam = false;
			ReleaseComponent();
		}

		public void Set(bool isBestTeam)
		{
			if (_isBestTeam != isBestTeam)
			{
				_isBestTeam = isBestTeam;
				RefreshInfo();
			}
		}

		private void CreateComponent()
		{
			_this = PrefabAuto.Factory.Create(Owner.RootWidget.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_this.transform);
			RefreshInfo();
		}

		public void ReleaseComponent()
		{
			FactoryBase.SafeReleaseComponent(ref _this);
		}
	}

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/BestTeamComponent";

	public static PrefabAuto<BestTeamComponent> PrefabAuto = new PrefabAuto<BestTeamComponent>(PrefabPath);

	public UIWidget WidgetRoot;
}
