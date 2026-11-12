using NGame2.NAccount;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class KingsPassItemComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate, ItemIconComponent.Listener
{
	public interface Listener
	{
		void OnClick(KingsPassItemComponent comp);
	}

	public const string PrefabPath = "Prefab2/Component/KingsPass/KingsPassItemComponent";

	public static PrefabAuto<KingsPassItemComponent> PrefabAuto = new PrefabAuto<KingsPassItemComponent>("Prefab2/Component/KingsPass/KingsPassItemComponent");

	public UISimpleGrid ItemGrid;

	public UISprite Sprite_GetMark;

	public UISprite Sprite_Locked;

	public UILabel Label_Price;

	public UIWidget IconRoot;

	public KingsPassMission KingsPassMission;

	public KingsPassSeasonMissionData KingsPassSeasonMissionData;

	private ItemIconComponent _ItemIcon;

	private Listener _listener;

	private IconProxy IconProxy;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref _ItemIcon);
		ReleaseComponent();
	}

	public bool EnabledReceive()
	{
		if (KingsPassMission == null)
		{
			return false;
		}
		if (KingsPassMission.LastStep == 0 && KingsPassMission.Progress == 0)
		{
			return false;
		}
		if (KingsPassMission.LastStep == 1 && KingsPassMission.Progress == 0)
		{
			return false;
		}
		return true;
	}

	public void Open(KingsPassMission kingsPassMission, KingsPassSeasonMissionData kingsPassSeasonMissionData, Listener listener)
	{
		KingsPassSeasonMissionData = kingsPassSeasonMissionData;
		if (KingsPassSeasonMissionData != null)
		{
			KingsPassMission = kingsPassMission;
			_listener = listener;
			RewardData data = GameRoot.Instance.RewardDataContainer.GetData(KingsPassSeasonMissionData.RewardIndex);
			if (data != null)
			{
				Setup(data);
				SetSubUI();
			}
		}
	}

	private void Setup(RewardData rewardData)
	{
		HashString itemCode = rewardData.ItemCode1;
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
		if (dataByCode == null)
		{
			return;
		}
		_ItemIcon = UIHelper.AddGridItem(ItemGrid, ItemIconComponent.PrefabAuto);
		_ItemIcon.Open(dataByCode, this);
		ItemIconComponent itemIcon = _ItemIcon;
		int item1CountMax = rewardData.Item1CountMax;
		Color? color = null;
		int num = item1CountMax;
		itemIcon.SetItemCount(num, color);
		UIHelper.RepositionGrid(ItemGrid);
		string additionalRewardPriceType = KingsPassSeasonMissionData.AdditionalRewardPriceType;
		if (!String_Extension_Method.IsNullOrEmpty(additionalRewardPriceType))
		{
			UIHelper.EnableGameObject(IconRoot, enable: true);
			UIHelper.EnableGameObject(Label_Price, enable: true);
			if (IconProxy == null)
			{
				IconProxy = IconProxy.Create(IconRoot);
			}
			IconProxy.SetupCurrencyNoBg(additionalRewardPriceType);
			UILabel label_Price = Label_Price;
			int additionalRewardPrice = KingsPassSeasonMissionData.AdditionalRewardPrice;
			UIHelper.SetLabelText(label_Price, Vespa_Int_Extension_Method.ToCommaString(additionalRewardPrice));
		}
	}

	private void SetSubUI()
	{
		if (KingsPassMission == null)
		{
			SetGetMark(isGet: false);
			SetLocked(isLocked: true);
			return;
		}
		bool getMark = KingsPassMission.LastStep == 1 && KingsPassMission.Progress == 0;
		SetGetMark(getMark);
		bool locked = KingsPassMission.LastStep == 0 && KingsPassMission.Progress == 0;
		SetLocked(locked);
	}

	public void SetGetMark(bool isGet)
	{
		UIHelper.EnableGameObject(Sprite_GetMark, isGet);
	}

	public void SetLocked(bool isLocked)
	{
		UIHelper.EnableGameObject(Sprite_Locked, isLocked);
	}

	public int GetCurrentItemIndex()
	{
		if (!(_ItemIcon != null))
		{
			return 0;
		}
		return _ItemIcon.ItemIndex;
	}

	public void Refresh()
	{
		KingsPassMission = GlobalAutoBehaviour<AccountManager>.instance.KingsPassManager.Get(new KingsPassMissionKey(KingsPassSeasonMissionData.SeasonIndex, KingsPassSeasonMissionData.MissionIndex));
		if (KingsPassMission != null)
		{
			KingsPassSeasonMissionData = GameRoot.Instance.KingsPassSeasonMissionDataContainer.GetData(KingsPassMission.SeasonIndex, KingsPassMission.MissionIndex);
			if (KingsPassSeasonMissionData != null)
			{
				SetSubUI();
			}
		}
	}

	private void ReleaseComponent()
	{
		if (IconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref IconProxy);
		}
		IconProxy = null;
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}
}
