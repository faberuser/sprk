using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.ChapterClearReward;

public class ChapterRewardComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate, ItemIconComponent.Listener, CurrencyIconComponent.Listener
{
	public interface INotiListener
	{
		void IsGetReward(int index, bool isGetReward);
	}

	public interface IListener
	{
		void GetReward(int step, Action responseComplete);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/ChapterClearReward/ChapterRewardComponent";

	public static PrefabAuto<ChapterRewardComponent> PrefabAuto = new PrefabAuto<ChapterRewardComponent>("Resources/Prefab2/Component/ChapterClearReward/ChapterRewardComponent");

	private const float RewardComponentScale = 0.56f;

	public UISprite GetRewardSprite;

	public Transform GetRewardableEffectTranform;

	public Transform GetRewardBoosterEffectTransform;

	public UILabel ReqStarLabel;

	public UISpriteSelector RewardSpriteSeletor;

	public UIGame3DWidget GetRewardEffect;

	public UISimpleGrid RewardGrid;

	private IListener _iListener;

	private INotiListener _iNotiListener;

	private ChapterClearRewardData _data;

	private List<ItemIconComponent> _rewardItemComponentList = new List<ItemIconComponent>();

	private List<CurrencyIconComponent> _rewardCurrencyComponentList = new List<CurrencyIconComponent>();

	private bool _rewarded;

	private bool IsGetRewardable
	{
		get
		{
			if (_data == null)
			{
				return false;
			}
			int totalStarByChapterClearReward = GameRoot.Instance.CampaignDungeonDataContainer.GetTotalStarByChapterClearReward(_data.ChapterIndex);
			int num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.ClearDungeonStar(_data.ChapterIndex, totalStarByChapterClearReward);
			if (!_rewarded)
			{
				return _data.RewardReqStar <= num;
			}
			return false;
		}
	}

	public void Open(INotiListener iNotiListener, IListener iListener, ChapterClearRewardData data)
	{
		Init(iNotiListener, iListener, data);
	}

	private void Init(INotiListener iNotiListener, IListener iListener, ChapterClearRewardData data)
	{
		_iNotiListener = iNotiListener;
		_iListener = iListener;
		_data = data;
		InitReward();
		SetReqStar();
		SetRewardStatusInfo();
		NotiIsGetReward();
	}

	private void InitReward()
	{
		if (_data != null)
		{
			ClearRewardList();
			switch (_data.RewardType)
			{
			case RewardType.Item:
				CreateItemIconComponent();
				break;
			case RewardType.Gold:
			case RewardType.Gem:
				CreateCurrencyIconComponent();
				break;
			}
		}
	}

	private void SetRewared()
	{
		if (_data == null)
		{
			_rewarded = false;
		}
		else
		{
			_rewarded = GlobalAutoBehaviour<AccountManager>.instance.ChapterRewardManager.ChapterClearRewarded(_data.ChapterIndex, _data.StepIndex);
		}
	}

	private void SetRewardStatusInfo()
	{
		SetRewared();
		SetRewardStatus();
		EnableGetReward(_rewarded);
		EnableGetRewardableEffect(IsGetRewardable);
		EnableGetRewardableBoosterEffect(IsGetRewardable);
	}

	private void CreateCurrencyIconComponent()
	{
		if (_data != null)
		{
			CurrencyIconComponent currencyIconComponent = UIHelper.AddGridItem(RewardGrid, CurrencyIconComponent.PrefabAuto);
			if (currencyIconComponent != null)
			{
				CurrencyType currencyType = UIHelper.GetCurrencyType(_data.RewardType);
				int rewardCount = _data.RewardCount;
				currencyIconComponent.Open(currencyType, rewardCount, this);
				_rewardCurrencyComponentList.Add(currencyIconComponent);
			}
			UIHelper.RepositionGrid(RewardGrid);
		}
	}

	private void CreateItemIconComponent()
	{
		if (_data == null)
		{
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(_data.RewardItemCode);
		if (dataByCode != null)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(RewardGrid, ItemIconComponent.PrefabAuto);
			if (itemIconComponent != null)
			{
				itemIconComponent.Open(dataByCode, this);
				int rewardCount = _data.RewardCount;
				Color? color = null;
				int num = rewardCount;
				itemIconComponent.SetItemCount(num, color);
				_rewardItemComponentList.Add(itemIconComponent);
			}
			UIHelper.RepositionGrid(RewardGrid);
		}
	}

	private void SetReqStar()
	{
		if (_data != null && !(ReqStarLabel == null))
		{
			int rewardReqStar = GameRoot.Instance.ChapterClearRewardDataContainer.GetRewardReqStar(_data.ChapterIndex, _data.StepIndex);
			UIHelper.SetLabelText(ReqStarLabel, rewardReqStar);
		}
	}

	private void SetRewardStatus()
	{
		if (!(RewardSpriteSeletor == null))
		{
			RewardSpriteSeletor.Select(_rewarded.ToString());
		}
	}

	public void EnableGetReward(bool isEnable)
	{
		if (!(GetRewardSprite == null))
		{
			UIHelper.EnableGameObject(GetRewardSprite, isEnable);
		}
	}

	public void EnableGetRewardableEffect(bool isEnable)
	{
		if (!(GetRewardableEffectTranform == null))
		{
			UIHelper.EnableGameObject(GetRewardableEffectTranform, isEnable);
		}
	}

	public void EnableGetRewardableBoosterEffect(bool isEnable)
	{
		if (!(GetRewardBoosterEffectTransform == null))
		{
			UIHelper.EnableGameObject(GetRewardBoosterEffectTransform, isEnable);
		}
	}

	private void NotiIsGetReward()
	{
		if (_iNotiListener != null && _data != null)
		{
			_iNotiListener.IsGetReward(_data.StepIndex, IsGetRewardable);
		}
	}

	private void GetReward()
	{
		if (_iListener == null || _data == null || !IsGetRewardable)
		{
			return;
		}
		_iListener.GetReward(_data.StepIndex, delegate
		{
			if (!(GetRewardEffect == null))
			{
				GetRewardEffect.Play(null);
				SetRewardStatusInfo();
				NotiIsGetReward();
			}
		});
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		GetReward();
	}

	void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent component)
	{
		GetReward();
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		ClearRewardList();
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		ClearRewardList();
	}

	private void ClearRewardList()
	{
		FactoryBase.ReleaseAndClearList(_rewardCurrencyComponentList);
		FactoryBase.ReleaseAndClearList(_rewardItemComponentList);
	}
}
