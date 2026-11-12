using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NWorldMap;
using NShared.NWorldMap.NEventSubData;
using NShared.ReceiveCompletedReward;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NEventManager;

public class ExcavationManager : EventIndicatorManager<Excavation>
{
	public const float ExcavationTime = 3f;

	private ExcavationProgress _excavationProgress;

	private Coroutine _excavationCoroutine;

	public override void Open(EventManager eventManager)
	{
		base.Open(eventManager);
		_eventType = NShared.NWorldMap.EventType.Excavation;
		_useEatEffect = true;
	}

	public override void Close()
	{
		base.Close();
		CloseExcavationProgress();
	}

	public override EIndicatorType GetIndicatorType(WorldMapEventInfo worldMapEventInfo)
	{
		return EIndicatorType.Excavation;
	}

	public void StartExcavation(Action<NShared.ReceiveCompletedReward.Response> onOpenReward = null)
	{
		_eventManager.Mediator.Viewer.ChasePlayer();
		VespaCoroutine.StartScene(coStartExcavation(onOpenReward));
	}

	private IEnumerator coStartExcavation(Action<NShared.ReceiveCompletedReward.Response> onOpenReward)
	{
		_excavationCoroutine = VespaCoroutine.StartScene(coExcavation(onOpenReward));
		LabelPopup popup = LabelPopup.Open();
		yield return _excavationCoroutine;
		popup.Close();
		_excavationCoroutine = null;
	}

	private IEnumerator coExcavation(Action<NShared.ReceiveCompletedReward.Response> onOpenReward)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		MapNode dungeonNode = _eventManager.Mediator.NodeGraph.GetNodeByDungeonIndex(dungeonIndex);
		if (dungeonNode == null)
		{
			MessagePopup.OpenError(ResultType.DungeonNotFound);
			yield break;
		}
		WorldMapEventInfo worldMapEventInfo = GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.Get(NShared.NWorldMap.EventType.Excavation, chapterIndex, dungeonIndex);
		if (worldMapEventInfo == null)
		{
			MessagePopup.OpenError(ResultType.WorldMapEventNotFound);
			yield break;
		}
		int eventIndex = worldMapEventInfo.EventIndex;
		NShared.ReceiveCompletedReward.Response rewardResponse = null;
		_eventManager.RequestReceiveCompletedReward(chapterIndex, new int[1] { dungeonIndex }, new int[1] { eventIndex }, delegate(NShared.ReceiveCompletedReward.Response response)
		{
			if (response == null || response.BaseResult != BaseResultType.Success || response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(ResultType.Fail);
			}
			else
			{
				rewardResponse = response;
			}
		});
		NodeIndicator<Excavation> nodeIndicator = FindIndicatorByData((Excavation e) => e.ChapterIndex == chapterIndex && e.DungeonIndex == dungeonIndex);
		if (nodeIndicator != null)
		{
			RemoveIndicator(nodeIndicator, eatEffect: true);
		}
		GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.Remove(worldMapEventInfo);
		bool progressEnded = false;
		CloseExcavationProgress();
		_excavationProgress = ExcavationProgress.PrefabAuto.Factory.Create(_eventManager.Mediator.Layout.EventRoot);
		_excavationProgress.transform.position = dungeonNode.transform.position;
		_excavationProgress.transform.localScale = Vector3.one;
		_excavationProgress.Open(3f, delegate
		{
			CloseExcavationProgress();
			progressEnded = true;
		});
		while (!progressEnded)
		{
			yield return null;
		}
		float waitStartTime = Time.time;
		while (rewardResponse == null && Time.time - waitStartTime < 10f)
		{
			yield return null;
		}
		if (rewardResponse == null)
		{
			MessagePopup.OpenError(ResultType.Fail);
			yield break;
		}
		yield return VespaCoroutine.StartScene(coBoxOpen(dungeonNode));
		yield return new WaitForSeconds(0.5f);
		Vespa_System_Action_Extension_Method.fire(onOpenReward, rewardResponse);
	}

	private IEnumerator coBoxOpen(MapNode dungeonNode)
	{
		DummyNodeIndicator boxIndicator = new DummyNodeIndicator(dungeonNode.ChapterIndex, dungeonNode.DungeonIndex);
		boxIndicator.Open(_eventManager.Mediator, "Box", eatEffectOnClose: false);
		float startTime = Time.time;
		float duration = 1f;
		while (Time.time - startTime < duration)
		{
			yield return null;
		}
		boxIndicator.Close(eatEffect: true);
	}

	public bool IsExcavating()
	{
		return _excavationCoroutine != null;
	}

	private void CloseExcavationProgress()
	{
		if ((bool)_excavationProgress)
		{
			FactoryBase.ReleaseComponent(_excavationProgress);
			_excavationProgress = null;
		}
	}
}
