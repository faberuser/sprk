using System;
using System.Collections.Generic;
using System.Linq;
using NVespa;
using NVespa.NSingleton;
using PigeonCoopToolkit.Utillities;
using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails;

public abstract class TrailRenderer_Base : MonoBehaviour
{
	private int _frameCount;

	public PCTrailRendererData TrailData;

	public bool Emit;

	protected bool _emit;

	private PCTrail _activeTrail;

	private List<PCTrail> _fadingTrails;

	protected Transform _t;

	protected virtual void Awake()
	{
		SceneAutoBehaviour<HandlerVespa>.instance.GlobalTrailRendererCount++;
		_fadingTrails = new List<PCTrail>();
		_t = base.transform;
		_emit = Emit;
		if (_emit)
		{
			_activeTrail = new PCTrail(GetMaxNumberOfPoints());
			_activeTrail.IsActiveTrail = true;
			OnStartEmit();
		}
	}

	protected virtual void Start()
	{
	}

	public void ChainUpdate(ILocalTimer timer, float deltaTime)
	{
		if (_frameCount != Time.frameCount)
		{
			_frameCount = Time.frameCount;
			if (_t == null && _emit)
			{
				Emit = false;
				CheckEmitChange();
			}
			InternalUpdate(timer, deltaTime);
		}
	}

	protected virtual void InternalUpdate(ILocalTimer timer, float deltaTime)
	{
		SceneAutoBehaviour<HandlerVespa>.instance.ReserveMaterial(TrailData.TrailMaterial);
		if (_activeTrail != null)
		{
			UpdatePoints(_activeTrail, timer, deltaTime);
			UpdateTrail(_activeTrail, timer, deltaTime);
			GenerateMesh(_activeTrail);
			SceneAutoBehaviour<HandlerVespa>.instance.AddTrail(TrailData.TrailMaterial, _activeTrail);
		}
		for (int num = _fadingTrails.Count - 1; num >= 0; num--)
		{
			if (_fadingTrails[num] == null || !_fadingTrails[num].Points.Any((PCTrailPoint a) => a.TimeActive() < TrailData.Lifetime))
			{
				if (_fadingTrails[num] != null)
				{
					_fadingTrails[num].Dispose();
				}
				_fadingTrails.RemoveAt(num);
			}
			else
			{
				UpdatePoints(_fadingTrails[num], timer, deltaTime);
				UpdateTrail(_fadingTrails[num], timer, deltaTime);
				GenerateMesh(_fadingTrails[num]);
				SceneAutoBehaviour<HandlerVespa>.instance.AddTrail(TrailData.TrailMaterial, _fadingTrails[num]);
			}
		}
		CheckEmitChange();
	}

	protected virtual void OnDestroy()
	{
		if (SystemStatus.IsApplicationQuit)
		{
			return;
		}
		if (SceneAutoBehaviour<HandlerVespa>.isValidInstance)
		{
			SceneAutoBehaviour<HandlerVespa>.instance.GlobalTrailRendererCount--;
		}
		if (_activeTrail != null)
		{
			_activeTrail.Dispose();
			_activeTrail = null;
		}
		if (_fadingTrails == null)
		{
			return;
		}
		foreach (PCTrail fadingTrail in _fadingTrails)
		{
			fadingTrail?.Dispose();
		}
		_fadingTrails.Clear();
	}

	protected virtual void OnStopEmit()
	{
	}

	protected virtual void OnStartEmit()
	{
	}

	protected virtual void OnTranslate(Vector3 t)
	{
	}

	protected abstract int GetMaxNumberOfPoints();

	protected virtual void Reset()
	{
		if (TrailData == null)
		{
			TrailData = new PCTrailRendererData();
		}
		TrailData.Lifetime = 1f;
		TrailData.ColorOverLife = new Gradient();
		TrailData.SizeOverLife = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
	}

	protected virtual void InitialiseNewPoint(PCTrailPoint newPoint)
	{
	}

	protected virtual void UpdateTrail(PCTrail trail, ILocalTimer timer, float deltaTime)
	{
	}

	protected void AddPoint(PCTrailPoint newPoint, Vector3 pos)
	{
		if (_activeTrail != null)
		{
			newPoint.Position = pos;
			newPoint.PointNumber = ((_activeTrail.Points.Count != 0) ? (_activeTrail.Points[_activeTrail.Points.Count - 1].PointNumber + 1) : 0);
			InitialiseNewPoint(newPoint);
			newPoint.SetDistanceFromStart((_activeTrail.Points.Count == 0) ? 0f : (_activeTrail.Points[_activeTrail.Points.Count - 1].GetDistanceFromStart() + Vector3.Distance(_activeTrail.Points[_activeTrail.Points.Count - 1].Position, pos)));
			if (TrailData.UseForwardOverride)
			{
				newPoint.Forward = (TrailData.ForwardOverrideRelative ? _t.TransformDirection(TrailData.ForwardOverride.normalized) : TrailData.ForwardOverride.normalized);
			}
			_activeTrail.Points.Add(newPoint);
		}
	}

	private void GenerateMesh(PCTrail trail)
	{
		trail.Mesh.Clear(keepVertexLayout: false);
		Vector3 vector = ((Camera.main != null) ? Camera.main.transform.forward : Vector3.forward);
		if (TrailData.UseForwardOverride)
		{
			vector = TrailData.ForwardOverride.normalized;
		}
		trail.activePointCount = NumberOfActivePoints(trail);
		if (trail.activePointCount < 2)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < trail.Points.Count; i++)
		{
			PCTrailPoint pCTrailPoint = trail.Points[i];
			float num2 = pCTrailPoint.TimeActive() / TrailData.Lifetime;
			if (!(pCTrailPoint.TimeActive() > TrailData.Lifetime))
			{
				if (TrailData.UseForwardOverride && TrailData.ForwardOverrideRelative)
				{
					vector = pCTrailPoint.Forward;
				}
				Vector3 zero = Vector3.zero;
				zero = ((i >= trail.Points.Count - 1) ? Vector3.Cross((pCTrailPoint.Position - trail.Points[i - 1].Position).normalized, vector).normalized : Vector3.Cross((trail.Points[i + 1].Position - pCTrailPoint.Position).normalized, vector).normalized);
				Color color = (TrailData.StretchColorToFit ? TrailData.GetColor(1f - (float)num / (float)trail.activePointCount / 2f) : TrailData.GetColor(num2));
				float num3 = (TrailData.StretchSizeToFit ? TrailData.GetTrailSize(1f - (float)num / (float)trail.activePointCount / 2f) : TrailData.GetTrailSize(num2));
				trail.verticies[num] = pCTrailPoint.Position + zero * num3;
				float num4 = (TrailData.TimeBaseTexMapping ? Mathf.Clamp01(num2) : ((!(TrailData.MaterialTileLength <= 0f)) ? (pCTrailPoint.GetDistanceFromStart() / TrailData.MaterialTileLength) : ((float)num / (float)trail.activePointCount / 2f)));
				trail.uvs[num] = new Vector2(Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(TrailData.TexRemapping, num4, num4), 0f);
				trail.normals[num] = vector;
				trail.colors[num] = color;
				num++;
				trail.verticies[num] = pCTrailPoint.Position - zero * num3;
				num4 = (TrailData.TimeBaseTexMapping ? Mathf.Clamp01(num2) : ((!(TrailData.MaterialTileLength <= 0f)) ? (pCTrailPoint.GetDistanceFromStart() / TrailData.MaterialTileLength) : ((float)num / (float)trail.activePointCount / 2f)));
				trail.uvs[num] = new Vector2(Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(TrailData.TexRemapping, num4, num4), 1f);
				trail.normals[num] = vector;
				trail.colors[num] = color;
				num++;
			}
		}
		Vector2 vector2 = trail.verticies[num - 1];
		for (int j = num; j < trail.verticies.Length; j++)
		{
			trail.verticies[j] = vector2;
		}
		int num5 = 0;
		for (int k = 0; k < 2 * (trail.activePointCount - 1); k++)
		{
			if (k % 2 == 0)
			{
				trail.indicies[num5] = k;
				num5++;
				trail.indicies[num5] = k + 1;
				num5++;
				trail.indicies[num5] = k + 2;
			}
			else
			{
				trail.indicies[num5] = k + 2;
				num5++;
				trail.indicies[num5] = k + 1;
				num5++;
				trail.indicies[num5] = k;
			}
			num5++;
		}
		int num6 = trail.indicies[num5 - 1];
		for (int l = num5; l < trail.indicies.Length; l++)
		{
			trail.indicies[l] = num6;
		}
		trail.Mesh.vertices = trail.verticies;
		trail.Mesh.SetIndices(trail.indicies, MeshTopology.Triangles, 0);
		trail.Mesh.uv = trail.uvs;
		trail.Mesh.normals = trail.normals;
		trail.Mesh.colors = trail.colors;
	}

	private void DrawMesh(Mesh trailMesh, Material trailMaterial)
	{
		Graphics.DrawMesh(trailMesh, Matrix4x4.identity, trailMaterial, base.gameObject.layer);
	}

	private void UpdatePoints(PCTrail line, ILocalTimer timer, float deltaTime)
	{
		for (int i = 0; i < line.Points.Count; i++)
		{
			line.Points[i].Update(deltaTime);
		}
	}

	[Obsolete("UpdatePoint is deprecated, you should instead override UpdateTrail and loop through the individual points yourself (See Smoke or Smoke Plume scripts for how to do this).", true)]
	protected virtual void UpdatePoint(PCTrailPoint pCTrailPoint, float deltaTime)
	{
	}

	private void CheckEmitChange()
	{
		if (_emit != Emit)
		{
			SetEmitChange(Emit);
		}
	}

	private void SetEmitChange(bool emit)
	{
		_emit = emit;
		if (_emit)
		{
			_activeTrail = new PCTrail(GetMaxNumberOfPoints());
			_activeTrail.IsActiveTrail = true;
			OnStartEmit();
			return;
		}
		OnStopEmit();
		if (_activeTrail != null)
		{
			_activeTrail.IsActiveTrail = false;
			_fadingTrails.Add(_activeTrail);
			_activeTrail = null;
		}
	}

	private int NumberOfActivePoints(PCTrail line)
	{
		int num = 0;
		for (int i = 0; i < line.Points.Count; i++)
		{
			if (line.Points[i].TimeActive() < TrailData.Lifetime)
			{
				num++;
			}
		}
		return num;
	}

	public void Translate(Vector3 t)
	{
		if (_activeTrail != null)
		{
			for (int i = 0; i < _activeTrail.Points.Count; i++)
			{
				_activeTrail.Points[i].Position += t;
			}
		}
		if (_fadingTrails != null)
		{
			foreach (PCTrail fadingTrail in _fadingTrails)
			{
				for (int j = 0; j < fadingTrail.Points.Count; j++)
				{
					fadingTrail.Points[j].Position += t;
				}
			}
		}
		OnTranslate(t);
	}

	public void CreateTrail(Vector3 from, Vector3 to, float distanceBetweenPoints)
	{
		float num = Vector3.Distance(from, to);
		Vector3 normalized = (to - from).normalized;
		float num2 = 0f;
		PigeonCoopToolkit.Utillities.CircularBuffer<PCTrailPoint> circularBuffer = new PigeonCoopToolkit.Utillities.CircularBuffer<PCTrailPoint>(GetMaxNumberOfPoints());
		int num3 = 0;
		for (; num2 < num; num2 += distanceBetweenPoints)
		{
			PCTrailPoint pCTrailPoint = new PCTrailPoint();
			pCTrailPoint.PointNumber = num3;
			pCTrailPoint.Position = from + normalized * num2;
			circularBuffer.Add(pCTrailPoint);
			InitialiseNewPoint(pCTrailPoint);
			num3++;
			if (distanceBetweenPoints <= 0f)
			{
				break;
			}
		}
		PCTrailPoint pCTrailPoint2 = new PCTrailPoint();
		pCTrailPoint2.PointNumber = num3;
		pCTrailPoint2.Position = to;
		circularBuffer.Add(pCTrailPoint2);
		InitialiseNewPoint(pCTrailPoint2);
		PCTrail pCTrail = new PCTrail(GetMaxNumberOfPoints());
		pCTrail.Points = circularBuffer;
		_fadingTrails.Add(pCTrail);
	}

	public void ClearSystem(bool emitState)
	{
		if (_activeTrail != null)
		{
			_activeTrail.Dispose();
			_activeTrail = null;
		}
		if (_fadingTrails != null)
		{
			foreach (PCTrail fadingTrail in _fadingTrails)
			{
				fadingTrail?.Dispose();
			}
			_fadingTrails.Clear();
		}
		Emit = emitState;
		_emit = !emitState;
		CheckEmitChange();
	}

	public int NumSegments()
	{
		int num = 0;
		if (_activeTrail != null && NumberOfActivePoints(_activeTrail) != 0)
		{
			num++;
		}
		return num + _fadingTrails.Count;
	}
}
