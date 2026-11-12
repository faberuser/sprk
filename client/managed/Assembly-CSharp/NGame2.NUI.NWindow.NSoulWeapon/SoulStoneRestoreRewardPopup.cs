using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class SoulStoneRestoreRewardPopup : BaseRewardPopup
{
	public interface IListener
	{
		void GetBonus();
	}

	public new static string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/Popup/SoulStoneRestoreRewardPopup";

	public new static PrefabAuto<SoulStoneRestoreRewardPopup> PrefabAuto = new PrefabAuto<SoulStoneRestoreRewardPopup>(PrefabPath);

	public RestoreBonusInfo RestoreBonusInfo;

	public UIButton GetBonusButton;

	public UILabel BonusLabel;

	private IListener _iListener;

	public static void Open(IListener iListener, RestoreSoulStoneRewardInfo rewardInfo, bool isGetBonus)
	{
		SoulStoneRestoreRewardPopup soulStoneRestoreRewardPopup = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<SoulStoneRestoreRewardPopup>();
		if (soulStoneRestoreRewardPopup == null)
		{
			WindowPrefab<SoulStoneRestoreRewardPopup> windowPrefab = new WindowPrefab<SoulStoneRestoreRewardPopup>(PrefabPath, GameUIRoot.ELayerType.UI);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
			soulStoneRestoreRewardPopup = windowPrefab.instance;
		}
		soulStoneRestoreRewardPopup.Init(iListener, rewardInfo, isGetBonus);
	}

	private void Init(IListener iListener, RestoreSoulStoneRewardInfo rewardInfo, bool isGetBonus)
	{
		_iListener = iListener;
		RestoreBonusInfo.Open();
		if (rewardInfo != null)
		{
			PopupOpenData popupOpen = new PopupOpenData().SetItemResultInfos(rewardInfo.ItemResults).SetEquipResultInfos(rewardInfo.EquipItemResults).SetCurrencyResultInfos3(rewardInfo.CurrencyResults);
			OpenWithData(popupOpen, isGlobal: false);
			SetGetBonusButtonState(isGetBonus);
			SetAddBonusLabel(rewardInfo.AddSoulStoneMileage);
		}
	}

	private void SetGetBonusButtonState(bool enable)
	{
		UIHelper.EnableButton(GetBonusButton, enable);
	}

	private void SetAddBonusLabel(long addBonus)
	{
		UILabel bonusLabel = BonusLabel;
		long num = addBonus;
		int num2 = 0;
		UIHelper.SetLabelText(bonusLabel, (num > num2) ? ("+" + addBonus) : "");
	}

	public void OnClickGetBonus()
	{
		if (_iListener != null)
		{
			_iListener.GetBonus();
		}
	}
}
