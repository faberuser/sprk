using System.Collections.Generic;
using NGameAsset.NAsyncLoad;
using NGameAsset.NEffect;
using NVespa;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2;

public class OpenData : IAsyncLoadBase, IBattleFieldMapOpenData
{
	private Builder _builder;

	private BattleFieldMapStock _stock;

	private AssetDataAsync<GameObject, ModelData> _modelAsync;

	private List<NGameAsset.NEffect.OpenData> _effectFxOpenList;

	BattleFieldMapStock IBattleFieldMapOpenData.Stock => _stock;

	IModelData IBattleFieldMapOpenData.Model => _builder.Model;

	List<EffectFxData> IBattleFieldMapOpenData.EffectFxList
	{
		get
		{
			if (!String_Extension_Method.IsNullOrEmpty(_stock.Prefix))
			{
				for (int i = 0; i < _builder.PrefixEffectFxGroupList.Count; i++)
				{
					EffectFxGroupData effectFxGroupData = _builder.PrefixEffectFxGroupList[i];
					if (effectFxGroupData.Prefix == _stock.Prefix)
					{
						return effectFxGroupData.EffectFxList;
					}
				}
			}
			return _builder.EffectFxGroup.EffectFxList;
		}
	}

	string IAsyncLoadBase.KeyDesc => _builder.Code;

	bool IAsyncLoadBase.IsDone
	{
		get
		{
			if (_modelAsync.ISDONE())
			{
				return GameAsset_AsyncLoad_ExtensionMethod.AsyncIsDone(_effectFxOpenList);
			}
			return false;
		}
	}

	bool IAsyncLoadBase.IsValid
	{
		get
		{
			if (_modelAsync.ISVALID())
			{
				return GameAsset_AsyncLoad_ExtensionMethod.AsyncIsValid(_effectFxOpenList);
			}
			return false;
		}
	}

	string IAsyncLoadBase.LoadMessage => string.Empty;

	public void Open(Builder builder, BattleFieldMapStock stock)
	{
		_builder = builder;
		_stock = stock;
		_modelAsync = _builder.CreateModelAsync();
		List<EffectFxData> effectFxList = Vespa_Common_Class_Extension_Method.Interface((IBattleFieldMapOpenData)this).EffectFxList;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(effectFxList))
		{
			return;
		}
		_effectFxOpenList = new List<NGameAsset.NEffect.OpenData>();
		for (int i = 0; i < effectFxList.Count; i++)
		{
			EffectFxData effectFxData = effectFxList[i];
			NGameAsset.NEffect.Builder builder2 = SingletonAuto<NGameAsset.NEffect.BuilderContainer>.instance.Get(effectFxData.Code);
			if (builder2 == null)
			{
				break;
			}
			NGameAsset.NEffect.OpenData item = builder2.CreateOpenData();
			_effectFxOpenList.Add(item);
		}
	}

	public void Close()
	{
		if (_modelAsync != null)
		{
			_modelAsync.Close();
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_effectFxOpenList))
		{
			for (int i = 0; i < _effectFxOpenList.Count; i++)
			{
				_effectFxOpenList[i].Close();
			}
			_effectFxOpenList.Clear();
		}
	}

	public GameObject GetModelAsset()
	{
		if (_modelAsync == null)
		{
			return null;
		}
		return _modelAsync.Asset;
	}

	GameObject IBattleFieldMapOpenData.InstantiateModel()
	{
		if (!_modelAsync.ISVALID())
		{
			return null;
		}
		if (_modelAsync.Data == null)
		{
			return null;
		}
		return _modelAsync.Data.InstantiateModel(_modelAsync.Asset);
	}

	public IAsyncLoadBase GetInvalidAsset()
	{
		if (!_modelAsync.ISVALID())
		{
			return _modelAsync;
		}
		return null;
	}
}
