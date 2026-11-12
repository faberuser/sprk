using System.Collections;
using System.Collections.Generic;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class MapEdge2 : LinkFactoryComponent, InitOnCreate
{
	public static PrefabAuto<MapEdge2> PrefabAuto = new PrefabAuto<MapEdge2>("Prefab2/WorldMap/Edge2");

	public float EdgeRad;

	public float MainFieldSizeX;

	public float MainFieldSizeY;

	public float BridgeSizeX;

	public float BridgeSizeY;

	public float StepDist;

	public float StepRatio;

	public Transform RootTransform;

	public float EffectDurationPerDot;

	public float EffectGapPerDot;

	public float EffectEndOverlap;

	public AnimationCurve ScaleCurve;

	public AnimationCurve AlphaCurve;

	private List<MapEdgeComponent> _listEdgeComponent = new List<MapEdgeComponent>();

	void InitOnCreate.OnCreate()
	{
		UIHelper.EnableGameObject(this, enable: true);
	}

	void InitOnCreate.OnRelease()
	{
		Reset();
	}

	private void Reset()
	{
		FactoryBase.ReleaseAndClearList(_listEdgeComponent);
	}

	private void FindIntersect(Vector3 p1, Vector3 p2, bool bridge, float[] p)
	{
		float num = p2.x - p1.x;
		float num2 = p2.y - p1.y;
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		float num4 = (bridge ? BridgeSizeX : MainFieldSizeX);
		float num5 = (bridge ? BridgeSizeY : MainFieldSizeY);
		float num6 = num * num / (num4 * num4) + num2 * num2 / (num5 * num5);
		num6 = 1f / num6;
		num6 = Mathf.Sqrt(num6);
		num6 += EdgeRad / num3;
		num6 = Mathf.Clamp(num6, 0f, 1f);
		p[0] = p1.x + num * num6;
		p[1] = p1.y + num2 * num6;
	}

	public void Open(Vector3 localStartPosition, Vector3 localEndPosition, bool leftBridge, bool rightBridge)
	{
		Reset();
		if (StepRatio < 1f)
		{
			StepRatio = 1f;
		}
		float[] array = new float[2];
		float[] array2 = new float[2];
		FindIntersect(localStartPosition, localEndPosition, leftBridge, array);
		FindIntersect(localEndPosition, localStartPosition, rightBridge, array2);
		float num = array2[0] - array[0];
		float num2 = array2[1] - array[1];
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		float num4 = Mathf.Sqrt(num * num + num2 * num2 * StepRatio * StepRatio);
		float num5 = StepDist * num3 / num4;
		int num6 = (int)(num3 / num5 - 0.1f);
		if (num6 > 0)
		{
			float num7 = (num3 - (float)num6 * num5) / 2f;
			float num8 = (array2[0] - array[0]) / num3;
			float num9 = (array2[1] - array[1]) / num3;
			Vector3 localTrans = default(Vector3);
			for (int i = 0; i <= num6; i++)
			{
				float num10 = num7 + (float)i * num5;
				localTrans.x = array[0] + num10 * num8;
				localTrans.y = array[1] + num10 * num9;
				localTrans.z = 0f;
				MapEdgeComponent mapEdgeComponent = MapEdgeComponent.PrefabAuto.Factory.Create(RootTransform);
				mapEdgeComponent.Open(localTrans);
				_listEdgeComponent.Add(mapEdgeComponent);
			}
		}
	}

	private float GetPlayTime()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listEdgeComponent))
		{
			return 0f;
		}
		return (float)(_listEdgeComponent.Count - 1) * EffectGapPerDot + EffectDurationPerDot;
	}

	public float GetNextEffectStartTime()
	{
		float playTime = GetPlayTime();
		if (!(playTime > EffectEndOverlap))
		{
			return 0f;
		}
		return playTime - EffectEndOverlap;
	}

	public void UpdateEffectDot(MapEdgeComponent comp, int idx, float elapsedTime)
	{
		if (!(comp == null) && AlphaCurve != null && ScaleCurve != null)
		{
			float value = (elapsedTime - (float)idx * EffectGapPerDot) / EffectDurationPerDot;
			value = Mathf.Clamp01(value);
			float alpha = AlphaCurve.Evaluate(value);
			float num = ScaleCurve.Evaluate(value);
			comp.ComponentWidget.alpha = alpha;
			comp.ComponentWidget.transform.localScale = Vector3.one * num;
		}
	}

	public IEnumerator coPlayEffect()
	{
		float _elapsedTime = 0f;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listEdgeComponent) || AlphaCurve == null)
		{
			yield break;
		}
		foreach (MapEdgeComponent item in _listEdgeComponent)
		{
			if (!(item == null))
			{
				UIHelper.EnableGameObject(item, enable: true);
				item.ComponentWidget.alpha = AlphaCurve.Evaluate(0f);
				item.ComponentWidget.transform.localScale = Vector3.one * ScaleCurve.Evaluate(0f);
			}
		}
		UIHelper.EnableGameObject(this, enable: true);
		float playTime = GetPlayTime();
		while (_elapsedTime < playTime)
		{
			_elapsedTime += GlobalTimer.deltaTime;
			for (int i = 0; i < _listEdgeComponent.Count; i++)
			{
				UpdateEffectDot(_listEdgeComponent[i], i, _elapsedTime);
			}
			yield return null;
		}
		foreach (MapEdgeComponent item2 in _listEdgeComponent)
		{
			if (!(item2 == null))
			{
				item2.ComponentWidget.alpha = AlphaCurve.Evaluate(1f);
				item2.ComponentWidget.transform.localScale = Vector3.one * ScaleCurve.Evaluate(1f);
			}
		}
	}
}
