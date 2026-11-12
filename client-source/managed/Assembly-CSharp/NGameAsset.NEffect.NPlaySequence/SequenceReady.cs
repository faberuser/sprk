using NVespa;

namespace NGameAsset.NEffect.NPlaySequence;

public class SequenceReady : SmallPooling<SequenceReady>, ISmallPoolingItem, ISequence
{
	private ModelInstance _modelInstance;

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
	}

	public void Open(ModelInstance modelInstance)
	{
		_modelInstance = modelInstance;
		_elapsedTime = 0f;
		_isEnd = false;
	}

	PlaySequenceData ISequence.GetData()
	{
		return null;
	}

	float ISequence.Update(float deltaTime)
	{
		if (_modelInstance.PlayState == ModelInstance.EPlayState.Ending)
		{
			_isEnd = true;
			return deltaTime;
		}
		float beginTime = _modelInstance.Data.GetCommonData().BeginTime;
		if (_elapsedTime + deltaTime < beginTime)
		{
			_elapsedTime += deltaTime;
			return 0f;
		}
		float result = deltaTime - (beginTime - _elapsedTime);
		_elapsedTime = beginTime;
		_isEnd = true;
		_modelInstance.IsVisible = true;
		_modelInstance.UpdateVisible();
		return result;
	}

	bool ISequence.IsEnd()
	{
		return _isEnd;
	}

	void ISequence.ReleaseSelf()
	{
		SmallPooling<SequenceReady>.ReleaseInstance(this);
	}
}
