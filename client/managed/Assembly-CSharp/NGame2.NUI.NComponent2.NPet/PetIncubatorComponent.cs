using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class PetIncubatorComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface IListener
	{
		void OnClickEquip(int incubatorIndex);

		void OnClickPurchase(int incubatorIndex);
	}

	public const string PrefabPath = "Prefab2/Component/Pet/PetIncubatorComponent";

	public static PrefabAuto<PetIncubatorComponent> PrefabAuto = new PrefabAuto<PetIncubatorComponent>("Prefab2/Component/Pet/PetIncubatorComponent");

	public UIBundleLoader Incubator_Icon_BundleLoader;

	public UILabel Incubator_Grade_Label;

	public UILabel Incubator_Name_Label;

	public UILabel Incubator_Desc_Label;

	public UILabel Incubator_Func_Label;

	public Transform Equip_Root;

	public Transform Value_Root;

	public UIWidget IconRoot;

	public UILabel Value_Label;

	public Transform Possession_Root;

	public Transform SpriteSelect_Root;

	[Header("Incubator Color")]
	public Color Color_Grade_1;

	public Color Color_Grade_2;

	public Color Color_Grade_3;

	public Color Color_Grade_4;

	public Color Color_Grade_5;

	public Color Color_Grade_6;

	public Color ColorGoldEnough = Color.white;

	public Color ColorGoldNotEnough = Color.red;

	private IListener _listener;

	private IconProxy _iconProxy;

	[HideInInspector]
	public bool IsUsed { get; private set; }

	public bool IsOwned { get; private set; }

	public PetIncubatorData PetIncubatorData { get; private set; }

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		if (_iconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref _iconProxy);
		}
		_iconProxy = null;
	}

	public void Open(IListener listener, int dataIndex, bool isUsed, bool isOwned)
	{
		PetIncubatorData = GameRoot.Instance.PetIncubatorDataContainer.GetData(dataIndex);
		if (PetIncubatorData != null)
		{
			_listener = listener;
			IsUsed = isUsed;
			IsOwned = isOwned;
			Init();
		}
	}

	private void Init()
	{
		string stringDirect = LocalizationManager.GetStringDirect(PetIncubatorData.NameLocalKey);
		UIHelper.SetLabelText(Incubator_Name_Label, stringDirect);
		string stringDirect2 = LocalizationManager.GetStringDirect(PetIncubatorData.DescLocalKey);
		UIHelper.SetLabelText(Incubator_Desc_Label, stringDirect2);
		string stringDirect3 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_PERCENT", PetIncubatorData.TimePer);
		UIHelper.SetLabelText(Incubator_Func_Label, stringDirect3);
		string stringDirect4 = LocalizationManager.GetStringDirect($"ITEM_GRADE_{PetIncubatorData.Grade}");
		Color gradeLabelColor = GetGradeLabelColor(PetIncubatorData.Grade);
		UIHelper.SetLabelText(Incubator_Grade_Label, stringDirect4);
		UIHelper.SetLabelColor(Incubator_Grade_Label, gradeLabelColor);
		UIHelper.EAssetTextureAssign assetTexAssign = UIHelper.EAssetTextureAssign.None;
		string petIncubatorIconPath = UIHelper.GetPetIncubatorIconPath(PetIncubatorData.Index);
		UIHelper.SetBundleTexture(Incubator_Icon_BundleLoader, petIncubatorIconPath, assetTexAssign);
		SetButton();
	}

	private void SetButton()
	{
		UIHelper.EnableGameObject(SpriteSelect_Root, IsUsed);
		UIHelper.EnableGameObject(Possession_Root, IsUsed);
		UIHelper.EnableGameObject(Equip_Root, !IsUsed && IsOwned);
		UIHelper.EnableGameObject(Value_Root, !IsOwned);
		if (_iconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref _iconProxy);
		}
		if (!IsUsed)
		{
			CurrencyType buyCurrencyType = PetIncubatorData.BuyCurrencyType;
			bool flag = true;
			switch (buyCurrencyType)
			{
			case CurrencyType.Gold:
				flag = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold >= PetIncubatorData.BuyCurrencyValue;
				break;
			case CurrencyType.Gem:
			{
				long gem = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gem;
				int buyCurrencyValue = PetIncubatorData.BuyCurrencyValue;
				flag = gem >= buyCurrencyValue;
				break;
			}
			}
			Color color = (flag ? ColorGoldEnough : ColorGoldNotEnough);
			UILabel value_Label = Value_Label;
			int buyCurrencyValue2 = PetIncubatorData.BuyCurrencyValue;
			UIHelper.SetLabelText(value_Label, Vespa_Int_Extension_Method.ToCommaString(buyCurrencyValue2));
			UIHelper.SetLabelColor(Value_Label, color);
			if (_iconProxy == null)
			{
				_iconProxy = IconProxy.Create(IconRoot);
			}
			if (_iconProxy != null)
			{
				_iconProxy.SetupCurrencyNoBg(PetIncubatorData.BuyCurrencyType);
			}
		}
	}

	private Color GetGradeLabelColor(int grade)
	{
		return grade switch
		{
			2 => Color_Grade_2, 
			3 => Color_Grade_3, 
			4 => Color_Grade_4, 
			5 => Color_Grade_5, 
			6 => Color_Grade_6, 
			_ => Color_Grade_1, 
		};
	}

	public void Refresh(bool isUsed, bool isOwned)
	{
		IsUsed = isUsed;
		IsOwned = isOwned;
		SetButton();
	}

	public void OnClickEquipButton()
	{
		if (_listener != null && IsOwned && !IsUsed)
		{
			_listener.OnClickEquip(PetIncubatorData.Index);
		}
	}

	public void OnClickBuyButton()
	{
		if (_listener != null && !IsOwned && !IsUsed)
		{
			_listener.OnClickPurchase(PetIncubatorData.Index);
		}
	}
}
