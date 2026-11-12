using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NEclipse;

public class EclipseClearWaveRewardPopup : BaseWindow
{
	public const string PrefabPath = "Prefab2/Window/Eclipse/Popup/EclipseClearWaveRewardPopup";

	public UISimpleGrid RewardGrid;

	public UILabel TitleLabel;

	public UILabel DescLabel;

	public Transform NoAdvanceWaveLabelTransform;

	private List<Component> _rewardComponentList = new List<Component>();

	public static void Open(int deckIndex, int waveIndex)
	{
		EclipseClearWaveRewardPopup eclipseClearWaveRewardPopup = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<EclipseClearWaveRewardPopup>();
		if (eclipseClearWaveRewardPopup != null)
		{
			eclipseClearWaveRewardPopup.CompleteAnimationWithoutCallback();
			eclipseClearWaveRewardPopup.OpenInternal(deckIndex, waveIndex);
			eclipseClearWaveRewardPopup.PlayWindowOpenAnimation();
		}
		else
		{
			WindowPrefab<EclipseClearWaveRewardPopup> windowPrefab = new WindowPrefab<EclipseClearWaveRewardPopup>("Prefab2/Window/Eclipse/Popup/EclipseClearWaveRewardPopup", GetCurrentLayerType());
			windowPrefab.instance.OpenInternal(deckIndex, waveIndex);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	private static GameUIRoot.ELayerType GetCurrentLayerType()
	{
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
		{
			return GameUIRoot.ELayerType.UI3DUI;
		}
		return GameUIRoot.ELayerType.UI;
	}

	private void OpenInternal(int deckIndex, int waveIndex)
	{
		base.Open();
		Init(deckIndex, waveIndex);
	}

	private void Init(int deckIndex, int waveIndex)
	{
		SetTitle(LocalizationManager.GetStringDirect("ECLIPSE_TEAM_REWARD_POPUP_TITLE", waveIndex));
		SetDescLabel(deckIndex, waveIndex);
		SetRewardList(waveIndex);
	}

	private void SetTitle(string title)
	{
		UIHelper.SetLabelText(TitleLabel, title);
	}

	private void SetDescLabel(int deckIndex, int waveIndex)
	{
		UIHelper.SetLabelText(DescLabel, string.Empty);
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			if (!GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.CheckAdvanceWave(waveIndex))
			{
				UIHelper.SetLabelText(DescLabel, LocalizationManager.GetStringDirect("ECLIPSE_TEAM_REWARD_POPUP_DESC_FAIL", deckIndex));
			}
			else
			{
				UIHelper.SetLabelText(DescLabel, LocalizationManager.GetStringDirect("ECLIPSE_TEAM_REWARD_POPUP_DESC", deckIndex, waveIndex));
			}
		}
	}

	private void SetRewardList(int waveIndex)
	{
		FactoryBase.ReleaseAndClearList(_rewardComponentList);
		UIHelper.EnableGameObject(NoAdvanceWaveLabelTransform, enable: false);
		if (RewardGrid == null)
		{
			return;
		}
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || !GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.CheckAdvanceWave(waveIndex))
		{
			SetTitle(LocalizationManager.GetStringDirect("ECLIPSE_TEAM_REWARD_POPUP_TITLE_FAIL"));
			UIHelper.EnableGameObject(NoAdvanceWaveLabelTransform, enable: true);
			return;
		}
		List<RewardElementData> waveRangeRewardList = GameRoot.Instance.EclipseRewardDataContainer.GetWaveRangeRewardList(waveIndex);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(waveRangeRewardList))
		{
			AddRewardElementComponent(waveRangeRewardList);
			UIHelper.RepositionGrid(RewardGrid);
		}
	}

	private void AddRewardElementComponent(List<RewardElementData> list)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		RewardElementData rewardElementData = null;
		Component component = null;
		for (int i = 0; i < list.Count; i++)
		{
			rewardElementData = list[i];
			if (rewardElementData != null)
			{
				component = GetComponent(rewardElementData);
				_rewardComponentList.Add(component);
			}
		}
	}

	private Component GetComponent(RewardElementData rewardElementData)
	{
		Component component = null;
		if (rewardElementData.GetCurrencyType() != CurrencyType.None)
		{
			component = UIHelper.AddGridItem(RewardGrid, CurrencyIconComponent.PrefabAuto);
			if (component == null)
			{
				return null;
			}
			CurrencyIconComponent obj = component as CurrencyIconComponent;
			CurrencyType currencyType = rewardElementData.GetCurrencyType();
			int itemMax = rewardElementData.ItemMax;
			CurrencyIconComponent.Listener listener = null;
			int num = itemMax;
			obj.Open(currencyType, num, listener);
		}
		if (!rewardElementData.ItemCode.IsNullOrEmpty())
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
			if (dataByCode == null)
			{
				return null;
			}
			component = UIHelper.AddGridItem(RewardGrid, ItemIconComponent.PrefabAuto);
			if (component == null)
			{
				return null;
			}
			(component as ItemIconComponent).Open(dataByCode);
			if (dataByCode.Type != ItemType.Equip)
			{
				ItemIconComponent obj2 = component as ItemIconComponent;
				int itemMin = rewardElementData.ItemMin;
				int itemMax2 = rewardElementData.ItemMax;
				Color? color = null;
				int num2 = itemMax2;
				int num3 = itemMin;
				obj2.SetItemMinMax(num3, num2, color);
			}
		}
		return component;
	}
}
