using NGame2.NAccount;
using NGameAsset;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class EquippedSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private EquippedSubComponent _this;

		private bool _isShow;

		public override bool IsActive()
		{
			return _isShow;
		}

		public override void Release()
		{
			_isShow = false;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void Set(bool isShow)
		{
			if (_isShow == isShow)
			{
				RefreshInfo();
				return;
			}
			_isShow = isShow;
			if (!_isShow)
			{
				ReleaseComponent();
			}
		}

		public override void ChainUpdate()
		{
			if (_isShow && Owner.gameObject.activeInHierarchy && _this == null)
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
			if (!(Owner == null) && Owner.EquipItemInfo != null && !(_this == null))
			{
				_this.Refresh(Owner.EquipItemInfo);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/EquippedSubComponent";

	public static PrefabAuto<EquippedSubComponent> PrefabAuto = new PrefabAuto<EquippedSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UITexture HeroIcon;

	public void Refresh(EquipItemInfo itemInfo)
	{
		SetupEquippedHeroIcon(GetEquippedHeroIndex(itemInfo));
	}

	public int GetEquippedHeroIndex(EquipItemInfo itemInfo)
	{
		if (itemInfo != null)
		{
			CachedHeroInfo heroOfEquipItem = GlobalAutoBehaviour<AccountManager>.instance.GetHeroOfEquipItem(itemInfo.SlotIndex);
			if (heroOfEquipItem == null)
			{
				return 0;
			}
			if (heroOfEquipItem.CreatureData == null)
			{
				return 0;
			}
			return heroOfEquipItem.CreatureData.Index;
		}
		return 0;
	}

	public void SetupEquippedHeroIcon(int heroIndex)
	{
		GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		AssetTexture heroIcon = UIHelper.GetHeroIcon2(heroIndex, 3);
		UIHelper.ResetPlaneAssetTextureAsync(this, HeroIcon, heroIcon);
	}
}
