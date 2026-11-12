using System;
using System.Collections.Generic;
using NGameAsset.NAsyncLoad;
using NGameAsset.NEffect;
using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature;

public class OpenData : ICreatureOpenData, IAnimationOpenData, IAsyncLoadBase
{
	private AssetDataAsync<GameObject, ModelData> _modelAsync;

	private AssetDataAsync<Material, ModelData> _modelHairReplacementAsync;

	private AssetDataAsync<GameObject, HairData> _hairAsync;

	private AssetDataAsync<Material, HairData> _hairMaterialAsync;

	private List<AssetDataAsync<Mesh, FacialModelData>> _facialModelAsyncList;

	private List<AnimationLoadData> _animationLoadList;

	private List<SelfEffectData> _selfEffectList;

	private List<WeaponInstance.OpenData> _weaponOpenList;

	private List<CustomizeItemInstance.OpenData> _customizeItemOpenList;

	public Builder Builder { get; private set; }

	public CreatureStock Stock { get; private set; }

	string IAsyncLoadBase.KeyDesc => Stock.Code;

	bool IAsyncLoadBase.IsDone
	{
		get
		{
			if (!_modelAsync.ISDONE())
			{
				return false;
			}
			if (!_modelHairReplacementAsync.ISDONE())
			{
				return false;
			}
			if (!_hairAsync.ISDONE())
			{
				return false;
			}
			if (!_hairMaterialAsync.ISDONE())
			{
				return false;
			}
			if (!GameAsset_AsyncLoad_ExtensionMethod.AsyncIsDone(_facialModelAsyncList))
			{
				return false;
			}
			if (!AnimationLoadData.AsyncIsDone(_animationLoadList))
			{
				return false;
			}
			if (!GameAsset_AsyncLoad_ExtensionMethod.AsyncIsDone(_weaponOpenList))
			{
				return false;
			}
			return true;
		}
	}

	bool IAsyncLoadBase.IsValid
	{
		get
		{
			if (!_modelAsync.ISVALID())
			{
				return false;
			}
			if (_modelHairReplacementAsync != null && !_modelHairReplacementAsync.ISVALID())
			{
				return false;
			}
			if (_hairAsync != null && !_hairAsync.ISVALID())
			{
				return false;
			}
			if (_hairMaterialAsync != null && !_hairMaterialAsync.ISVALID())
			{
				return false;
			}
			if (!GameAsset_AsyncLoad_ExtensionMethod.AsyncIsValid(_facialModelAsyncList))
			{
				return false;
			}
			if (!AnimationLoadData.AsyncIsValid(_animationLoadList))
			{
				return false;
			}
			if (!GameAsset_AsyncLoad_ExtensionMethod.AsyncIsValid(_weaponOpenList))
			{
				return false;
			}
			return true;
		}
	}

	string IAsyncLoadBase.LoadMessage => string.Empty;

	CreatureStock ICreatureOpenData.GetStock()
	{
		return Stock;
	}

	CreatureBaseData ICreatureOpenData.GetBaseData()
	{
		return Builder.CreatureBase;
	}

	List<MotionSequenceData> ICreatureOpenData.GetMotionSequenceList()
	{
		return Builder.MotionSequenceList;
	}

	IAnimationBlendData ICreatureOpenData.GetAnimationBlend()
	{
		return Builder.AnimationBlend;
	}

	IModelData ICreatureOpenData.GetModelData()
	{
		return _modelAsync.Data;
	}

	GameObject ICreatureOpenData.GetModelAsset()
	{
		return _modelAsync.Asset;
	}

	Material ICreatureOpenData.GetModelHairReplacementAsset()
	{
		if (_modelHairReplacementAsync != null)
		{
			return _modelHairReplacementAsync.Asset;
		}
		return null;
	}

	IHairData ICreatureOpenData.GetHairData()
	{
		if (_hairAsync != null)
		{
			return _hairAsync.Data;
		}
		return null;
	}

	GameObject ICreatureOpenData.GetHairAsset()
	{
		if (_hairAsync != null)
		{
			return _hairAsync.Asset;
		}
		return null;
	}

	Material ICreatureOpenData.GetHairMaterialAsset()
	{
		if (_hairMaterialAsync != null)
		{
			return _hairMaterialAsync.Asset;
		}
		return null;
	}

	int ICreatureOpenData.GetFacialModelCount()
	{
		return __Extension_Method_ICollection__.COUNT(_facialModelAsyncList);
	}

	IFacialModelData ICreatureOpenData.GetFacialModelData(int index)
	{
		return _facialModelAsyncList[index].Data;
	}

	Mesh ICreatureOpenData.GetFacialModelMesh(int index)
	{
		return _facialModelAsyncList[index].Asset;
	}

	int IAnimationOpenData.GetAnimationCount()
	{
		return __Extension_Method_ICollection__.COUNT(_animationLoadList);
	}

	IAnimationData IAnimationOpenData.GetAnimationData(int index)
	{
		return _animationLoadList[index].Data;
	}

	IAsyncLoad<AnimationClip> IAnimationOpenData.GetAnimationClip(int index)
	{
		return _animationLoadList[index].ClipAsync;
	}

	bool IAnimationOpenData.IsAnimationClipLoaded(int index)
	{
		return _animationLoadList[index].IsLoaded();
	}

	int ICreatureOpenData.GetSelfEffectCount()
	{
		return __Extension_Method_ICollection__.COUNT(_selfEffectList);
	}

	SelfEffectData ICreatureOpenData.GetSelfEffectData(int index)
	{
		return _selfEffectList[index];
	}

	int ICreatureOpenData.GetPivotCount()
	{
		return __Extension_Method_ICollection__.COUNT(Builder.PivotList);
	}

	IPivotData ICreatureOpenData.GetPivotData(int index)
	{
		return Builder.PivotList[index];
	}

	int ICreatureOpenData.GetSubCreatureCount()
	{
		return Builder.SubCreatureList.COUNT();
	}

	ISubCreatureData ICreatureOpenData.GetSubCreatureData(int index)
	{
		return Builder.SubCreatureList[index];
	}

	int ICreatureOpenData.GetWeaponCount()
	{
		return __Extension_Method_ICollection__.COUNT(_weaponOpenList);
	}

	WeaponInstance.IOpenData ICreatureOpenData.GetWeaponOpenData(int index)
	{
		return _weaponOpenList[index];
	}

	int ICreatureOpenData.GetCustomizeItemCount()
	{
		return __Extension_Method_ICollection__.COUNT(_customizeItemOpenList);
	}

	CustomizeItemInstance.OpenData ICreatureOpenData.GetCustomizeItemInfo(int index)
	{
		return _customizeItemOpenList[index];
	}

	public bool Open(Builder builder, CreatureStock stock)
	{
		ModelData modelData = ModelData.SelectDataFromList(stock.Star, stock.BodyPrefix, builder.ModelList);
		if (modelData == null)
		{
			return false;
		}
		Builder = builder;
		Stock = stock;
		_modelAsync = modelData.CreateAssetAsync();
		_modelHairReplacementAsync = modelData.CreateHairReplacementAssetAsync();
		HairData hairData = HairData.SelectDataFromList(stock.HairPrefix, modelData.DefaultHairName, builder.HairList);
		if (hairData != null)
		{
			_hairAsync = hairData.CreateAssetAsync();
			_hairMaterialAsync = hairData.CreateHairMaterialAssetAsync();
		}
		bool flag = false;
		_animationLoadList = new List<AnimationLoadData>();
		List<AnimationData> list = builder.CreateAnimationList(modelData, stock.LoadLevel);
		list.Sort(delegate(AnimationData lhs, AnimationData rhs)
		{
			if (lhs.Key.MotionType < rhs.Key.MotionType)
			{
				return -1;
			}
			return (lhs.Key.MotionType > rhs.Key.MotionType) ? 1 : lhs.Key.Name.CompareTo(rhs.Key.Name);
		});
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
		{
			AnimationData animationData = list[num];
			bool flag2 = stock.NeedPreload(animationData);
			if (flag2)
			{
				flag = true;
			}
			AnimationLoadData item = new AnimationLoadData(animationData, flag2);
			_animationLoadList.Add(item);
		}
		if (!flag && !__Extension_Method_ICollection__.IsNullOrEmpty(_animationLoadList))
		{
			_animationLoadList[0].LoadClip();
		}
		if (stock.LoadLevel == CreatureStock.ELoadLevel.IncludeFacial)
		{
			_facialModelAsyncList = builder.CreateFacialModelAsyncList(modelData);
		}
		_selfEffectList = builder.CreateSelfEffectList(modelData);
		for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(builder.WeaponList); num2++)
		{
			WeaponInstance.OpenData openData = builder.WeaponList[num2].CreateOpenData(stock);
			if (openData != null)
			{
				if (_weaponOpenList == null)
				{
					_weaponOpenList = new List<WeaponInstance.OpenData>();
				}
				_weaponOpenList.Add(openData);
			}
		}
		CreateCustomizeItemAsyncOpenList();
		return true;
	}

	private void CreateCustomizeItemAsyncOpenList()
	{
		Dictionary<NShared.AccessoryPartType, CustomizeStock>.ValueCollection valueCollection = Stock.CustomizeStockValues();
		if (valueCollection == null)
		{
			return;
		}
		foreach (CustomizeStock item in valueCollection)
		{
			NGameAsset.NEffect.Builder builder = SingletonAuto<NGameAsset.NEffect.BuilderContainer>.instance.Get(item.Code);
			if (builder == null)
			{
				VespaDebug.LogError($"effectBuilder == null  Code : ({item.Code})");
				continue;
			}
			NGameAsset.NEffect.OpenData openData = builder.CreateOpenData();
			if (openData != null)
			{
				if (_customizeItemOpenList == null)
				{
					_customizeItemOpenList = new List<CustomizeItemInstance.OpenData>();
				}
				CustomizeItemInstance.OpenData openData2 = new CustomizeItemInstance.OpenData();
				openData2.Open(openData, item);
				_customizeItemOpenList.Add(openData2);
			}
		}
	}

	public void LoadFacialModelAsync()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_facialModelAsyncList) && Builder != null && _modelAsync != null)
		{
			_facialModelAsyncList = Builder.CreateFacialModelAsyncList(_modelAsync.Data);
		}
	}

	public void Close()
	{
		if (_modelAsync != null)
		{
			_modelAsync.Close();
			_modelAsync = null;
		}
		if (_animationLoadList != null)
		{
			for (int i = 0; i < _animationLoadList.Count; i++)
			{
				_animationLoadList[i].Close();
			}
			_animationLoadList.Clear();
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_facialModelAsyncList))
		{
			for (int j = 0; j < _facialModelAsyncList.Count; j++)
			{
				_facialModelAsyncList[j].Close();
			}
			_facialModelAsyncList.Clear();
		}
		if (_weaponOpenList != null)
		{
			for (int k = 0; k < _weaponOpenList.Count; k++)
			{
				_weaponOpenList[k].Close();
			}
			_weaponOpenList.Clear();
		}
		if (_modelHairReplacementAsync != null)
		{
			_modelHairReplacementAsync.Close();
			_modelHairReplacementAsync = null;
		}
		if (_hairAsync != null)
		{
			_hairAsync.Close();
			_hairAsync = null;
		}
		if (_hairMaterialAsync != null)
		{
			_hairMaterialAsync.Close();
			_hairMaterialAsync = null;
		}
		CloseAndClearCustomizeItemOpenList();
	}

	private void CloseAndClearCustomizeItemOpenList()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_customizeItemOpenList); i++)
		{
			_customizeItemOpenList[i].Close();
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_customizeItemOpenList);
	}

	public void ChangeCustomizeItemInfo(CustomizeStock itemStock)
	{
		CreatureStock stock = Stock;
		stock.SetCustomizeStock(itemStock);
		Stock = stock;
		CloseAndClearCustomizeItemOpenList();
		CreateCustomizeItemAsyncOpenList();
	}

	public void ChangeHairPrefix(string hairPrefix)
	{
		CreatureStock stock = Stock;
		stock.HairPrefix = hairPrefix;
		Stock = stock;
		HairData hairData = HairData.SelectDataFromList(stock.HairPrefix, _modelAsync.Data.DefaultHairName, Builder.HairList);
		if (hairData != null)
		{
			_hairAsync = hairData.CreateAssetAsync();
			_hairMaterialAsync = hairData.CreateHairMaterialAssetAsync();
		}
	}

	public void ChangeWeaponPrefix(WeaponStock weaponStock)
	{
		CreatureStock stock = Stock;
		stock.WeaponStock = weaponStock;
		Stock = stock;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_weaponOpenList); i++)
		{
			_weaponOpenList[i].Close();
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_weaponOpenList);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(Builder.WeaponList); j++)
		{
			WeaponInstance.OpenData openData = Builder.WeaponList[j].CreateOpenData(stock);
			if (openData != null)
			{
				if (_weaponOpenList == null)
				{
					_weaponOpenList = new List<WeaponInstance.OpenData>();
				}
				_weaponOpenList.Add(openData);
			}
		}
	}

	public void LoadExtraAnimation(DefineCreature.EMotion motionType)
	{
		List<AnimationKeyData> keyList = null;
		Builder.AnimationList.ForEach(delegate(AnimationData data)
		{
			if (data.Key.MotionType == motionType)
			{
				if (keyList == null)
				{
					keyList = new List<AnimationKeyData>();
				}
				keyList.Add(data.Key);
			}
		});
		LoadExtraAnimation(keyList);
	}

	public void LoadExtraAnimation(List<AnimationKeyData> keyList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(keyList); i++)
		{
			AnimationKeyData animationKeyData = keyList[i];
			bool flag = false;
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_animationLoadList); j++)
			{
				AnimationLoadData animationLoadData = _animationLoadList[j];
				AnimationKeyData key = animationLoadData.Data.Key;
				if (AnimationKeyData.Comparer.Default.Equals(animationKeyData, key))
				{
					flag = true;
					animationLoadData.LoadClip();
					break;
				}
			}
			if (!flag)
			{
				AnimationData animationData = Builder.GetAnimationData(_modelAsync.Data, animationKeyData);
				if (animationData == null)
				{
					VespaDebug.LogError("Not Exist AnimationData: " + animationKeyData.PlayName);
					continue;
				}
				AnimationLoadData item = new AnimationLoadData(animationData, preload: true);
				_animationLoadList.Add(item);
			}
		}
	}

	public AsyncManager.IEntry CreateAsyncEntry(Action onComplete)
	{
		return new AsyncManager.SingleEntry
		{
			AsyncLoad = this,
			OnComplete = onComplete
		};
	}

	public IAsyncLoadBase GetInvalidAsset()
	{
		if (!_modelAsync.ISVALID())
		{
			return _modelAsync;
		}
		if (_modelHairReplacementAsync != null && !_modelHairReplacementAsync.ISVALID())
		{
			return _modelHairReplacementAsync;
		}
		if (_hairAsync != null && !_hairAsync.ISVALID())
		{
			return _hairAsync;
		}
		if (_hairMaterialAsync != null && !_hairMaterialAsync.ISVALID())
		{
			return _hairMaterialAsync;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_facialModelAsyncList))
		{
			for (int i = 0; i < _facialModelAsyncList.Count; i++)
			{
				AssetDataAsync<Mesh, FacialModelData> assetDataAsync = _facialModelAsyncList[i];
				if (!assetDataAsync.ISVALID())
				{
					return assetDataAsync;
				}
			}
		}
		if (_weaponOpenList != null)
		{
			for (int j = 0; j < _weaponOpenList.Count; j++)
			{
				IAsyncLoadBase invalidAsset = _weaponOpenList[j].GetInvalidAsset();
				if (invalidAsset != null)
				{
					return invalidAsset;
				}
			}
		}
		return null;
	}
}
