using System;
using NGameAsset.NCreature;
using NGameAsset.NCreature.NAnimationSample;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NObject.NMotionTrail;

public class TrailRender
{
	public TrailInstance Trail;

	private MotionTrailObject _owner;

	private int _createSampleCount;

	private int _currentFrame;

	private float _currentTime;

	private int _prevFrame;

	private float _prevTime;

	private RenderGroupModifier2 _renderGroupModifier;

	public TrailRender(MotionTrailObject motionTrail)
	{
		_owner = motionTrail;
		_renderGroupModifier = SmallPooling<RenderGroupModifier2>.CreateInstance();
		_renderGroupModifier.Open(useSoloTimeColor: true);
	}

	public bool Open()
	{
		Trail = _owner.SampleData.CreateTrailInstance(_owner.CreateData.BeginFrame, _owner.CreateData.EndFrame);
		if (Trail == null)
		{
			return false;
		}
		_createSampleCount = 0;
		_currentFrame = 0;
		_currentTime = _owner.LocalTimer.time;
		_prevFrame = 0;
		_prevTime = _owner.LocalTimer.time;
		return true;
	}

	public void Close()
	{
		if (_renderGroupModifier != null)
		{
			SmallPooling<RenderGroupModifier2>.ReleaseInstance(_renderGroupModifier);
			_renderGroupModifier = null;
		}
		if (Trail != null)
		{
			_owner.SampleData.ReleaseTrailData(Trail);
			Trail = null;
		}
	}

	public void SetFrame(int frame)
	{
		if (_createSampleCount != Trail.SampleBuffer.Count && frame > _currentFrame)
		{
			_prevFrame = _currentFrame;
			_currentFrame = frame;
		}
	}

	public void ChainUpdate()
	{
		_renderGroupModifier.ChainUpdate(_owner.LocalTimer);
		_renderGroupModifier.BeginFlush();
		_renderGroupModifier.EndFlush();
		_prevTime = _currentTime;
		_currentTime = _owner.LocalTimer.time;
		if (_currentFrame > _owner.CreateData.BeginFrame)
		{
			if (_createSampleCount == 0 && _owner.Emit)
			{
				CreateTrailPoint();
			}
			else
			{
				UpdateTrailPoint();
			}
			RenderMesh2();
		}
	}

	private void RenderMesh2()
	{
		if (Trail.PointBuffer.Count >= 2 && !(_owner.ParentTm == null))
		{
			Trail.ReserveMeshRawData(Trail.PointBuffer.Count);
			FillVertexPosition();
			FillVertexUV();
			FillVertexColor();
			Trail.RawData.FillIndices(Trail.PointBuffer.Count);
			Trail.BuildMesh();
			Graphics.DrawMesh(Trail.Mesh, _owner.ParentTm.localToWorldMatrix, _owner.CreateData.MaterialAuto.Res, _owner.Layer, null, 0, _renderGroupModifier.PropertyBlock);
		}
	}

	private void FillVertexPosition()
	{
		for (int i = 0; i < Trail.PointBuffer.Count - 1; i++)
		{
			TrailInstance.TrailPoint trailPoint = Trail.PointBuffer.GetUnsafe(i);
			TrailInstance.TrailPoint trailPoint2 = Trail.PointBuffer.GetUnsafe(i + 1);
			Trail.RawData.verticies[i * 3] = trailPoint.Top;
			Trail.RawData.verticies[i * 3 + 1] = trailPoint.Bottom;
			Trail.RawData.verticies[i * 3 + 2] = (trailPoint.Top + trailPoint.Bottom + trailPoint2.Top + trailPoint2.Bottom) / 4f;
		}
		int num = Trail.PointBuffer.Count - 1;
		TrailInstance.TrailPoint trailPoint3 = Trail.PointBuffer.GetUnsafe(num);
		Trail.RawData.verticies[num * 3] = trailPoint3.Top;
		Trail.RawData.verticies[num * 3 + 1] = trailPoint3.Bottom;
	}

	private void FillVertexUV()
	{
		if (Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.TexCurve))
		{
			FillVertexUV_CalcNoTexCurve();
		}
		else
		{
			FillVertexUV_CalcTexCurve();
		}
	}

	private void FillVertexUV_Original()
	{
		float time = _owner.LocalTimer.time;
		if (_owner.CreateData.TexCurveKeyType == CreateData.EKeyType.LifeTime)
		{
			Func<float, float> func = ((!Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.TexCurve)) ? new Func<float, float>(_owner.CreateData.CalcTexCurve) : new Func<float, float>(_owner.CreateData.CalcNoTexCurve));
			float num = Trail.PointBuffer.GetUnsafe(0).EndTime;
			for (int i = 0; i < Trail.PointBuffer.Count - 1; i++)
			{
				float endTime = Trail.PointBuffer.GetUnsafe(i + 1).EndTime;
				float num2 = Mathf.Clamp01((num - time) / _owner.CreateData.LifeTime);
				float num3 = Mathf.Clamp01((endTime - time) / _owner.CreateData.LifeTime);
				float x = func(num2);
				float x2 = func((num2 + num3) * 0.5f);
				Trail.RawData.uvs[i * 3].x = x;
				Trail.RawData.uvs[i * 3 + 1].x = x;
				Trail.RawData.uvs[i * 3 + 2].x = x2;
				num = endTime;
			}
			int num4 = Trail.PointBuffer.Count - 1;
			float arg = Mathf.Clamp01((Trail.PointBuffer.GetUnsafe(num4).EndTime - time) / _owner.CreateData.LifeTime);
			float x3 = func(arg);
			Trail.RawData.uvs[num4 * 3].x = x3;
			Trail.RawData.uvs[num4 * 3 + 1].x = x3;
		}
	}

	private void FillVertexUV_CalcNoTexCurve()
	{
		float time = _owner.LocalTimer.time;
		if (_owner.CreateData.TexCurveKeyType == CreateData.EKeyType.LifeTime)
		{
			float num = Trail.PointBuffer.GetUnsafe(0).EndTime;
			for (int i = 0; i < Trail.PointBuffer.Count - 1; i++)
			{
				float endTime = Trail.PointBuffer.GetUnsafe(i + 1).EndTime;
				float num2 = Mathf.Clamp01((num - time) / _owner.CreateData.LifeTime);
				float num3 = Mathf.Clamp01((endTime - time) / _owner.CreateData.LifeTime);
				float x = num2;
				float x2 = (num2 + num3) * 0.5f;
				Trail.RawData.uvs[i * 3].x = x;
				Trail.RawData.uvs[i * 3 + 1].x = x;
				Trail.RawData.uvs[i * 3 + 2].x = x2;
				num = endTime;
			}
			int num4 = Trail.PointBuffer.Count - 1;
			float x3 = Mathf.Clamp01((Trail.PointBuffer.GetUnsafe(num4).EndTime - time) / _owner.CreateData.LifeTime);
			Trail.RawData.uvs[num4 * 3].x = x3;
			Trail.RawData.uvs[num4 * 3 + 1].x = x3;
		}
	}

	private void FillVertexUV_CalcTexCurve()
	{
		float time = _owner.LocalTimer.time;
		if (_owner.CreateData.TexCurveKeyType == CreateData.EKeyType.LifeTime)
		{
			float num = Trail.PointBuffer.GetUnsafe(0).EndTime;
			for (int i = 0; i < Trail.PointBuffer.Count - 1; i++)
			{
				float endTime = Trail.PointBuffer.GetUnsafe(i + 1).EndTime;
				float num2 = Mathf.Clamp01((num - time) / _owner.CreateData.LifeTime);
				float num3 = Mathf.Clamp01((endTime - time) / _owner.CreateData.LifeTime);
				float x = _owner.CreateData.CalcTexCurve(num2);
				float x2 = _owner.CreateData.CalcTexCurve((num2 + num3) * 0.5f);
				Trail.RawData.uvs[i * 3].x = x;
				Trail.RawData.uvs[i * 3 + 1].x = x;
				Trail.RawData.uvs[i * 3 + 2].x = x2;
				num = endTime;
			}
			int num4 = Trail.PointBuffer.Count - 1;
			float texUFrame = Mathf.Clamp01((Trail.PointBuffer.GetUnsafe(num4).EndTime - time) / _owner.CreateData.LifeTime);
			float x3 = _owner.CreateData.CalcTexCurve(texUFrame);
			Trail.RawData.uvs[num4 * 3].x = x3;
			Trail.RawData.uvs[num4 * 3 + 1].x = x3;
		}
	}

	private void FillVertexColor()
	{
		_ = _owner.LocalTimer.time;
		if (!Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.AlphaByCreateFrame) && !Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.AlphaByLifetime))
		{
			FillVertexColor_CalcAlphaByAllCurve();
		}
		else if (!Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.AlphaByCreateFrame) && Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.AlphaByLifetime))
		{
			FillVertexColor_CalcAlphaByCreateFrame();
		}
		else if (Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.AlphaByCreateFrame) && !Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.AlphaByLifetime))
		{
			FillVertexColor_CalcAlphaByLifetime();
		}
		else
		{
			FillVertexColor_CalcAlphaNoCurve();
		}
	}

	private void FillVertexColor_Original()
	{
		float time = _owner.LocalTimer.time;
		Func<float, float, float> func = ((!Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.AlphaByCreateFrame) && !Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.AlphaByLifetime)) ? new Func<float, float, float>(_owner.CreateData.CalcAlphaByAllCurve) : ((!Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.AlphaByCreateFrame) && Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.AlphaByLifetime)) ? new Func<float, float, float>(_owner.CreateData.CalcAlphaByCreateFrame) : ((!Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.AlphaByCreateFrame) || Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(_owner.CreateData.AlphaByLifetime)) ? new Func<float, float, float>(_owner.CreateData.CalcAlphaNoCurve) : new Func<float, float, float>(_owner.CreateData.CalcAlphaByLifetime))));
		for (int i = 0; i < Trail.PointBuffer.Count - 1; i++)
		{
			TrailInstance.TrailPoint trailPoint = Trail.PointBuffer.GetUnsafe(i);
			TrailInstance.TrailPoint trailPoint2 = Trail.PointBuffer.GetUnsafe(i + 1);
			float num = Mathf.Clamp01((trailPoint.EndTime - time) / _owner.CreateData.LifeTime);
			float num2 = func(trailPoint.CreateFrame, num);
			float num3 = Mathf.Clamp01((trailPoint2.EndTime - time) / _owner.CreateData.LifeTime);
			float arg = (num + num3) * 0.5f;
			float arg2 = (trailPoint.CreateFrame + trailPoint2.CreateFrame) * 0.5f;
			float num4 = func(arg2, arg);
			Trail.RawData.colors[i * 3].a = num2 * _renderGroupModifier.ExtraAlphaFinal;
			Trail.RawData.colors[i * 3 + 1].a = num2 * _renderGroupModifier.ExtraAlphaFinal;
			Trail.RawData.colors[i * 3 + 2].a = num4 * _renderGroupModifier.ExtraAlphaFinal;
		}
		int num5 = Trail.PointBuffer.Count - 1;
		TrailInstance.TrailPoint trailPoint3 = Trail.PointBuffer.GetUnsafe(num5);
		float arg3 = Mathf.Clamp01((trailPoint3.EndTime - time) / _owner.CreateData.LifeTime);
		float num6 = func(trailPoint3.CreateFrame, arg3);
		Trail.RawData.colors[num5 * 3].a = num6 * _renderGroupModifier.ExtraAlphaFinal;
		Trail.RawData.colors[num5 * 3 + 1].a = num6 * _renderGroupModifier.ExtraAlphaFinal;
	}

	private void FillVertexColor_CalcAlphaByAllCurve()
	{
		float time = _owner.LocalTimer.time;
		for (int i = 0; i < Trail.PointBuffer.Count - 1; i++)
		{
			TrailInstance.TrailPoint trailPoint = Trail.PointBuffer.GetUnsafe(i);
			TrailInstance.TrailPoint trailPoint2 = Trail.PointBuffer.GetUnsafe(i + 1);
			float num = Mathf.Clamp01((trailPoint.EndTime - time) / _owner.CreateData.LifeTime);
			float num2 = _owner.CreateData.CalcAlphaByAllCurve(trailPoint.CreateFrame, num);
			float num3 = Mathf.Clamp01((trailPoint2.EndTime - time) / _owner.CreateData.LifeTime);
			float lifetimeFrame = (num + num3) * 0.5f;
			float createFrame = (trailPoint.CreateFrame + trailPoint2.CreateFrame) * 0.5f;
			float num4 = _owner.CreateData.CalcAlphaByAllCurve(createFrame, lifetimeFrame);
			Trail.RawData.colors[i * 3].a = num2 * _renderGroupModifier.ExtraAlphaFinal;
			Trail.RawData.colors[i * 3 + 1].a = num2 * _renderGroupModifier.ExtraAlphaFinal;
			Trail.RawData.colors[i * 3 + 2].a = num4 * _renderGroupModifier.ExtraAlphaFinal;
		}
		int num5 = Trail.PointBuffer.Count - 1;
		TrailInstance.TrailPoint trailPoint3 = Trail.PointBuffer.GetUnsafe(num5);
		float lifetimeFrame2 = Mathf.Clamp01((trailPoint3.EndTime - time) / _owner.CreateData.LifeTime);
		float num6 = _owner.CreateData.CalcAlphaByAllCurve(trailPoint3.CreateFrame, lifetimeFrame2);
		Trail.RawData.colors[num5 * 3].a = num6 * _renderGroupModifier.ExtraAlphaFinal;
		Trail.RawData.colors[num5 * 3 + 1].a = num6 * _renderGroupModifier.ExtraAlphaFinal;
	}

	private void FillVertexColor_CalcAlphaByCreateFrame()
	{
		float time = _owner.LocalTimer.time;
		for (int i = 0; i < Trail.PointBuffer.Count - 1; i++)
		{
			TrailInstance.TrailPoint trailPoint = Trail.PointBuffer.GetUnsafe(i);
			TrailInstance.TrailPoint trailPoint2 = Trail.PointBuffer.GetUnsafe(i + 1);
			float num = Mathf.Clamp01((trailPoint.EndTime - time) / _owner.CreateData.LifeTime);
			float num2 = _owner.CreateData.CalcAlphaByCreateFrame(trailPoint.CreateFrame, num);
			float num3 = Mathf.Clamp01((trailPoint2.EndTime - time) / _owner.CreateData.LifeTime);
			float lifetimeFrame = (num + num3) * 0.5f;
			float createFrame = (trailPoint.CreateFrame + trailPoint2.CreateFrame) * 0.5f;
			float num4 = _owner.CreateData.CalcAlphaByCreateFrame(createFrame, lifetimeFrame);
			Trail.RawData.colors[i * 3].a = num2 * _renderGroupModifier.ExtraAlphaFinal;
			Trail.RawData.colors[i * 3 + 1].a = num2 * _renderGroupModifier.ExtraAlphaFinal;
			Trail.RawData.colors[i * 3 + 2].a = num4 * _renderGroupModifier.ExtraAlphaFinal;
		}
		int num5 = Trail.PointBuffer.Count - 1;
		TrailInstance.TrailPoint trailPoint3 = Trail.PointBuffer.GetUnsafe(num5);
		float lifetimeFrame2 = Mathf.Clamp01((trailPoint3.EndTime - time) / _owner.CreateData.LifeTime);
		float num6 = _owner.CreateData.CalcAlphaByCreateFrame(trailPoint3.CreateFrame, lifetimeFrame2);
		Trail.RawData.colors[num5 * 3].a = num6 * _renderGroupModifier.ExtraAlphaFinal;
		Trail.RawData.colors[num5 * 3 + 1].a = num6 * _renderGroupModifier.ExtraAlphaFinal;
	}

	private void FillVertexColor_CalcAlphaByLifetime()
	{
		float time = _owner.LocalTimer.time;
		for (int i = 0; i < Trail.PointBuffer.Count - 1; i++)
		{
			TrailInstance.TrailPoint trailPoint = Trail.PointBuffer.GetUnsafe(i);
			TrailInstance.TrailPoint trailPoint2 = Trail.PointBuffer.GetUnsafe(i + 1);
			float num = Mathf.Clamp01((trailPoint.EndTime - time) / _owner.CreateData.LifeTime);
			float num2 = _owner.CreateData.CalcAlphaByLifetime(trailPoint.CreateFrame, num);
			float num3 = Mathf.Clamp01((trailPoint2.EndTime - time) / _owner.CreateData.LifeTime);
			float lifetimeFrame = (num + num3) * 0.5f;
			float createFrame = (trailPoint.CreateFrame + trailPoint2.CreateFrame) * 0.5f;
			float num4 = _owner.CreateData.CalcAlphaByLifetime(createFrame, lifetimeFrame);
			Trail.RawData.colors[i * 3].a = num2 * _renderGroupModifier.ExtraAlphaFinal;
			Trail.RawData.colors[i * 3 + 1].a = num2 * _renderGroupModifier.ExtraAlphaFinal;
			Trail.RawData.colors[i * 3 + 2].a = num4 * _renderGroupModifier.ExtraAlphaFinal;
		}
		int num5 = Trail.PointBuffer.Count - 1;
		TrailInstance.TrailPoint trailPoint3 = Trail.PointBuffer.GetUnsafe(num5);
		float lifetimeFrame2 = Mathf.Clamp01((trailPoint3.EndTime - time) / _owner.CreateData.LifeTime);
		float num6 = _owner.CreateData.CalcAlphaByLifetime(trailPoint3.CreateFrame, lifetimeFrame2);
		Trail.RawData.colors[num5 * 3].a = num6 * _renderGroupModifier.ExtraAlphaFinal;
		Trail.RawData.colors[num5 * 3 + 1].a = num6 * _renderGroupModifier.ExtraAlphaFinal;
	}

	private void FillVertexColor_CalcAlphaNoCurve()
	{
		float time = _owner.LocalTimer.time;
		for (int i = 0; i < Trail.PointBuffer.Count - 1; i++)
		{
			TrailInstance.TrailPoint trailPoint = Trail.PointBuffer.GetUnsafe(i);
			TrailInstance.TrailPoint trailPoint2 = Trail.PointBuffer.GetUnsafe(i + 1);
			Mathf.Clamp01((trailPoint.EndTime - time) / _owner.CreateData.LifeTime);
			float num = Mathf.Clamp01((trailPoint2.EndTime - time) / _owner.CreateData.LifeTime);
			_ = trailPoint.CreateFrame;
			_ = trailPoint2.CreateFrame;
			Trail.RawData.colors[i * 3].a = _renderGroupModifier.ExtraAlphaFinal;
			Trail.RawData.colors[i * 3 + 1].a = _renderGroupModifier.ExtraAlphaFinal;
			Trail.RawData.colors[i * 3 + 2].a = _renderGroupModifier.ExtraAlphaFinal;
		}
		int num2 = Trail.PointBuffer.Count - 1;
		Mathf.Clamp01((Trail.PointBuffer.GetUnsafe(num2).EndTime - time) / _owner.CreateData.LifeTime);
		Trail.RawData.colors[num2 * 3].a = _renderGroupModifier.ExtraAlphaFinal;
		Trail.RawData.colors[num2 * 3 + 1].a = _renderGroupModifier.ExtraAlphaFinal;
	}

	private void CreateTrailPoint()
	{
		for (int i = _createSampleCount; i < Trail.SampleBuffer.Count; i++)
		{
			AnimationSampleData.SampleFrame sampleFrame = Trail.SampleBuffer.GetUnsafe(i);
			if (sampleFrame.Frame <= _currentFrame)
			{
				AddTrailPoint(sampleFrame.Frame, sampleFrame.Position, sampleFrame.Rotation);
				_createSampleCount++;
			}
		}
		if (_createSampleCount > 0 && _createSampleCount < Trail.SampleBuffer.Count - 1)
		{
			AnimationSampleData.SampleFrame sampleFrame2 = Trail.SampleBuffer.GetUnsafe(_createSampleCount - 1);
			AnimationSampleData.SampleFrame to = Trail.SampleBuffer.GetUnsafe(_createSampleCount);
			AnimationSampleData.SampleFrame.LerpValue(sampleFrame2, to, _currentFrame, out var oPosition, out var oRotation);
			AddTrailPoint(_currentFrame, oPosition, oRotation);
		}
	}

	private void UpdateTrailPoint()
	{
		if (_owner.Emit && _createSampleCount < Trail.SampleBuffer.Count)
		{
			if (Trail.PointBuffer.Count > 0)
			{
				TrailInstance.TrailPoint item = Trail.PointBuffer.GetUnsafe(Trail.PointBuffer.Count - 1);
				Trail.PointBuffer.RemoveAt(Trail.PointBuffer.Count - 1);
				ReservePoolUtil.Release(item);
			}
			CreateTrailPoint();
		}
		while (Trail.PointBuffer.Count > 1)
		{
			TrailInstance.TrailPoint trailPoint = Trail.PointBuffer[0];
			TrailInstance.TrailPoint trailPoint2 = Trail.PointBuffer[1];
			if (!(_owner.LocalTimer.time <= trailPoint.EndTime))
			{
				if (trailPoint2.EndTime - _owner.LocalTimer.time <= Mathf.Epsilon)
				{
					TrailInstance.TrailPoint item2 = Trail.PointBuffer.GetUnsafe(0);
					Trail.PointBuffer.RemoveAt(0);
					ReservePoolUtil.Release(item2);
					continue;
				}
				float t = Mathf.InverseLerp(trailPoint.EndTime, trailPoint2.EndTime, _owner.LocalTimer.time);
				Vector3 top = Vector3.Lerp(trailPoint.Top, trailPoint2.Top, t);
				Vector3 bottom = Vector3.Lerp(trailPoint.Bottom, trailPoint2.Bottom, t);
				float createFrame = Mathf.Lerp(trailPoint.CreateFrame, trailPoint2.CreateFrame, t);
				float createTime = Mathf.Lerp(trailPoint.CreateTime, trailPoint2.CreateTime, t);
				_ = _owner.LocalTimer.time;
				trailPoint.Top = top;
				trailPoint.Bottom = bottom;
				trailPoint.CreateFrame = createFrame;
				trailPoint.CreateTime = createTime;
				trailPoint.EndTime = _owner.LocalTimer.time;
				break;
			}
			break;
		}
	}

	private void AddTrailPoint(float frame, Vector3 sampleFramePosition, Quaternion sampleFrameRotation)
	{
		float t = Mathf.InverseLerp(_prevFrame, _currentFrame, frame);
		TrailInstance.TrailPoint trailPoint = ReservePoolUtil.Create<TrailInstance.TrailPoint>();
		trailPoint.Top = sampleFramePosition + sampleFrameRotation * _owner.CreateData.TopOffset;
		trailPoint.Bottom = sampleFramePosition + sampleFrameRotation * _owner.CreateData.BottomOffset;
		trailPoint.CreateTime = Mathf.Lerp(_prevTime, _currentTime, t);
		trailPoint.EndTime = trailPoint.CreateTime + _owner.CreateData.LifeTime;
		trailPoint.CreateFrame = Mathf.InverseLerp(_owner.CreateData.BeginFrame, _owner.CreateData.EndFrame, frame);
		Trail.PointBuffer.Add(trailPoint);
	}
}
