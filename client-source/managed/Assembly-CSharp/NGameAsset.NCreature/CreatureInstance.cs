using System;
using System.Collections.Generic;
using NGame2;
using NGameAsset.NCommon;
using NVespa;
using UnityEngine;

namespace NGameAsset.NCreature;

public class CreatureInstance : ICreature, IAnimationLoadMonitor, ExtraRenderFxController.IOwner
{
	private class EnableRenderVisitor : IGenericVisitor<RenderProxy>, IVisitor
	{
		public bool IsEnabled = true;

		void IGenericVisitor<RenderProxy>.Visit(RenderProxy proxy)
		{
			if (!(proxy.renderer == null))
			{
				proxy.renderer.enabled = IsEnabled;
			}
		}
	}

	private class PivotInstance
	{
		public Transform Transform;

		public IPivotData Data;
	}

	private class ReassignHairData
	{
		public SkinnedMeshRenderer Skinned;

		public string RootBonePath;

		public PoolingList<string> BonePathList;
	}

	public const float MinAlpha = 0.0019607844f;

	public GameObjectCache InstanceCache = new GameObjectCache();

	public GameObjectCache ModelCache = new GameObjectCache();

	private RenderCache _renderCache;

	private RenderGroupModifier2 _renderGroupModifier;

	private IncreaseDic<string, PivotInstance> _pivotDic = new IncreaseDic<string, PivotInstance>();

	private Dictionary<string, Transform> _transformCache = new Dictionary<string, Transform>();

	private AnimationInstance _animation;

	public IncreaseDic<AnimationKeyData, IAnimationData> AnimationDic = new IncreaseDic<AnimationKeyData, IAnimationData>(AnimationKeyData.Comparer.Default);

	private List<SubCreatureInstance> _subCreatureList;

	private List<WeaponInstance> _weaponList;

	private List<SelfEffectInstance> _selfEffectList;

	private FacialInstance _facial;

	private CircleShadowInstance _circleShadow;

	private List<CustomizeItemInstance> _customizeItemInstanceList;

	private Quaternion _rotationBlend;

	private List<ICreatureMaterialModifier> _mtrlModifierList;

	private EnableRenderVisitor _enableRenderVisitor = new EnableRenderVisitor();

	private NGameAsset.NCreature.ActiveCustomizeItemVisitor _activeCustomizeItemVisitor = new NGameAsset.NCreature.ActiveCustomizeItemVisitor();

	public string Code => OpenData.GetStock().Code;

	public ExtraRenderFxController ExtraRenderFxController { get; private set; }

	public ILocalTimer LocalTimer { get; private set; }

	public ICreatureOpenData OpenData { get; private set; }

	public IAnimationBlendData AnimationBlend => OpenData.GetAnimationBlend();

	public List<MotionSequenceData> MotionSequenceList => OpenData.GetMotionSequenceList();

	int ExtraRenderFxController.IOwner.Layer => InstanceCache.Layer;

	RenderGroupModifier2 ExtraRenderFxController.IOwner.RenderGroupModifier => _renderGroupModifier;

	public void ApplyTo(CreatureInstance other)
	{
		Vespa_Common_Class_Extension_Method.Interface((ICreature)other).SetParent(InstanceCache.Tm.parent);
		Vespa_Common_Class_Extension_Method.Interface((ICreature)other).SetTimer(LocalTimer);
		Vespa_Common_Class_Extension_Method.Interface((ICreature)other).SetShaderTransform(ExtraRenderFxController.ShaderTransform, ExtraRenderFxController.ShaderTransformPivot);
		Vespa_Common_Class_Extension_Method.Interface((ICreature)other).SetGrayscale(ExtraRenderFxController.GrayscaleEnabled);
		RenderGroupModifier2 renderGroupModifier = Vespa_Common_Class_Extension_Method.Interface((ICreature)other).GetRenderGroupModifier();
		_renderGroupModifier.CopyTo(renderGroupModifier);
		other.FlushRenderGroupModifier();
		Vespa_Common_Class_Extension_Method.Interface((ICreature)other).SetRotation(_rotationBlend, reset: true);
		_animation.ApplyTo(other);
	}

	private void ConstructHairBone(Transform hairTm, Transform bodyTm)
	{
		for (int num = hairTm.childCount - 1; num >= 0; num--)
		{
			Transform child = hairTm.GetChild(num);
			if (!(child == null))
			{
				Transform transform = bodyTm.Find(child.name);
				if (transform != null)
				{
					ConstructHairBone(child, transform);
				}
				else
				{
					child.parent = bodyTm;
				}
			}
		}
	}

	private void CustomizeHair()
	{
		if (ModelCache.Go == null)
		{
			return;
		}
		IHairData hairData = OpenData.GetHairData();
		if (hairData == null)
		{
			return;
		}
		GameObject gameObject = hairData.InstantiateModel(OpenData.GetHairAsset());
		SkinnedMeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		using (PoolingList<ReassignHairData> poolingList = default(PoolingList<ReassignHairData>))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(componentsInChildren); i++)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = componentsInChildren[i];
				if (!(skinnedMeshRenderer == null))
				{
					ReassignHairData reassignHairData = new ReassignHairData();
					reassignHairData.Skinned = skinnedMeshRenderer;
					reassignHairData.RootBonePath = Vespa_Unity_Transform_Extension_Method.GetTransformPath(skinnedMeshRenderer.rootBone, gameObject.transform);
					Transform[] bones = skinnedMeshRenderer.bones;
					for (int j = 0; j < __Extension_Method_ICollection__.COUNT(bones); j++)
					{
						Transform transform = bones[j];
						reassignHairData.BonePathList.Add(Vespa_Unity_Transform_Extension_Method.GetTransformPath(transform, gameObject.transform));
					}
					poolingList.Add(reassignHairData);
				}
			}
			ConstructHairBone(gameObject.transform, ModelCache.Go.transform);
			for (int k = 0; k < poolingList.Count; k++)
			{
				ReassignHairData reassignHairData2 = poolingList[k];
				reassignHairData2.Skinned.rootBone = ModelCache.Go.transform.Find(reassignHairData2.RootBonePath);
				Transform[] array = new Transform[reassignHairData2.BonePathList.Count];
				for (int l = 0; l < __Extension_Method_ICollection__.COUNT(array); l++)
				{
					array[l] = ModelCache.Go.transform.Find(reassignHairData2.BonePathList[l]);
				}
				reassignHairData2.Skinned.bones = array;
				reassignHairData2.BonePathList.Clear();
			}
		}
		VespaObject.PendingDestroyGo(gameObject);
	}

	public bool Open(ICreatureOpenData data)
	{
		OpenData = data;
		ModelCache.Go = OpenData.GetModelData().InstantiateModel(OpenData.GetModelAsset());
		if (ModelCache.Go == null)
		{
			return false;
		}
		Animation component = ModelCache.Go.GetComponent<Animation>();
		if (component == null)
		{
			return false;
		}
		CustomizeHair();
		_animation = new AnimationInstance(component, OpenData);
		InstanceCache.Go = new GameObject("Scale");
		InstanceCache.Tm.localScale = Vector3.one * OpenData.GetBaseData().Scale;
		ModelCache.Tm.SetParent(InstanceCache.Tm, worldPositionStays: false);
		bool flag = false;
		for (int i = 0; i < OpenData.GetAnimationCount(); i++)
		{
			IAnimationData animationData = OpenData.GetAnimationData(i);
			if (animationData == null)
			{
				continue;
			}
			AnimationKeyData key = animationData.GetKey();
			if (!AnimationDic.ContainsKey(key))
			{
				if (!flag && OpenData.IsAnimationClipLoaded(i))
				{
					_animation.SetAnimation(key, 0f, 0f);
					flag = true;
				}
				AnimationDic.Add(key, animationData);
			}
		}
		for (int j = 0; j < OpenData.GetPivotCount(); j++)
		{
			IPivotData pivotData = OpenData.GetPivotData(j);
			if (!String_Extension_Method.IsNullOrEmpty(pivotData.Name))
			{
				Transform transform = ModelCache.Tm.Find(pivotData.Path);
				if (!(transform == null))
				{
					PivotInstance pivotInstance = new PivotInstance();
					pivotInstance.Transform = transform;
					pivotInstance.Data = pivotData;
					_pivotDic.Add(pivotData.Name, pivotInstance);
				}
			}
		}
		Material modelHairReplacementAsset = OpenData.GetModelHairReplacementAsset();
		Material hairMaterialAsset = OpenData.GetHairMaterialAsset();
		RenderProxy.ReplaceMaterialData replaceMaterialData = null;
		if (modelHairReplacementAsset != null && hairMaterialAsset != null)
		{
			replaceMaterialData = new RenderProxy.ReplaceMaterialData();
			replaceMaterialData.From = modelHairReplacementAsset;
			replaceMaterialData.To = hairMaterialAsset;
		}
		_renderCache = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<RenderCache>(ModelCache.Go);
		_renderCache.Initialize(replaceMaterialData);
		ExtraRenderFxController = new ExtraRenderFxController(this);
		_renderGroupModifier = SmallPooling<RenderGroupModifier2>.CreateInstance();
		_renderGroupModifier.Open(useSoloTimeColor: true);
		OpenPivot(isWeaponPivot: false);
		OpenWeapon();
		OpenPivot(isWeaponPivot: true);
		OpenSubCreature();
		OpenSelfEffect();
		OpenCustomizeItem();
		_circleShadow = new CircleShadowInstance(this);
		if (OpenData.GetFacialModelCount() > 0)
		{
			_facial = new FacialInstance();
			if (!_facial.Open(this, OpenData, _renderCache))
			{
				_facial.Close();
				_facial = null;
			}
		}
		return true;
	}

	public void OpenFacialModel()
	{
		if (_facial == null && OpenData.GetFacialModelCount() > 0)
		{
			_facial = new FacialInstance();
			if (!_facial.Open(this, OpenData, _renderCache))
			{
				_facial.Close();
				_facial = null;
			}
		}
	}

	public void ReopenCustomizeItem()
	{
		List<CustomizeItemInstance.OpenData> list = new List<CustomizeItemInstance.OpenData>();
		List<CustomizeItemInstance> list2 = new List<CustomizeItemInstance>();
		for (int i = 0; i < OpenData.GetCustomizeItemCount(); i++)
		{
			CustomizeItemInstance.OpenData customizeItemInfo = OpenData.GetCustomizeItemInfo(i);
			CustomizeItemInstance customizeItemInstance = ((_customizeItemInstanceList == null) ? null : _customizeItemInstanceList.Find((CustomizeItemInstance ins) => ins.Stock.IsEqualItem(customizeItemInfo.Stock)));
			if (customizeItemInstance != null)
			{
				list2.Add(customizeItemInstance);
			}
			else
			{
				list.Add(customizeItemInfo);
			}
		}
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(_customizeItemInstanceList); num++)
		{
			if (!list2.Contains(_customizeItemInstanceList[num]))
			{
				_customizeItemInstanceList[num].Close();
				_customizeItemInstanceList.RemoveAt(num);
				num--;
			}
		}
		for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(list); num2++)
		{
			CustomizeItemInstance customizeItemInstance2 = new CustomizeItemInstance();
			customizeItemInstance2.Open(this, list[num2]);
			if (_customizeItemInstanceList == null)
			{
				_customizeItemInstanceList = new List<CustomizeItemInstance>();
			}
			_customizeItemInstanceList.Add(customizeItemInstance2);
		}
	}

	private void ReleaseCustomizeItemInstance()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_customizeItemInstanceList); i++)
		{
			_customizeItemInstanceList[i].Close();
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_customizeItemInstanceList);
	}

	public void ReOpenWeapon()
	{
		if (_weaponList != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_weaponList); i++)
			{
				_weaponList[i].Close();
			}
			_weaponList.Clear();
		}
		ReleaseEffect();
		OpenWeapon();
	}

	private void ReleaseEffect()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_selfEffectList); i++)
		{
			_selfEffectList[i].Close();
		}
	}

	public void RefreshAnimationData()
	{
		for (int i = 0; i < OpenData.GetAnimationCount(); i++)
		{
			IAnimationData animationData = OpenData.GetAnimationData(i);
			if (animationData != null)
			{
				AnimationKeyData key = animationData.GetKey();
				if (!AnimationDic.ContainsKey(key))
				{
					AnimationDic.Add(key, animationData);
				}
			}
		}
	}

	private void OpenSubCreature()
	{
		int subCreatureCount = OpenData.GetSubCreatureCount();
		if (subCreatureCount == 0)
		{
			return;
		}
		_subCreatureList = new List<SubCreatureInstance>();
		for (int i = 0; i < subCreatureCount; i++)
		{
			ISubCreatureData subCreatureData = OpenData.GetSubCreatureData(i);
			SubCreatureInstance subCreatureInstance = new SubCreatureInstance(this);
			if (subCreatureInstance.Open(OpenData, subCreatureData))
			{
				_subCreatureList.Add(subCreatureInstance);
			}
		}
	}

	private void OpenSelfEffect()
	{
		int selfEffectCount = OpenData.GetSelfEffectCount();
		if (selfEffectCount != 0)
		{
			_selfEffectList = new List<SelfEffectInstance>();
			for (int i = 0; i < selfEffectCount; i++)
			{
				SelfEffectInstance selfEffectInstance = new SelfEffectInstance();
				selfEffectInstance.Open(this, OpenData.GetSelfEffectData(i));
				_selfEffectList.Add(selfEffectInstance);
			}
		}
	}

	private void OpenCustomizeItem()
	{
		if (OpenData.GetCustomizeItemCount() != 0)
		{
			_customizeItemInstanceList = new List<CustomizeItemInstance>();
			for (int i = 0; i < OpenData.GetCustomizeItemCount(); i++)
			{
				CustomizeItemInstance.OpenData customizeItemInfo = OpenData.GetCustomizeItemInfo(i);
				CustomizeItemInstance customizeItemInstance = new CustomizeItemInstance();
				customizeItemInstance.Open(this, customizeItemInfo);
				_customizeItemInstanceList.Add(customizeItemInstance);
			}
		}
	}

	private void OpenWeapon()
	{
		if (OpenData.GetWeaponCount() == 0)
		{
			return;
		}
		_weaponList = new List<WeaponInstance>();
		for (int i = 0; i < OpenData.GetWeaponCount(); i++)
		{
			WeaponInstance.IOpenData weaponOpenData = OpenData.GetWeaponOpenData(i);
			WeaponInstance weaponInstance = new WeaponInstance(this);
			if (weaponInstance.Open(weaponOpenData))
			{
				_weaponList.Add(weaponInstance);
			}
		}
	}

	private void OpenPivot(bool isWeaponPivot)
	{
		for (int i = 0; i < OpenData.GetPivotCount(); i++)
		{
			IPivotData pivotData = OpenData.GetPivotData(i);
			if (String_Extension_Method.IsNullOrEmpty(pivotData.Name))
			{
				continue;
			}
			GameObject gameObject = null;
			if (!isWeaponPivot)
			{
				gameObject = ModelCache.Go;
			}
			else
			{
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_weaponList); j++)
				{
					WeaponInstance weaponInstance = _weaponList[j];
					if (string.Equals(pivotData.From, weaponInstance.Data.Name))
					{
						gameObject = weaponInstance.ModelGo;
						break;
					}
				}
			}
			if (!(gameObject == null))
			{
				Transform transform = gameObject.transform.Find(pivotData.Path);
				if (!(transform == null))
				{
					PivotInstance pivotInstance = new PivotInstance();
					pivotInstance.Transform = transform;
					pivotInstance.Data = pivotData;
					_pivotDic.Add(pivotData.Name, pivotInstance);
				}
			}
		}
	}

	private void MakeTransformCache(Transform tm)
	{
		_transformCache.Add(tm.name, tm);
		for (int i = 0; i < tm.childCount; i++)
		{
			Transform child = tm.GetChild(i);
			MakeTransformCache(child);
		}
	}

	public void Close()
	{
		ExtraRenderFxController = null;
		if (_facial != null)
		{
			_facial.Close();
			_facial = null;
		}
		if (_circleShadow != null)
		{
			_circleShadow.Close();
			_circleShadow = null;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_selfEffectList); i++)
		{
			_selfEffectList[i].Close();
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_selfEffectList);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_subCreatureList); j++)
		{
			_subCreatureList[j].Close();
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_subCreatureList);
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(_weaponList); k++)
		{
			_weaponList[k].Close();
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_weaponList);
		ReleaseCustomizeItemInstance();
		if (_animation != null)
		{
			_animation.Close();
			_animation = null;
		}
		ModelCache.DestroyGameObject();
		InstanceCache.DestroyGameObject();
		if (_renderGroupModifier != null)
		{
			SmallPooling<RenderGroupModifier2>.ReleaseInstance(_renderGroupModifier);
			_renderGroupModifier = null;
		}
		if (_mtrlModifierList != null)
		{
			_mtrlModifierList.Clear();
		}
		_pivotDic.Clear();
		_transformCache.Clear();
	}

	private void VisitSelf(IVisitor visitor)
	{
		_renderCache.Visit(visitor);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_weaponList); i++)
		{
			_weaponList[i].Visit(visitor);
		}
		_circleShadow.Visit(visitor);
	}

	void ICreature.Visit(IVisitor visitor)
	{
		VisitSelf(visitor);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_subCreatureList); i++)
		{
			_subCreatureList[i].Visit(visitor);
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_selfEffectList); j++)
		{
			_selfEffectList[j].Visit(visitor);
		}
		ForCustomizeItemList(delegate(CustomizeItemInstance customizeItem)
		{
			customizeItem.Visit(visitor);
		});
	}

	private void ForCustomizeItemList(Action<CustomizeItemInstance> action)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_customizeItemInstanceList); i++)
		{
			Vespa_System_Action_Extension_Method.fire(action, _customizeItemInstanceList[i]);
		}
	}

	void ICreature.SetParent(Transform parentTm)
	{
		InstanceCache.Tm.SetParent(parentTm, worldPositionStays: false);
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(InstanceCache.Go, parentTm.gameObject.layer);
	}

	void ICreature.SetShaderTransform(ShaderTransformData shaderTransform, Transform pivotTm)
	{
		if (pivotTm == null)
		{
			pivotTm = InstanceCache.Tm;
		}
		ExtraRenderFxController.Open(shaderTransform, pivotTm);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_selfEffectList); i++)
		{
			_selfEffectList[i].RefreshExtraRenderFx();
		}
		ForCustomizeItemList(delegate(CustomizeItemInstance customizeItem)
		{
			customizeItem.RefreshExtraRenderFx();
		});
	}

	void ICreature.SetGrayscale(bool isEnable)
	{
		ExtraRenderFxController.SetGrayscale(isEnable);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_selfEffectList); i++)
		{
			_selfEffectList[i].RefreshExtraRenderFx();
		}
		ForCustomizeItemList(delegate(CustomizeItemInstance customizeItem)
		{
			customizeItem.RefreshExtraRenderFx();
		});
	}

	RenderGroupModifier2 ICreature.GetRenderGroupModifier()
	{
		return _renderGroupModifier;
	}

	public void FlushRenderGroupModifier()
	{
		if (!_renderGroupModifier.BeginFlush())
		{
			return;
		}
		VisitSelf(_renderGroupModifier);
		_renderGroupModifier.EndFlush();
		bool flag = _renderGroupModifier.ExtraAlphaFinal > 0.0019607844f;
		if (flag != _enableRenderVisitor.IsEnabled)
		{
			_enableRenderVisitor.IsEnabled = flag;
			VisitSelf(_enableRenderVisitor);
		}
		if (flag != _activeCustomizeItemVisitor.IsActived)
		{
			_activeCustomizeItemVisitor.IsActived = flag;
			ForCustomizeItemList(delegate(CustomizeItemInstance customizeItem)
			{
				customizeItem.Visit(_activeCustomizeItemVisitor);
			});
		}
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(_subCreatureList); num++)
		{
			_subCreatureList[num].Apply(_renderGroupModifier);
		}
		for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(_selfEffectList); num2++)
		{
			_selfEffectList[num2].Apply(_renderGroupModifier);
		}
		for (int num3 = 0; num3 < __Extension_Method_ICollection__.COUNT(_customizeItemInstanceList); num3++)
		{
			_customizeItemInstanceList[num3].Apply(_renderGroupModifier);
		}
	}

	private ILocalTimer GetValidTimer()
	{
		if (LocalTimer != null)
		{
			return LocalTimer;
		}
		return MainTimer.DefaultLocal;
	}

	void ICreature.ChainUpdate()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_selfEffectList); i++)
		{
			_selfEffectList[i].CheckEnable();
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_weaponList); j++)
		{
			_weaponList[j].ChainUpdate();
		}
		ILocalTimer validTimer = GetValidTimer();
		if (OpenData.GetBaseData().RotationType == CreatureBaseData.ERotationType.Blend)
		{
			float maxDegreesDelta = OpenData.GetBaseData().RotationBlendMaxSpeed * ((float)validTimer.deltaTick * 0.001f);
			InstanceCache.Tm.localRotation = Quaternion.RotateTowards(InstanceCache.Tm.localRotation, _rotationBlend, maxDegreesDelta);
		}
		if (_circleShadow != null)
		{
			_circleShadow.ChainUpdate();
		}
		ApplyMaterialModifier();
		ExtraRenderFxController.ChainUpdate();
		_renderGroupModifier.ChainUpdate(validTimer);
		FlushRenderGroupModifier();
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(_subCreatureList); k++)
		{
			_subCreatureList[k].ChainUpdate();
		}
		for (int l = 0; l < __Extension_Method_ICollection__.COUNT(_selfEffectList); l++)
		{
			_selfEffectList[l].ChainUpdate();
		}
		for (int m = 0; m < __Extension_Method_ICollection__.COUNT(_customizeItemInstanceList); m++)
		{
			_customizeItemInstanceList[m].CheckEnable();
			_customizeItemInstanceList[m].ChainUpdate();
		}
	}

	private void ApplyMaterialModifier()
	{
		if (_mtrlModifierList == null)
		{
			return;
		}
		for (int i = 0; i < _mtrlModifierList.Count; i++)
		{
			ICreatureMaterialModifier creatureMaterialModifier = _mtrlModifierList[i];
			if (creatureMaterialModifier.IsEnd)
			{
				RevertMaterialModifier(creatureMaterialModifier);
				_mtrlModifierList.RemoveAt(i);
				i--;
			}
			else if (creatureMaterialModifier.IsDirty)
			{
				Material materialByNames = _renderCache.GetMaterialByNames(creatureMaterialModifier.RendererName, creatureMaterialModifier.MaterialName, isShared: false);
				if (materialByNames == null)
				{
					_mtrlModifierList.RemoveAt(i);
					i--;
				}
				else
				{
					creatureMaterialModifier.ApplyFieldValue(materialByNames);
				}
			}
		}
	}

	private void RevertMaterialModifier(ICreatureMaterialModifier modifier)
	{
		Material materialByNames = _renderCache.GetMaterialByNames(modifier.RendererName, modifier.MaterialName, isShared: true);
		if (!(materialByNames == null))
		{
			Material materialByNames2 = _renderCache.GetMaterialByNames(modifier.RendererName, modifier.MaterialName, isShared: false);
			if (!(materialByNames2 == null))
			{
				modifier.RevertFieldValue(materialByNames, materialByNames2);
			}
		}
	}

	void ICreature.SetTimer(ILocalTimer localTimer)
	{
		LocalTimer = localTimer;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_subCreatureList); i++)
		{
			_subCreatureList[i].SetTimer(localTimer);
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_selfEffectList); j++)
		{
			_selfEffectList[j].SetTimer(localTimer);
		}
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(_customizeItemInstanceList); k++)
		{
			_customizeItemInstanceList[k].SetTimer(localTimer);
		}
	}

	void ICreature.SetRotation(Quaternion rotation, bool reset)
	{
		_rotationBlend = rotation;
		switch (OpenData.GetBaseData().RotationType)
		{
		case CreatureBaseData.ERotationType.Normal:
			InstanceCache.Tm.localRotation = rotation;
			break;
		case CreatureBaseData.ERotationType.Blend:
			if (reset)
			{
				InstanceCache.Tm.localRotation = rotation;
			}
			break;
		case CreatureBaseData.ERotationType.Ignore:
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_subCreatureList); i++)
			{
				_subCreatureList[i].SetRotation(rotation, reset);
			}
			break;
		}
		}
	}

	void ICreature.AddMaterialModifier(ICreatureMaterialModifier modifier)
	{
		if (_mtrlModifierList == null)
		{
			_mtrlModifierList = new List<ICreatureMaterialModifier>();
		}
		_mtrlModifierList.Add(modifier);
	}

	void ICreature.ClearAnimation()
	{
		for (int i = 0; i < AnimationDic.Count; i++)
		{
			AnimationStock animation = new AnimationStock
			{
				Key = AnimationDic.GetKeyByIndex(i)
			};
			Vespa_Common_Class_Extension_Method.Interface((ICreature)this).SetAnimation(animation);
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_subCreatureList); j++)
		{
			_subCreatureList[j].ClearAnimation();
		}
	}

	void ICreature.SetAnimation(AnimationStock stock)
	{
		IAnimationData vALUE = AnimationDic.GETVALUE(stock.Key, null);
		if (vALUE == null)
		{
			if (stock.Key.MotionType == DefineCreature.EMotion.Idle)
			{
				stock.Key = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.BattleIdle);
			}
			else
			{
				if (stock.Key.MotionType != DefineCreature.EMotion.BattleIdle)
				{
					return;
				}
				stock.Key = AnimationKeyData.CreateMotionType(DefineCreature.EMotion.Idle);
			}
			vALUE = AnimationDic.GETVALUE(stock.Key, null);
			if (vALUE == null)
			{
				return;
			}
		}
		_animation.SetAnimation(vALUE.GetKey(), stock.Frame, stock.Weight);
		int num = __Extension_Method_ICollection__.COUNT(_weaponList);
		bool flag = true;
		bool isNullNode = true;
		for (int i = 0; i < num; i++)
		{
			WeaponInstance weaponInstance = _weaponList[i];
			bool nodeVisible = vALUE.GetNodeVisible(weaponInstance.Data.Name, stock.Frame, out isNullNode);
			flag = (isNullNode ? weaponInstance.Data.IsShow : nodeVisible);
			weaponInstance.SetVisible(flag);
			_weaponList[i].SetAnimation(stock.Key, stock.Frame, stock.Weight);
		}
		for (int j = 0; j < _pivotDic.Count; j++)
		{
			PivotInstance valueByIndex = _pivotDic.GetValueByIndex(j);
			if (valueByIndex.Data.CanHide)
			{
				bool nodeVisible2 = vALUE.GetNodeVisible(valueByIndex.Data.Name, stock.Frame, out isNullNode);
				if (valueByIndex.Transform.gameObject.activeSelf != nodeVisible2)
				{
					valueByIndex.Transform.gameObject.SetActive(nodeVisible2);
				}
			}
		}
		if (_facial != null)
		{
			if (stock.FacialAnimation != null)
			{
				if (stock.FacialAnimation.BlendData == null)
				{
					_facial.SetFacialWeight(stock.FacialAnimation, stock.Frame, stock.Weight);
				}
				else
				{
					_facial.SetFacialWeight(stock.FacialAnimation, stock.FacialAnimation.BlendData.BlendElapsedTime, stock.FacialAnimation.BlendData.Weight);
				}
			}
			else
			{
				FacialAnimationData facialAnimation = vALUE.GetFacialAnimation();
				if (facialAnimation != null)
				{
					_facial.SetFacialWeight(facialAnimation, stock.Frame, stock.Weight);
				}
			}
			if (stock.PrevFacialAnimation != null)
			{
				if (stock.PrevFacialAnimation.BlendData != null)
				{
					_facial.SetFacialWeight(stock.PrevFacialAnimation, 0f, stock.PrevFacialAnimation.BlendData.Weight);
				}
				else
				{
					_facial.SetFacialWeight(stock.PrevFacialAnimation, 0f, 0f);
				}
			}
		}
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(_subCreatureList); k++)
		{
			_subCreatureList[k].SetAnimation(stock);
		}
		if (stock.IsMain && _selfEffectList != null)
		{
			for (int l = 0; l < _selfEffectList.Count; l++)
			{
				_selfEffectList[l].OnAnimation(stock);
			}
		}
	}

	public void CloseAllNonPermanentEffects()
	{
		if (_selfEffectList == null)
		{
			return;
		}
		for (int i = 0; i < _selfEffectList.Count; i++)
		{
			if (!_selfEffectList[i].IsPlayTypeAlways())
			{
				_selfEffectList[i].Close();
			}
		}
	}

	void ICreature.ClearFacialAnimation(FacialAnimationData facialAnimation)
	{
		if (_facial != null && facialAnimation != null && !__Extension_Method_ICollection__.IsNullOrEmpty(facialAnimation.StaticShapeList))
		{
			_facial.SetFacialWeight(facialAnimation, 0f, 0f);
		}
	}

	IAnimationData ICreature.GetAnimationData(AnimationKeyData key)
	{
		return AnimationDic.GETVALUE(key, null);
	}

	float ICreature.GetAnimationBlend(AnimationKeyData motionFrom, AnimationKeyData motionTo)
	{
		if (AnimationBlend != null && AnimationBlend.TryBlendTime(motionFrom, motionTo, out var blendTime))
		{
			return blendTime;
		}
		if (GameSetting.data.GameAsset.AnimationBlend.TryBlendTime(motionFrom.MotionType, motionTo.MotionType, out blendTime))
		{
			return blendTime;
		}
		return DefineCreature.DefaultBlendTime(motionTo.MotionType);
	}

	ICreatureOpenData ICreature.GetCreatureOpenData()
	{
		return OpenData;
	}

	ITransformInstnace ICreature.GetCutomizeItemInstance(CustomizeStock stock)
	{
		if (__Extension_Method_ICollection__.COUNT(_customizeItemInstanceList) == 0)
		{
			return null;
		}
		return _customizeItemInstanceList.Find((CustomizeItemInstance inc) => inc.Stock.IsEqualItem(stock));
	}

	Transform ICreature.GetPivot(EPivotType pivot)
	{
		return pivot switch
		{
			EPivotType.Root => InstanceCache.Tm, 
			EPivotType.AssetRoot => ModelCache.Tm, 
			_ => Vespa_Common_Class_Extension_Method.Interface((ICreature)this).GetPivot(Vespa_String_Extension_Method.GetEnumString(pivot)), 
		};
	}

	Transform ICreature.GetPivot(string name)
	{
		if (String_Extension_Method.IsNullOrEmpty(name))
		{
			return InstanceCache.Tm;
		}
		return _pivotDic.GETVALUE(name, null)?.Transform;
	}

	Transform ICreature.GetPivotSafe(string name)
	{
		Transform pivot = Vespa_Common_Class_Extension_Method.Interface((ICreature)this).GetPivot(name);
		if (pivot != null)
		{
			return pivot;
		}
		return InstanceCache.Tm;
	}

	Transform ICreature.FindTransform(string name, bool skipNullCaching)
	{
		if (_transformCache.TryGetValue(name, out var value))
		{
			if (!skipNullCaching || !(value == null))
			{
				return value;
			}
			_transformCache.Remove(name);
		}
		if (ModelCache.Go == null)
		{
			return null;
		}
		value = Vespa_Unity_Transform_Extension_Method.FindChildRecursively(ModelCache.Tm, name);
		if (skipNullCaching && value == null)
		{
			return null;
		}
		_transformCache.Add(name, value);
		return value;
	}

	Dictionary<string, bool> IAnimationLoadMonitor.GetLoadReports()
	{
		return _animation.GetRegistedClip();
	}
}
