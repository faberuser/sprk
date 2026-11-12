using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NShared;
using NShared.NWorldMap;
using NShared.NWorldMap.NEventSubData;
using NShared.ReceiveCompletedReward;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NEventManager;

public class MapCoinManager : EventIndicatorManager<MapCoin>
{
	private class CoinInfo
	{
		public int EventIndex;

		public int ChapterIndex;

		public int DungeonIndex;
	}

	public const float RequestReceiveMapCoinInterval = 5f;

	private List<CoinInfo> _queuedMapCoinList = new List<CoinInfo>();

	private float _lastRequestReceiveMapCoinTime;

	public override void Open(EventManager eventManager)
	{
		base.Open(eventManager);
		_eventType = NShared.NWorldMap.EventType.MapCoin;
		_useEatEffect = true;
		_lastRequestReceiveMapCoinTime = 0f;
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		UpdateReceiveMapCoin();
	}

	public override void Visit()
	{
		base.Visit();
		ReceiveMapCoin();
	}

	public override EIndicatorType GetIndicatorType(WorldMapEventInfo worldMapEventInfo)
	{
		if (worldMapEventInfo.RewardGold > 0)
		{
			return EIndicatorType.Coin;
		}
		if (worldMapEventInfo.RewardGem > 0)
		{
			return EIndicatorType.Ruby;
		}
		if (!string.IsNullOrEmpty(worldMapEventInfo.RewardItems))
		{
			try
			{
				if (__Extension_Method_ICollection__.COUNT(BaseJsonMarshaler.ParseArray(worldMapEventInfo.RewardItems, JsonMarshaler.Parse<RewardItemInfo>)) > 0)
				{
					return EIndicatorType.Ruby;
				}
			}
			catch (Exception ex)
			{
				VespaDebug.LogError("Parsing error " + ex.ToString() + " string=" + worldMapEventInfo.RewardItems);
			}
		}
		return EIndicatorType.None;
	}

	private void ReceiveMapCoin()
	{
		_ = _eventManager.Mediator;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		NodeIndicator<MapCoin> nodeIndicator = FindIndicatorByData((MapCoin e) => e.ChapterIndex == chapterIndex && e.DungeonIndex == dungeonIndex);
		if (nodeIndicator != null)
		{
			int eventIndex = nodeIndicator.Data.EventIndex;
			if (GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.Get(eventIndex, chapterIndex, dungeonIndex) != null)
			{
				RemoveIndicator(nodeIndicator, eatEffect: true);
				ReceiveMapCoin(eventIndex, chapterIndex, dungeonIndex);
			}
		}
	}

	private void ReceiveMapCoin(int eventIndex, int chapterIndex, int dungeonIndex)
	{
		CoinInfo item = new CoinInfo
		{
			EventIndex = eventIndex,
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex
		};
		for (int i = 0; i < _queuedMapCoinList.Count; i++)
		{
			CoinInfo coinInfo = _queuedMapCoinList[i];
			if (((coinInfo.EventIndex == eventIndex) & (coinInfo.ChapterIndex == chapterIndex)) && coinInfo.DungeonIndex == dungeonIndex)
			{
				return;
			}
		}
		WorldMapEventInfo worldMapEventInfo = GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.Get(eventIndex, chapterIndex, dungeonIndex);
		if (worldMapEventInfo != null && worldMapEventInfo.EventType == NShared.NWorldMap.EventType.MapCoin)
		{
			if (_queuedMapCoinList.Count > 0 && _queuedMapCoinList[_queuedMapCoinList.Count - 1].ChapterIndex != chapterIndex)
			{
				RequestReceiveMapCoin();
			}
			_queuedMapCoinList.Add(item);
			GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.Remove(new WorldMapEventInfoKey(eventIndex, chapterIndex, dungeonIndex));
			checked
			{
				if (worldMapEventInfo.RewardGold > 0)
				{
					long gold = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold + worldMapEventInfo.RewardGold;
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetGold(gold);
				}
				else if (worldMapEventInfo.RewardGem > 0)
				{
					long gem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gem;
					int rewardGem = worldMapEventInfo.RewardGem;
					long gem2 = gem + rewardGem;
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetGem(gem2);
				}
			}
		}
	}

	private void UpdateReceiveMapCoin()
	{
		float time = Time.time;
		if (!(time - _lastRequestReceiveMapCoinTime < 5f))
		{
			_lastRequestReceiveMapCoinTime = time;
			RequestReceiveMapCoin();
		}
	}

	private void RequestReceiveMapCoin(Action<NShared.ReceiveCompletedReward.Response> onResponse = null)
	{
		if (_queuedMapCoinList.Count != 0)
		{
			int chapterIndex = _queuedMapCoinList[0].ChapterIndex;
			int[] eventIndices = _queuedMapCoinList.Select((CoinInfo e) => e.EventIndex).ToArray();
			int[] dungeonIndices = _queuedMapCoinList.Select((CoinInfo e) => e.DungeonIndex).ToArray();
			_queuedMapCoinList.Clear();
			_eventManager.RequestReceiveCompletedReward(chapterIndex, dungeonIndices, eventIndices, delegate(NShared.ReceiveCompletedReward.Response response)
			{
				_eventManager.ApplyReceiveCompletedRewardResponse(response);
				Vespa_System_Action_Extension_Method.fire(onResponse, response);
			});
		}
	}
}
