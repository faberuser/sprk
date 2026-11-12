using System.Collections.Generic;
using NGameAsset.NAsyncLoad;
using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature;

public class WeaponInstance
{
	public interface IOpenData : IAnimationOpenData
	{
		IWeaponData GetWeaponData();

		IModelData GetModelData();

		GameObject GetModelAsset();

		int GetWeaponEffectDataCount();

		IWeaponEffectData GetWeaponEffectData(int index);
	}

	public class OpenData : IOpenData, IAnimationOpenData, IAsyncLoadBase
	{
		public WeaponData Data;

		public AssetDataAsync<GameObject, ModelData> ModelAsync;

		public List<IWeaponEffectData> WeaponEffectDataList;

		public List<AnimationLoadData> AnimationLoadList;

		string IAsyncLoadBase.KeyDesc => Data.Name;

		bool IAsyncLoadBase.IsDone
		{
			get
			{
				if (ModelAsync.ISDONE())
				{
					return AnimationLoadData.AsyncIsDone(AnimationLoadList);
				}
				return false;
			}
		}

		bool IAsyncLoadBase.IsValid
		{
			get
			{
				if (ModelAsync.ISVALID())
				{
					return AnimationLoadData.AsyncIsValid(AnimationLoadList);
				}
				return false;
			}
		}

		string IAsyncLoadBase.LoadMessage => string.Empty;

		IWeaponData IOpenData.GetWeaponData()
		{
			return Data;
		}

		IModelData IOpenData.GetModelData()
		{
			return ModelAsync.Data;
		}

		GameObject IOpenData.GetModelAsset()
		{
			return ModelAsync.Asset;
		}

		int IAnimationOpenData.GetAnimationCount()
		{
			return __Extension_Method_ICollection__.COUNT(AnimationLoadList);
		}

		IAnimationData IAnimationOpenData.GetAnimationData(int index)
		{
			return AnimationLoadList[index].Data;
		}

		IAsyncLoad<AnimationClip> IAnimationOpenData.GetAnimationClip(int index)
		{
			return AnimationLoadList[index].ClipAsync;
		}

		bool IAnimationOpenData.IsAnimationClipLoaded(int index)
		{
			return AnimationLoadList[index].IsLoaded();
		}

		int IOpenData.GetWeaponEffectDataCount()
		{
			return __Extension_Method_ICollection__.COUNT(WeaponEffectDataList);
		}

		IWeaponEffectData IOpenData.GetWeaponEffectData(int index)
		{
			return WeaponEffectDataList[index];
		}

		public void Close()
		{
			if (ModelAsync != null)
			{
				ModelAsync.Close();
				ModelAsync = null;
			}
			if (AnimationLoadList != null)
			{
				for (int i = 0; i < AnimationLoadList.Count; i++)
				{
					AnimationLoadList[i].Close();
				}
				AnimationLoadList.Clear();
			}
			if (WeaponEffectDataList != null)
			{
				WeaponEffectDataList.Clear();
			}
		}

		public IAsyncLoadBase GetInvalidAsset()
		{
			if (!ModelAsync.ISVALID())
			{
				return ModelAsync;
			}
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(AnimationLoadList); i++)
			{
				AnimationLoadData animationLoadData = AnimationLoadList[i];
				if (animationLoadData.IsLoadStart && !animationLoadData.IsLoaded())
				{
					return animationLoadData.ClipAsync;
				}
			}
			return null;
		}
	}

	private CreatureInstance _owner;

	private RenderCache _renderCache;

	private IAnimationData _defaultAnimationData;

	private List<SelfEffectInstance> _selfEffectInstanceList;

	private List<WeaponEffectInstance> _weaponEffectInstanceList;

	private AnimationInstance _animation;

	public IOpenData WeaponOpenData { get; private set; }

	public IWeaponData Data { get; private set; }

	public GameObject ModelGo { get; private set; }

	public WeaponInstance(CreatureInstance owner)
	{
		_owner = owner;
	}

	public void ChainUpdate()
	{
		ChainUpdateSelfEffectList();
		ChainUpdateWeaponEffectList();
	}

	private void ChainUpdateSelfEffectList()
	{
		if (_selfEffectInstanceList == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_selfEffectInstanceList); i++)
		{
			SelfEffectInstance selfEffectInstance = _selfEffectInstanceList[i];
			if (selfEffectInstance != null)
			{
				selfEffectInstance.SetEnable(ModelGo.activeSelf);
				selfEffectInstance.CheckEnable();
				selfEffectInstance.ChainUpdate();
			}
		}
	}

	private void ChainUpdateWeaponEffectList()
	{
		if (_weaponEffectInstanceList == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_weaponEffectInstanceList); i++)
		{
			WeaponEffectInstance weaponEffectInstance = _weaponEffectInstanceList[i];
			if (weaponEffectInstance != null)
			{
				weaponEffectInstance.SetVisible(ModelGo.activeSelf);
				weaponEffectInstance.CheckCreateEffect(_owner.ExtraRenderFxController, _owner.LocalTimer);
				weaponEffectInstance.ChainUpdate();
			}
		}
	}

	public bool Open(IOpenData openData)
	{
		WeaponOpenData = openData;
		Data = openData.GetWeaponData();
		ModelGo = openData.GetModelData().InstantiateModel(openData.GetModelAsset());
		if (ModelGo == null)
		{
			return false;
		}
		_renderCache = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<RenderCache>(ModelGo);
		_renderCache.Initialize();
		OpenAnimation(openData);
		List<SelfEffectData> selfEffectList = openData.GetModelData().GetSelfEffectList();
		if (selfEffectList != null && selfEffectList.Count > 0)
		{
			OpenSelfEffect(openData);
		}
		OpenWeaponEffect(openData);
		Transform pivotSafe = Vespa_Common_Class_Extension_Method.Interface((ICreature)_owner).GetPivotSafe(Data.AttachPivot);
		Vespa_Unity_Transform_Extension_Method.ParentRelative(ModelGo.transform, pivotSafe);
		Vespa_Unity_Transform_Extension_Method.SetLayerRecursively(ModelGo.transform, pivotSafe);
		return true;
	}

	private void OpenSelfEffect(IOpenData data)
	{
		List<SelfEffectData> selfEffectList = data.GetModelData().GetSelfEffectList();
		if (__Extension_Method_ICollection__.COUNT(selfEffectList) != 0)
		{
			_selfEffectInstanceList = new List<SelfEffectInstance>();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(selfEffectList); i++)
			{
				SelfEffectInstance selfEffectInstance = new SelfEffectInstance();
				selfEffectInstance.Open(_owner, selfEffectList[i]);
				_selfEffectInstanceList.Add(selfEffectInstance);
			}
		}
	}

	private void OpenWeaponEffect(IOpenData data)
	{
		int weaponEffectDataCount = data.GetWeaponEffectDataCount();
		if (weaponEffectDataCount <= 0)
		{
			return;
		}
		for (int i = 0; i < weaponEffectDataCount; i++)
		{
			IWeaponEffectData weaponEffectData = data.GetWeaponEffectData(i);
			if (weaponEffectData == null)
			{
				continue;
			}
			WeaponEffectInstance weaponEffectInstance = new WeaponEffectInstance();
			if (!weaponEffectInstance.Open(this, weaponEffectData))
			{
				weaponEffectInstance.Close();
				continue;
			}
			if (_weaponEffectInstanceList == null)
			{
				_weaponEffectInstanceList = new List<WeaponEffectInstance>();
			}
			_weaponEffectInstanceList.Add(weaponEffectInstance);
		}
	}

	public void Close()
	{
		if (ModelGo != null)
		{
			AssetUpdateManager.PendingDestroyGo(ModelGo);
			ModelGo = null;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_selfEffectInstanceList); i++)
		{
			_selfEffectInstanceList[i].Close();
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_selfEffectInstanceList);
		if (_weaponEffectInstanceList != null)
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_weaponEffectInstanceList); j++)
			{
				_weaponEffectInstanceList[j]?.Close();
			}
			_weaponEffectInstanceList.Clear();
		}
	}

	public void Visit(IVisitor visitor)
	{
		_renderCache.Visit(visitor);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_selfEffectInstanceList); i++)
		{
			_selfEffectInstanceList[i].Visit(visitor);
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_weaponEffectInstanceList); j++)
		{
			_weaponEffectInstanceList[j]?.Visit(visitor);
		}
	}

	public void SetVisible(bool isVisible)
	{
		if (ModelGo.activeSelf != isVisible)
		{
			ModelGo.SetActive(isVisible);
		}
	}

	private void OpenAnimation(IOpenData openData)
	{
		if (openData.GetAnimationCount() == 0)
		{
			return;
		}
		Animation component = ModelGo.GetComponent<Animation>();
		if (component == null)
		{
			return;
		}
		component.playAutomatically = false;
		component.cullingType = AnimationCullingType.AlwaysAnimate;
		_animation = new AnimationInstance(component, openData);
		Dictionary<AnimationKeyData, IAnimationData> dictionary = new Dictionary<AnimationKeyData, IAnimationData>(AnimationKeyData.Comparer.Default);
		bool flag = false;
		for (int i = 0; i < openData.GetAnimationCount(); i++)
		{
			IAnimationData animationData = openData.GetAnimationData(i);
			if (animationData == null)
			{
				continue;
			}
			AnimationKeyData key = animationData.GetKey();
			if (!dictionary.ContainsKey(key))
			{
				if (!flag && openData.IsAnimationClipLoaded(i))
				{
					SetAnimation(key, 0f, 0f);
					flag = true;
				}
				dictionary.Add(key, animationData);
			}
		}
		_defaultAnimationData = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(dictionary, Data.DefaultAnimationKey, null);
	}

	public void SetAnimation(AnimationKeyData key, float frame, float weight)
	{
		if (_animation != null)
		{
			_animation.SetAnimation(key, frame, weight);
		}
	}
}
