using System.Collections.Generic;
using JsonBin;
using NGameAsset.NCreature.NAnimationSample;
using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature;

public class AnimationSampleData : ILoadJson
{
	public class SampleFrame : ILoadJson
	{
		public bool IsDynamic { get; private set; }

		public int Frame { get; private set; }

		public Vector3 Position { get; private set; }

		public Quaternion Rotation { get; private set; }

		bool ILoadJson.Load(JsonRead json)
		{
			Frame = json["Frame"].GETINT(Frame);
			Position = json["Position"].GETVECTOR3(Position);
			Rotation = json["Rotation"].GETQUATERNION(Rotation);
			return true;
		}

		public static SampleFrame CreateSample(int frame, Vector3 position, Quaternion rotation, bool isDynamic)
		{
			SampleFrame sampleFrame = ReservePoolUtil.Create<SampleFrame>();
			sampleFrame.IsDynamic = isDynamic;
			sampleFrame.Frame = frame;
			sampleFrame.Position = position;
			sampleFrame.Rotation = rotation;
			return sampleFrame;
		}

		public static SampleFrame Lerp(SampleFrame from, SampleFrame to, int frame)
		{
			float t = Mathf.InverseLerp(from.Frame, to.Frame, frame);
			return CreateSample(frame, Vector3.Lerp(from.Position, to.Position, t), Quaternion.Lerp(from.Rotation, to.Rotation, t), isDynamic: true);
		}

		public static void LerpValue(SampleFrame from, SampleFrame to, int frame, out Vector3 oPosition, out Quaternion oRotation)
		{
			float t = Mathf.InverseLerp(from.Frame, to.Frame, frame);
			oPosition = Vector3.Lerp(from.Position, to.Position, t);
			oRotation = Quaternion.Lerp(from.Rotation, to.Rotation, t);
		}
	}

	public string Name = string.Empty;

	private LazyLoad<SampleFrameList> _sampleList;

	private Queue<TrailInstance> _unusedTrailQueue = new Queue<TrailInstance>();

	public SampleFrameList SampleList => _sampleList.Value;

	public TrailInstance CreateTrailInstance(int beginFrame, int endFrame)
	{
		if (SampleList.Count < 2 || beginFrame == endFrame)
		{
			return null;
		}
		TrailInstance unusedTrailData = GetUnusedTrailData();
		int i;
		for (i = 0; i < SampleList.Count - 1; i++)
		{
			SampleFrame sampleFrame = SampleList.Get(i);
			SampleFrame sampleFrame2 = SampleList.Get(i + 1);
			if (beginFrame <= sampleFrame.Frame)
			{
				unusedTrailData.AddSampleFrame(sampleFrame);
				break;
			}
			if (sampleFrame.Frame < beginFrame && beginFrame < sampleFrame2.Frame)
			{
				unusedTrailData.AddSampleFrame(SampleFrame.Lerp(sampleFrame, sampleFrame2, beginFrame));
				break;
			}
			_ = sampleFrame2.Frame;
		}
		for (; i < SampleList.Count - 1; i++)
		{
			SampleFrame sampleFrame3 = SampleList.Get(i);
			SampleFrame sampleFrame4 = SampleList.Get(i + 1);
			if (endFrame <= sampleFrame3.Frame)
			{
				unusedTrailData.AddSampleFrame(sampleFrame3);
				break;
			}
			if (sampleFrame3.Frame < endFrame && endFrame < sampleFrame4.Frame)
			{
				unusedTrailData.AddSampleFrame(SampleFrame.Lerp(sampleFrame3, sampleFrame4, endFrame));
				break;
			}
			if (endFrame == sampleFrame4.Frame)
			{
				unusedTrailData.AddSampleFrame(sampleFrame4);
				break;
			}
			if (sampleFrame4.Frame < endFrame)
			{
				unusedTrailData.AddSampleFrame(sampleFrame4);
			}
		}
		if (unusedTrailData.SampleBuffer.Count < 2)
		{
			ReleaseTrailData(unusedTrailData);
			return null;
		}
		return unusedTrailData;
	}

	public void ReleaseTrailData(TrailInstance trailData)
	{
		trailData.Clear();
		_unusedTrailQueue.Enqueue(trailData);
	}

	public void DestroyOnSceneChanged()
	{
		while (_unusedTrailQueue.Count > 0)
		{
			_unusedTrailQueue.Dequeue()?.Release();
		}
	}

	private TrailInstance GetUnusedTrailData()
	{
		if (_unusedTrailQueue.Count > 0)
		{
			return _unusedTrailQueue.Dequeue();
		}
		return CreateNewTrailData();
	}

	private TrailInstance CreateNewTrailData()
	{
		return new TrailInstance(SampleList.Count);
	}

	public SampleFrame CalcSampleFrame(int frame)
	{
		if (!SampleList.GetNeighbor(out var o_minT, out var o_maxT, (SampleFrame curFrame) => curFrame.Frame.CompareTo(frame)) || o_minT.Frame == o_maxT.Frame)
		{
			return o_minT;
		}
		return SampleFrame.Lerp(o_minT, o_maxT, frame);
	}

	public Vector3 CalculateTangent(int index, Vector3 offset)
	{
		SampleFrame sampleFrame = SampleList.Get(index);
		SampleFrame sampleFrame2 = (SampleList.IsValidIndex(index - 1) ? SampleList.Get(index - 1) : sampleFrame);
		SampleFrame sampleFrame3 = (SampleList.IsValidIndex(index + 1) ? SampleList.Get(index + 1) : sampleFrame);
		Vector3 vector = sampleFrame2.Position + sampleFrame2.Rotation * offset;
		Vector3 vector2 = sampleFrame.Position + sampleFrame.Rotation * offset;
		Vector3 vector3 = sampleFrame3.Position + sampleFrame3.Rotation * offset;
		int num = sampleFrame.Frame - sampleFrame2.Frame;
		int num2 = sampleFrame3.Frame - sampleFrame.Frame;
		Vector3 vector4 = vector2 - vector;
		Vector3 vector5 = vector3 - vector2;
		return (vector4 * num2 + vector5 * num).normalized;
	}

	bool ILoadJson.Load(JsonRead json)
	{
		Name = json["Name"].GETSTRING(Name);
		_sampleList.SetJsonRead(json["SampleList"]);
		return true;
	}
}
