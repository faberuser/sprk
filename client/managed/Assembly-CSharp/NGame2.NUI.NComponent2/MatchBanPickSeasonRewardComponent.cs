using System.Collections.Generic;
using NGame2.NContentsDefine;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MatchBanPickSeasonRewardComponent : LinkFactoryComponent, InitOnCreate, ItemIconComponent.Listener, CurrencyIconComponent.Listener
{
	public interface Listener
	{
		void OnClickShowMultipleDesc(List<ItemIconComponent> itemIconComp, List<CurrencyIconComponent> currencyIconComp, int selectIndex = 0);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Match/MatchBanPickSeasonRewardComponent";

	public static PrefabAuto<MatchBanPickSeasonRewardComponent> PrefabAuto = new PrefabAuto<MatchBanPickSeasonRewardComponent>("Resources/Prefab2/Component/Match/MatchBanPickSeasonRewardComponent");

	[Header("[Comp Base]")]
	public UILabel Label_Rank;

	public UILabel Label_Score;

	public UISpriteSelector TierIconSprite;

	public UISprite TierNumSprite;

	public UISimpleGrid IconPostion;

	private string[] _seasonBuffItemCode = new string[3] { "BOOSTER_GOLD_BANPICK_REWARD_1ST", "BOOSTER_GOLD_BANPICK_REWARD_2ND", "BOOSTER_GOLD_BANPICK_REWARD_3RD" };

	private List<ItemIconComponent> _itemIconComponentList = new List<ItemIconComponent>();

	private List<CurrencyIconComponent> _currencyIconComponentList = new List<CurrencyIconComponent>();

	private List<CurrencyIconComponent> _currencyIconComponentList2 = new List<CurrencyIconComponent>();

	private Listener _listener;

	private MatchSeasonType _seasonType;

	public MatchBanPickTierRewardData _currData { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
		FactoryBase.ReleaseAndClearList(_currencyIconComponentList);
		FactoryBase.ReleaseAndClearList(_currencyIconComponentList2);
	}

	public void Open(Listener listener, MatchBanPickTierRewardData data, MatchSeasonType seasonType = MatchSeasonType.Free)
	{
		_listener = listener;
		_currData = data;
		_seasonType = seasonType;
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
		FactoryBase.ReleaseAndClearList(_currencyIconComponentList);
		FactoryBase.ReleaseAndClearList(_currencyIconComponentList2);
		Init();
	}

	public void Init()
	{
		int tierIndex = _currData.TierIndex;
		GlobalBanPickTierRewardDataContainer.GlobalBanpickTier globalBanpickTier = GameRoot.Instance.GlobalBanPickTierRewardDataContainer.ConvertTierIndexToTierEnum(tierIndex);
		TierIconSprite.Select(globalBanpickTier.ToString());
		string text = "";
		switch (globalBanpickTier)
		{
		case GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Bronze:
			text = "Arena Bronze Number";
			break;
		case GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Silver:
			text = "Arena Silver Number";
			break;
		case GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Gold:
			text = "Arena Gold Number";
			break;
		case GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Platinum:
			text = "Arena Platinum Number";
			break;
		case GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Diamond:
			text = "Arena Diamond Number";
			break;
		case GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Master:
			text = "Arena Master Number";
			break;
		case GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.RoyalMaster:
			text = "Arena Royal Master Number";
			break;
		case GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Challenge:
			text = "Arena Challenge Number";
			break;
		case GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Champion:
			text = "Arena Champion Number";
			break;
		}
		text = text + "_" + tierIndex % 10;
		UIHelper.EnableGameObject(TierNumSprite, enable: true);
		TierNumSprite.spriteName = text;
		if (globalBanpickTier < GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Challenge)
		{
			MatchBanPickTierData data = GameRoot.Instance.GlobalBanPickTierDataContainer.GetData(tierIndex);
			if (data != null)
			{
				if (data.MinScore == 0)
				{
					UIHelper.SetLabelText(Label_Score, LocalizationManager.GetStringDirect("GLORY_SEASON_LASTPLACE"));
				}
				else
				{
					UIHelper.SetLabelText(Label_Score, data.MinScore);
				}
			}
		}
		else
		{
			IList<MatchBanPickTierData> allData = GameRoot.Instance.GlobalBanPickTierDataContainer.GetAllData();
			int num = 0;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
			{
				MatchBanPickTierData matchBanPickTierData = allData[i];
				if (matchBanPickTierData.MinScore > num)
				{
					num = matchBanPickTierData.MinScore;
				}
			}
			UIHelper.SetLabelText(Label_Score, num);
		}
		VisualTierRank visualTierRank = VisualTierRank.CreateFromTableMinRank(_currData, MatchArenaType.BanPick);
		VisualTierRank visualTierRank2 = VisualTierRank.CreateFromTableMaxRank(_currData, MatchArenaType.BanPick);
		if (visualTierRank.Value > 0 && visualTierRank2.Value > 0)
		{
			UIHelper.EnableGameObject(Label_Rank, enable: true);
			if (visualTierRank.Value == visualTierRank2.Value)
			{
				UIHelper.SetLabelText(Label_Rank, LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_RANK_COUNT1", visualTierRank.Value));
				if (visualTierRank.Value == 1)
				{
					globalBanpickTier = GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Ranker_1st;
				}
				else if (visualTierRank.Value == 2)
				{
					globalBanpickTier = GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Ranker_2rd;
				}
				else if (visualTierRank.Value == 3)
				{
					globalBanpickTier = GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Ranker_3rd;
				}
				TierIconSprite.Select(globalBanpickTier.ToString());
				UIHelper.EnableGameObject(TierNumSprite, enable: false);
			}
			else
			{
				UIHelper.SetLabelText(Label_Rank, LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_RANK_COUNT2", visualTierRank.Value, visualTierRank2.Value));
			}
		}
		else if (globalBanpickTier <= GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Master && globalBanpickTier >= GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Platinum)
		{
			MatchBanPickTierData data2 = GameRoot.Instance.GlobalBanPickTierDataContainer.GetData(tierIndex);
			if (data2 != null)
			{
				UIHelper.SetLabelText(Label_Rank, LocalizationManager.GetStringDirect("MATCH_TOP_PERCENTAGE", data2.MaxPercent));
			}
			else
			{
				UIHelper.EnableGameObject(Label_Rank, enable: false);
			}
		}
		else
		{
			UIHelper.EnableGameObject(Label_Rank, enable: false);
		}
		if (_seasonType == MatchSeasonType.Regular)
		{
			RewardData data3 = GameRoot.Instance.RewardDataContainer.GetData(_currData.ShowRewardIndex);
			if (data3 != null)
			{
				List<RewardElementData> rewardItemList = data3.GetRewardItemList();
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(rewardItemList))
				{
					foreach (RewardElementData item in rewardItemList)
					{
						ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item.ItemCode);
						if (dataByCode != null)
						{
							ItemIconComponent itemIconComponent = UIHelper.AddGridItem(IconPostion, ItemIconComponent.PrefabAuto);
							itemIconComponent.Open(dataByCode, this);
							int itemMin = item.ItemMin;
							Color? color = null;
							int num2 = itemMin;
							itemIconComponent.SetItemCount(num2, color);
							_itemIconComponentList.Add(itemIconComponent);
						}
					}
				}
			}
			switch (globalBanpickTier)
			{
			case GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Ranker_1st:
			{
				ItemData dataByCode3 = GameRoot.Instance.ItemDataContainer.GetDataByCode(_seasonBuffItemCode[0]);
				if (dataByCode3 != null)
				{
					ItemIconComponent itemIconComponent3 = UIHelper.AddGridItem(IconPostion, ItemIconComponent.PrefabAuto);
					itemIconComponent3.Open(dataByCode3, this);
					_itemIconComponentList.Add(itemIconComponent3);
				}
				break;
			}
			case GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Ranker_2rd:
			{
				ItemData dataByCode4 = GameRoot.Instance.ItemDataContainer.GetDataByCode(_seasonBuffItemCode[1]);
				if (dataByCode4 != null)
				{
					ItemIconComponent itemIconComponent4 = UIHelper.AddGridItem(IconPostion, ItemIconComponent.PrefabAuto);
					itemIconComponent4.Open(dataByCode4, this);
					_itemIconComponentList.Add(itemIconComponent4);
				}
				break;
			}
			case GlobalBanPickTierRewardDataContainer.GlobalBanpickTier.Ranker_3rd:
			{
				ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(_seasonBuffItemCode[2]);
				if (dataByCode2 != null)
				{
					ItemIconComponent itemIconComponent2 = UIHelper.AddGridItem(IconPostion, ItemIconComponent.PrefabAuto);
					itemIconComponent2.Open(dataByCode2, this);
					_itemIconComponentList.Add(itemIconComponent2);
				}
				break;
			}
			}
			CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(IconPostion, CurrencyIconComponent.PrefabAuto);
			int seasonReward = _currData.SeasonReward;
			currencyIconComponent.Open(CurrencyType.RankingPoint, seasonReward, this);
			_currencyIconComponentList.Add(currencyIconComponent);
		}
		IconPostion.RepositionGrid();
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (_listener != null)
		{
			int num = 0;
			num = _itemIconComponentList.FindIndex((ItemIconComponent x) => x == itemIconComponent);
			if (num < 0)
			{
				num = 0;
			}
			_listener.OnClickShowMultipleDesc(_itemIconComponentList, _currencyIconComponentList, num);
		}
	}

	void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent currencyIconComponent)
	{
		if (_listener != null)
		{
			int num = 0;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(_itemIconComponentList))
			{
				num += __Extension_Method_ICollection__.COUNT(_itemIconComponentList);
			}
			int num2 = _currencyIconComponentList.FindIndex((CurrencyIconComponent x) => x == currencyIconComponent);
			num = ((num2 >= 0) ? (num + num2) : 0);
			_listener.OnClickShowMultipleDesc(_itemIconComponentList, _currencyIconComponentList, num);
		}
	}
}
