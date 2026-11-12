using NGame2.NAccount;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public class RewardSubComponent : LinkFactoryComponent
{
	public class ComponentProxy : GenericProxyBase<ComponentProxy>
	{
		private RewardSubComponent _this;

		private bool _displayBooster;

		private string _boosterCode;

		private bool _displayBoosterLabel;

		private bool _displayInvenCount;

		private ItemData _itemData;

		private bool _isLightBackground;

		private bool _displayUnderLabel;

		private string _underTextValue;

		public override bool IsActive()
		{
			if (!_displayBooster && !_displayInvenCount)
			{
				return _displayUnderLabel;
			}
			return true;
		}

		public override void Release()
		{
			_displayBooster = false;
			_displayInvenCount = false;
			_displayUnderLabel = false;
			_underTextValue = string.Empty;
			ReleaseComponent();
		}

		public override void OnDisable()
		{
			ReleaseComponent();
		}

		public void SetBoosterInfo(bool displayBooster, string boosterCode, bool displayBoosterLabel)
		{
			if (String_Extension_Method.IsNullOrEmpty(boosterCode))
			{
				displayBooster = false;
			}
			if (displayBooster == _displayBooster && boosterCode == _boosterCode && displayBoosterLabel == _displayBoosterLabel)
			{
				return;
			}
			_displayBooster = displayBooster;
			_boosterCode = boosterCode;
			_displayBoosterLabel = displayBoosterLabel;
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

		public void SetInvenCount(bool displayInvenCount, ItemData itemData, bool isLightBackground)
		{
			if (itemData == null)
			{
				displayInvenCount = false;
			}
			if (itemData == _itemData && isLightBackground == _isLightBackground && displayInvenCount == _displayInvenCount)
			{
				return;
			}
			_itemData = itemData;
			_isLightBackground = isLightBackground;
			_displayInvenCount = displayInvenCount;
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

		public void SetUnderLabel(bool displayUnderLabel, string text, bool isLightBackground)
		{
			if (string.IsNullOrEmpty(text))
			{
				displayUnderLabel = false;
			}
			if (_displayUnderLabel == displayUnderLabel && _underTextValue == text)
			{
				return;
			}
			_displayUnderLabel = displayUnderLabel;
			_underTextValue = text;
			_isLightBackground = isLightBackground;
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
				_this.SetInvenCount(_displayInvenCount, _itemData, _isLightBackground);
				_this.SetBoosterInfo(_displayBooster, _boosterCode, _displayBoosterLabel);
				_this.SetUnderText(_displayUnderLabel, _underTextValue, _isLightBackground);
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

	public static string PrefabPath = "Prefab2/Component/ItemIconSub/RewardSubComponent";

	public static PrefabAuto<RewardSubComponent> PrefabAuto = new PrefabAuto<RewardSubComponent>(PrefabPath);

	public UIWidget WidgetRoot;

	public UILabel BoosterLabel;

	public UILabel InvenLabel;

	public UILabel UnderLabel;

	public UIBundleLoader BundleTex_Booster;

	public UITexture Texture_Booster;

	public void SetInvenCount(bool isShow, ItemData itemData, bool isLightBackground)
	{
		UIHelper.EnableGameObject(InvenLabel, enable: false);
		if (itemData != null && (itemData.Type == ItemType.UpgradeBook || itemData.Type == ItemType.AwakeChallenge || itemData.Type == ItemType.HeroEnhanceMaterial || itemData.Type == ItemType.NPCFriendlyPoint))
		{
			UIHelper.EnableGameObject(InvenLabel, isShow);
			int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(itemData.Index);
			UIHelper.SetLabelText(InvenLabel, LocalizationManager.GetString(Item.COMMON_GROWTH_REWARD_COUNT, itemCount));
			UIHelper.SetLabelColor(InvenLabel, isLightBackground ? Color.black : Color.white);
			UIHelper.SetLabelEffectColor(InvenLabel, isLightBackground ? Color.white : Color.black);
		}
	}

	public void SetBoosterInfo(bool displayBooster, string boosterCode, bool displayBoosterLabel)
	{
		UIHelper.EnableGameObject(BoosterLabel, displayBooster && displayBoosterLabel);
		UIHelper.EnableGameObject(BundleTex_Booster.UITEX(), displayBooster);
		if (displayBooster && boosterCode != null)
		{
			string boosterIconPath = UIHelper.GetBoosterIconPath(boosterCode);
			UIHelper.SetBundleTexture(BundleTex_Booster, boosterIconPath);
		}
	}

	public void SetUnderText(bool displayUnderLabel, string text, bool isLightBackground)
	{
		UIHelper.EnableGameObject(UnderLabel, displayUnderLabel);
		UIHelper.SetLabelText(UnderLabel, text);
		UIHelper.SetLabelColor(UnderLabel, isLightBackground ? Color.black : Color.white);
		UIHelper.SetLabelEffectColor(UnderLabel, isLightBackground ? Color.white : Color.black);
	}
}
