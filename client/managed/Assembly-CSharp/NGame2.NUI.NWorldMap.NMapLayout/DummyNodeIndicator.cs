using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class DummyNodeIndicator : NodeIndicator<DummyNodeIndicatorData>
{
	private class DummyTimer : ILocalTimer
	{
		public float Multiply;

		public ILocalTimer BaseTimer => UnityLocalTimer.Default;

		int ILocalTimer.deltaTick => Mathf.Max(1, Mathf.RoundToInt((float)BaseTimer.deltaTick * Multiply));

		int ILocalTimer.playbackSpeed => BaseTimer.playbackSpeed;

		bool ILocalTimer.pause => BaseTimer.pause;

		ETimerEventFlag ILocalTimer.TotalEventFlag => BaseTimer.TOTALEVENTFLAG();

		EAudioMixerFlag ILocalTimer.TotalAudioMixerFlag => BaseTimer.AUDIOMIXERFLAG();

		int ILocalTimer.GetPlaybackSpeedRecursively(int depth)
		{
			return BaseTimer.GetPlaybackSpeedRecursively(depth + 1);
		}

		bool ILocalTimer.GetPauseRecursively(int depth)
		{
			return BaseTimer.GetPauseRecursively(depth + 1);
		}

		public DummyTimer(float multiply)
		{
			Multiply = multiply;
		}
	}

	public DummyNodeIndicator(int chapterIndex, int dungeonIndex)
		: base(chapterIndex, dungeonIndex)
	{
	}

	public void Open(CrowdMediator mediator, string indicatorName, bool eatEffectOnClose)
	{
		if (mediator != null)
		{
			MapNode nodeByDungeonIndex = mediator.NodeGraph.GetNodeByDungeonIndex(base.DungeonIndex);
			Vector3 worldPosition = (nodeByDungeonIndex ? nodeByDungeonIndex.transform.position : Vector3.zero);
			StockData stockData = new StockData();
			stockData.MapComp = mediator.MapComponent;
			stockData.Data = new DummyNodeIndicatorData();
			stockData.AssetEffectCode = "UI.WorldMap.Indicator." + indicatorName;
			stockData.ParentTm = mediator.Layout.IndicatorRoot;
			stockData.WorldPosition = worldPosition;
			stockData.RenderModeWorldMap = true;
			stockData.RenderModeAngle = mediator.Layout.Render3dAngle;
			stockData.EatEffectOnClose = eatEffectOnClose;
			Open(stockData);
		}
	}

	public void Open(Mediator mediator, string indicatorName, bool eatEffectOnClose)
	{
		if (mediator != null)
		{
			MapNode nodeByDungeonIndex = mediator.NodeGraph.GetNodeByDungeonIndex(base.DungeonIndex);
			Vector3 worldPosition = (nodeByDungeonIndex ? nodeByDungeonIndex.transform.position : Vector3.zero);
			StockData stockData = new StockData();
			stockData.MapComp = mediator.MapComponent;
			stockData.Data = new DummyNodeIndicatorData();
			stockData.AssetEffectCode = "UI.WorldMap.Indicator." + indicatorName;
			stockData.ParentTm = mediator.Layout.IndicatorRoot;
			stockData.WorldPosition = worldPosition;
			stockData.RenderModeWorldMap = true;
			stockData.RenderModeAngle = mediator.Layout.Render3dAngle;
			stockData.EatEffectOnClose = eatEffectOnClose;
			Open(stockData);
		}
	}

	public void OpenAssetEffect(Mediator mediator, string effectCode, int positionX, int positionY, bool isWorldEffect)
	{
		if (mediator != null)
		{
			Vector3 position = new Vector3(positionX, positionY);
			Vector3 worldPosition = mediator.Layout.IndicatorRoot.TransformPoint(position);
			StockData stockData = new StockData();
			stockData.MapComp = mediator.MapComponent;
			stockData.Data = new DummyNodeIndicatorData();
			stockData.AssetEffectCode = effectCode;
			stockData.ParentTm = mediator.Layout.IndicatorRoot;
			stockData.WorldPosition = worldPosition;
			stockData.RenderModeWorldMap = isWorldEffect;
			stockData.RenderModeAngle = mediator.Layout.Render3dAngle;
			stockData.LocalTimer = new DummyTimer(Random.Range(0.8f, 1f));
			Open(stockData);
		}
	}
}
