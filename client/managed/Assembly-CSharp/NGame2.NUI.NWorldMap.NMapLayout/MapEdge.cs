using System;
using System.Collections.Generic;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class MapEdge : LinkFactoryComponent, InitOnCreate
{
	private class EdgeUiData
	{
		public UIWidget WayWidget;

		public UIWidget ShadowWidget;

		public float ShadowOffset;

		public float BaseScale;

		public EdgeUiData(UIWidget way, UIWidget shadow)
		{
			WayWidget = way;
			ShadowWidget = shadow;
			ShadowOffset = ShadowWidget.transform.localPosition.y;
			BaseScale = WayWidget.transform.localScale.y;
		}

		public void Set(int length, float angle, float thickness)
		{
			Quaternion localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
			float num = Mathf.Lerp(NodeShrinkRatio, 1f, Mathf.Abs(Mathf.Sin(angle * (MathF.PI / 180f))));
			WayWidget.transform.localRotation = localRotation;
			WayWidget.width = length;
			WayWidget.transform.localScale = new Vector3(1f, BaseScale * thickness * num, 1f);
			ShadowWidget.transform.localPosition = Vespa_Unity_Vector3_Extension_Method.ReplaceY(ShadowWidget.transform.localPosition, ShadowOffset * num);
			ShadowWidget.transform.localRotation = localRotation;
			ShadowWidget.width = WayWidget.width;
			ShadowWidget.transform.localScale = new Vector3(1f, BaseScale * thickness * num * NodeShadowScale, 1f);
		}

		public void Enable(bool isEnable)
		{
			UIHelper.EnableGameObject(WayWidget, isEnable);
			UIHelper.EnableGameObject(ShadowWidget, isEnable);
		}
	}

	public static PrefabAuto<MapEdge> PrefabAuto = new PrefabAuto<MapEdge>("Prefab2/WorldMap/Edge");

	[SerializeField]
	private UIWidget _bothway;

	[SerializeField]
	private UIWidget _oneway;

	[SerializeField]
	private UIWidget _notYet;

	[SerializeField]
	private UIWidget _bothwayShadow;

	[SerializeField]
	private UIWidget _onewayShadow;

	[SerializeField]
	private UIWidget _notYetShadow;

	private List<EdgeUiData> _edgeList = new List<EdgeUiData>();

	public static float NodeShrinkRatio => GameSetting.data.WorldMap.NodeShrinkRatio;

	public static float NodeShadowScale => GameSetting.data.WorldMap.NodeShadowScale;

	public int FromDungeonIndex { get; set; }

	public int ToDungeonIndex { get; set; }

	void InitOnCreate.OnCreate()
	{
		UIHelper.EnableGameObject(this, enable: true);
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(Vector3 localStartPosition, Vector3 localEndPosition, float thickness)
	{
		Vector3 vector = localEndPosition - localStartPosition;
		int num = Mathf.Max((int)vector.magnitude, 1);
		float angle = NVespa.Math.Angle(-Vespa_Unity_Vector3_Extension_Method.XY(vector / num));
		base.transform.localPosition = (localStartPosition + localEndPosition) * 0.5f;
		base.transform.localScale = Vector3.one;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_edgeList))
		{
			_edgeList.Add(new EdgeUiData(_bothway, _bothwayShadow));
			_edgeList.Add(new EdgeUiData(_oneway, _onewayShadow));
			_edgeList.Add(new EdgeUiData(_notYet, _notYetShadow));
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_edgeList); i++)
		{
			_edgeList[i].Set(num, angle, thickness);
		}
	}

	public void SetBothway()
	{
		_edgeList[0].Enable(isEnable: true);
		_edgeList[1].Enable(isEnable: false);
		_edgeList[2].Enable(isEnable: false);
	}

	public void SetOneway()
	{
		_edgeList[0].Enable(isEnable: false);
		_edgeList[1].Enable(isEnable: true);
		_edgeList[2].Enable(isEnable: false);
	}

	public void SetNotYet()
	{
		_edgeList[0].Enable(isEnable: false);
		_edgeList[1].Enable(isEnable: false);
		_edgeList[2].Enable(isEnable: true);
	}
}
