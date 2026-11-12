using NVespa;

namespace NGameAsset.NEffect.NPlaySequence;

public class SequenceNone : SmallPooling<SequenceNone>, ISmallPoolingItem, ISequence
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
			return 0f;
		}
		float result = deltaTime - (_data.TimeLength - _elapsedTime);
		_elapsedTime = _data.TimeLength;
		_isEnd = true;
		return result;
	}

	bool ISequence.IsEnd()
	{
		return _isEnd;
	}

	void ISequence.ReleaseSelf()
	{
		SmallPooling<SequenceNone>.ReleaseInstance(this);
	}
}
