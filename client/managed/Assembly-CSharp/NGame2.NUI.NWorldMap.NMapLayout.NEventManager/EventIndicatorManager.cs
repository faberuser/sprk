using NGame2.NAccount;
using NShared;
using NShared.NWorldMap;
using NVespa.NSingleton;

namespace NGame2.NUI.NWorldMap.NMapLayout.NEventManager;

public abstract class EventIndicatorManager<TEventSubData> : IndicatorManager<TEventSubData> where TEventSubData : EventSubData
{
	public abstract EIndicatorType GetIndicatorType(WorldMapEventInfo worldMapEventInfo);

	public override NodeIndicator<TEventSubData> CreateIndicator(WorldMapEventInfo worldMapEventInfo)
	{
		if (worldMapEventInfo == null)
		{
			return null;
		}
		if (_eventType == EventType.None)
		{
			return null;
		}
		if (worldMapEventInfo.EventType != _eventType)
		{
			return null;
		}
		Mediator mediator = _eventManager.Mediator;
		int chapterIndex = worldMapEventInfo.ChapterIndex;
		if (chapterIndex != NGame2.NAccount.UserCampaignManager.ChapterIndex)
		{
			return null;
		}
		int dungeonIndex = worldMapEventInfo.DungeonIndex;
		TEventSubData eventData = GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.GetEventData<TEventSubData>(chapterIndex, dungeonIndex);
		if (eventData == null)
		{
			return null;
		}
		MapNode nodeByDungeonIndex = mediator.NodeGraph.GetNodeByDungeonIndex(dungeonIndex);
		if (nodeByDungeonIndex == null)
		{
			return null;
		}
		EIndicatorType indicatorType = GetIndicatorType(worldMapEventInfo);
		if (indicatorType == EIndicatorType.None)
		{
			return null;
		}
		NodeIndicator<TEventSubData> nodeIndicator = new NodeIndicator<TEventSubData>(chapterIndex, dungeonIndex);
		nodeIndicator.Open(new NodeIndicator<TEventSubData>.StockData
		{
			MapComp = mediator.MapComponent,
			Data = eventData,
			AssetEffectCode = "UI.WorldMap.Indicator." + Vespa_String_Extension_Method.GetEnumString(indicatorType),
			ParentTm = mediator.Layout.IndicatorRoot,
			WorldPosition = nodeByDungeonIndex.transform.position,
			RenderModeWorldMap = true,
			RenderModeAngle = mediator.Layout.Render3dAngle,
			EatEffectOnClose = _useEatEffect
		});
		return nodeIndicator;
	}
}
