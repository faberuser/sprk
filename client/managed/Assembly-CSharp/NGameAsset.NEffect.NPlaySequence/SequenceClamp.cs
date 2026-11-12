using NVespa;
using UnityEngine;

namespace NGameAsset.NEffect.NPlaySequence;

public class SequenceClamp : SmallPooling<SequenceClamp>, ISmallPoolingItem, ISequence
{
	private ModelInstance _modelInstance;

	private PlaySequenceData _data;

	private float _elapsedTime;

	private bool _isEnd;

	void ISmallPoolingItem.OnCreateNew()
	{
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
	}

	void ISmallPoolingItem.OnRelease()
	{
		_modelInstance = null;
		_data = null;
	}

	public void Open(ModelInstance modelInstance, PlaySequenceData sequenceData)
	{
		_modelInstance = modelInstance;
		_data = sequenceData;
		_elapsedTime = 0f;
		_isEnd = false;
	}

	PlaySequenceData ISequence.GetData()
	{
		return _data;
	}

	float ISequence.Update(float deltaTime)
	{
		if (_modelInstance.PlayState == ModelInstance.EPlayState.Ending)
		{
			_isEnd = true;
			return deltaTime;
		}
		if (_elapsedTime + deltaTime < _data.TimeLength)
		{
			_elapsedTime += deltaTime;
			float frame = Mathf.Lerp(_data.ClipStartFrame, _data.ClipEndFrame, _elapsedTime / Vespa_Float_Extension_Method.NoneZero(_data.TimeLength));
			_modelInstance.SetClipFrame(frame, forceUpdate: false);
			return 0f;
		}
		float result = deltaTime - (_data.TimeLength - _elapsedTime);
		_elapsedTime = _data.TimeLength;
		_isEnd = true;
		_modelInstance.SetClipFrame(_data.ClipEndFrame, forceUpdate: false);
		return result;
	}

	bool ISequence.IsEnd()
	{
		return _isEnd;
	}

	void ISequence.ReleaseSelf()
	{
		SmallPooling<SequenceClamp>.ReleaseInstance(this);
	}
}
