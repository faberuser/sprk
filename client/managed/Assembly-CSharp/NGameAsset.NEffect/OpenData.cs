using NGameAsset.NAsyncLoad;
using NGameAsset.NSound;
using NVespa;
using UnityEngine;

namespace NGameAsset.NEffect;

public class OpenData : IEffectOpenData, IAsyncLoadBase
{
	private Builder _builder;

	private PoolingList<AssetDataAsync<GameObject, ModelData>> _modelAsyncList;

	private PoolingList<AssetDataAsync<GameObject, ParticleData>> _particleAsyncList;

	private PoolingList<AssetDataAsync<GameObject, TrailData>> _trailAsyncList;

	private PoolingList<IAsyncLoad<AudioClip>> _audioClipAsyncList;

	string IAsyncLoadBase.KeyDesc => _builder.Code;

	bool IAsyncLoadBase.IsDone
	{
		get
		{
			if (AsyncIsDone(ref _modelAsyncList) && AsyncIsDone(ref _particleAsyncList) && AsyncIsDone(ref _trailAsyncList))
			{
				return AsyncIsDone(ref _audioClipAsyncList);
			}
			return false;
		}
	}

	bool IAsyncLoadBase.IsValid
	{
		get
		{
			if (AsyncIsValid(ref _modelAsyncList) && AsyncIsValid(ref _particleAsyncList) && AsyncIsValid(ref _trailAsyncList))
			{
				return AsyncIsValid(ref _audioClipAsyncList);
			}
			return false;
		}
	}

	string IAsyncLoadBase.LoadMessage => string.Empty;

	public void Open(Builder builder)
	{
		_builder = builder;
		_builder.CreateModelAsyncList(ref _modelAsyncList);
		_builder.CreateParticleAsyncList(ref _particleAsyncList);
		_builder.CreateTrailAsyncList(ref _trailAsyncList);
		_builder.CreateAudioClipAsyncList(ref _audioClipAsyncList);
	}

	public void Close()
	{
		for (int i = 0; i < _modelAsyncList.Count; i++)
		{
			_modelAsyncList[i].Close();
		}
		_modelAsyncList.Clear();
		for (int j = 0; j < _particleAsyncList.Count; j++)
		{
			_particleAsyncList[j].Close();
		}
		_particleAsyncList.Clear();
		for (int k = 0; k < _trailAsyncList.Count; k++)
		{
			_trailAsyncList[k].Close();
		}
		_trailAsyncList.Clear();
		for (int l = 0; l < _audioClipAsyncList.Count; l++)
		{
			AudioClipContainer.ReleaseAudioClip(_audioClipAsyncList[l]);
		}
		_audioClipAsyncList.Clear();
	}

	string IEffectOpenData.GetCode()
	{
		return _builder.Code;
	}

	BaseData IEffectOpenData.GetBaseData()
	{
		return _builder.Base;
	}

	int IEffectOpenData.GetModelCount()
	{
		return _modelAsyncList.COUNT();
	}

	IModelData IEffectOpenData.GetModelData(int index)
	{
		if (!_modelAsyncList.IsValidIndex(index))
		{
			return null;
		}
		return _modelAsyncList[index].Data;
	}

	IAssetFactory<GameObject> IEffectOpenData.GetModelFactory(int index)
	{
		if (!_modelAsyncList.IsValidIndex(index))
		{
			return null;
		}
		return _modelAsyncList[index].Factory;
	}

	int IEffectOpenData.GetParticleCount()
	{
		return _particleAsyncList.COUNT();
	}

	IParticleData IEffectOpenData.GetParticleData(int index)
	{
		if (!_particleAsyncList.IsValidIndex(index))
		{
			return null;
		}
		return _particleAsyncList[index].Data;
	}

	IAssetFactory<GameObject> IEffectOpenData.GetParticleFactory(int index)
	{
		if (!_particleAsyncList.IsValidIndex(index))
		{
			return null;
		}
		return _particleAsyncList[index].Factory;
	}

	int IEffectOpenData.GetTrailCount()
	{
		return _trailAsyncList.COUNT();
	}

	ITrailData IEffectOpenData.GetTrailData(int index)
	{
		if (!_trailAsyncList.IsValidIndex(index))
		{
			return null;
		}
		return _trailAsyncList[index].Data;
	}

	IAssetFactory<GameObject> IEffectOpenData.GetTrailFactory(int index)
	{
		if (!_trailAsyncList.IsValidIndex(index))
		{
			return null;
		}
		return _trailAsyncList[index].Factory;
	}

	int IEffectOpenData.GetSoundFxCount()
	{
		return _builder.GetSoundFxCount();
	}

	SoundFxData IEffectOpenData.GetSoundFxData(int index)
	{
		return _builder.GetSoundFxData(index);
	}

	public IAsyncLoadBase GetInvalidAsset()
	{
		for (int i = 0; i < _modelAsyncList.Count; i++)
		{
			AssetDataAsync<GameObject, ModelData> assetDataAsync = _modelAsyncList[i];
			if (!assetDataAsync.ISVALID())
			{
				return assetDataAsync;
			}
		}
		for (int j = 0; j < _particleAsyncList.Count; j++)
		{
			AssetDataAsync<GameObject, ParticleData> assetDataAsync2 = _particleAsyncList[j];
			if (!assetDataAsync2.ISVALID())
			{
				return assetDataAsync2;
			}
		}
		for (int k = 0; k < _trailAsyncList.Count; k++)
		{
			AssetDataAsync<GameObject, TrailData> assetDataAsync3 = _trailAsyncList[k];
			if (!assetDataAsync3.ISVALID())
			{
				return assetDataAsync3;
			}
		}
		for (int l = 0; l < _audioClipAsyncList.Count; l++)
		{
			IAsyncLoad<AudioClip> asyncLoad = _audioClipAsyncList[l];
			if (!asyncLoad.ISVALID())
			{
				return asyncLoad;
			}
		}
		return null;
	}

	public static bool AsyncIsDone<T>(ref PoolingList<T> This) where T : IAsyncLoadBase
	{
		for (int i = 0; i < This.Count; i++)
		{
			if (!This[i].IsDone)
			{
				return false;
			}
		}
		return true;
	}

	public static bool AsyncIsValid<T>(ref PoolingList<T> This) where T : IAsyncLoadBase
	{
		for (int i = 0; i < This.Count; i++)
		{
			if (!This[i].IsValid)
			{
				return false;
			}
		}
		return true;
	}
}
