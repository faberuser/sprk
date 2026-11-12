using System.Collections.Generic;
using NGameAsset.NEffect.NPlaySequence;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGameAsset.NEffect;

public class ModelInstance
{
	public enum EPlayState
	{
		Play,
		Ending,
		EndingDisappear,
		End
	}

	private IAssetFactory<GameObject> _assetFactory;

	private Animation _animationComp;

	private AnimationState _animationState;

	private bool _effectModelAttached;

	private float _disappearAlpha = 1f;

	private GlobalBillboardManager.EntryData _billbiardEntry;

	private PoolingList<ModelRenderer> RenderInfoList;

	private PoolingList<ISequence> _sequenceList;

	public IModelData Data { get; private set; }

	public EffectInstance Owner { get; private set; }

	public GameObject InstanceGo { get; private set; }

	public GameObject ModelGo { get; private set; }

	public bool IsEnd => PlayState != EPlayState.Play;

	public EPlayState PlayState { get; private set; }

	public bool IsVisible { get; set; }

	public float PlayingTime { get; private set; }

	public float DisappearTime { get; private set; }

	public float ClipFrame { get; private set; }

	public ModelInstance(EffectInstance owner)
	{
		Owner = owner;
	}

	public bool Open(IModelData data, IAssetFactory<GameObject> assetFactory, int layer, ITransformData extraTransform)
	{
		Data = data;
		_assetFactory = assetFactory;
		ModelGo = _assetFactory.Create();
		if (ModelGo == null)
		{
			return false;
		}
		CommonData commonData = Data.GetCommonData();
		Transform transformForEffectModelAttach = commonData.GetTransformForEffectModelAttach(Owner, this);
		if (transformForEffectModelAttach != null)
		{
			_effectModelAttached = true;
		}
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(ModelGo, layer);
		InstanceGo = SceneAutoBehaviour<GameObjectFactory>.instance.Create();
		InstanceGo.layer = ModelGo.layer;
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(InstanceGo.transform, transformForEffectModelAttach);
		commonData.TransformSet.AssignTransform(InstanceGo.transform, _effectModelAttached ? null : extraTransform);
		if (commonData.BillboardType != CommonData.EBillboardType.None)
		{
			GlobalBillboardManager.EBillboardType globalBillboardType = commonData.GetGlobalBillboardType();
			_billbiardEntry = SingletonUpdater<GlobalBillboardManager>.instance.AddBiilboard(globalBillboardType, InstanceGo.transform);
		}
		Vespa_Unity_Transform_Extension_Method.ParentRelative(ModelGo.transform, InstanceGo.transform);
		SetupRenderer();
		SetupAnimation();
		PlayingTime = 0f;
		DisappearTime = 0f;
		ClipFrame = -1f;
		PlayState = EPlayState.Play;
		IsVisible = false;
		_disappearAlpha = 1f;
		UpdateVisible();
		SequenceReady sequenceReady = SmallPooling<SequenceReady>.CreateInstance();
		sequenceReady.Open(this);
		_sequenceList.Add(sequenceReady);
		List<PlaySequenceData> playSequenceList = Data.GetPlaySequenceList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(playSequenceList); i++)
		{
			PlaySequenceData playSequenceData = playSequenceList[i];
			if (playSequenceData.PlayType == PlaySequenceData.EPlayType.None)
			{
				SequenceNone sequenceNone = SmallPooling<SequenceNone>.CreateInstance();
				sequenceNone.Open(this, playSequenceData);
				_sequenceList.Add(sequenceNone);
			}
			else if (playSequenceData.PlayType == PlaySequenceData.EPlayType.Clamp)
			{
				SequenceClamp sequenceClamp = SmallPooling<SequenceClamp>.CreateInstance();
				sequenceClamp.Open(this, playSequenceData);
				_sequenceList.Add(sequenceClamp);
			}
			else if (playSequenceData.PlayType == PlaySequenceData.EPlayType.Loop)
			{
				SequenceLoop sequenceLoop = SmallPooling<SequenceLoop>.CreateInstance();
				sequenceLoop.Open(this, playSequenceData);
				_sequenceList.Add(sequenceLoop);
			}
			else if (playSequenceData.PlayType == PlaySequenceData.EPlayType.End)
			{
				SequenceEnd sequenceEnd = SmallPooling<SequenceEnd>.CreateInstance();
				sequenceEnd.Open(this, playSequenceData);
				_sequenceList.Add(sequenceEnd);
			}
		}
		return true;
	}

	public void Close(bool detachParent)
	{
		if (detachParent && InstanceGo != null)
		{
			InstanceGo.transform.parent = AssetUpdateManager.DummyRootTm;
		}
		SetEnding();
	}

	public void PlayPhase(string phaseName)
	{
		if (String_Extension_Method.IsNullOrEmpty(phaseName))
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < _sequenceList.Count; i++)
		{
			ISequence sequence = _sequenceList[i];
			if (sequence is SequenceReady)
			{
				num = i + 1;
				continue;
			}
			PlaySequenceData data = sequence.GetData();
			if (data != null && !(data.PhaseName != phaseName))
			{
				num2 = i - num;
				break;
			}
		}
		for (int j = 0; j < num2; j++)
		{
			if (num >= _sequenceList.Count)
			{
				break;
			}
			_sequenceList[num].ReleaseSelf();
			_sequenceList.RemoveAt(num);
		}
	}

	private void SetEnding()
	{
		if (Data.GetEndingDisappear().EnableThis)
		{
			PlayState = EPlayState.EndingDisappear;
		}
		else
		{
			PlayState = EPlayState.Ending;
		}
	}

	private void EndAll()
	{
		PlayState = EPlayState.End;
		if (IsVisible)
		{
			IsVisible = false;
			UpdateVisible();
		}
	}

	public bool IsEmpty()
	{
		return PlayState == EPlayState.End;
	}

	public void Release()
	{
		for (int i = 0; i < _sequenceList.Count; i++)
		{
			_sequenceList[i].ReleaseSelf();
		}
		_sequenceList.Clear();
		for (int j = 0; j < RenderInfoList.Count; j++)
		{
			RenderInfoList[j].Enabled = true;
			RenderInfoList[j].Close();
		}
		RenderInfoList.Clear();
		if (_billbiardEntry != null)
		{
			_billbiardEntry.Close();
			_billbiardEntry = null;
		}
		if (ModelGo != null)
		{
			_assetFactory.Release(ModelGo);
			ModelGo = null;
		}
		if (InstanceGo != null)
		{
			GameObjectFactory.ReleaseGameObject(InstanceGo);
			InstanceGo = null;
		}
		if (_animationComp != null)
		{
			_animationComp = null;
		}
	}

	public void UpdateVisible()
	{
		bool enabled = IsVisible && Owner.IsVisible;
		for (int i = 0; i < RenderInfoList.Count; i++)
		{
			RenderInfoList[i].Enabled = enabled;
		}
	}

	public void Visit(IVisitor visitor)
	{
		for (int i = 0; i < RenderInfoList.Count; i++)
		{
			visitor.TryVisit(RenderInfoList[i].Proxy);
		}
	}

	private void SetupRenderer()
	{
		List<RendererExtraData> renderExtraList = Data.GetRenderExtraList();
		for (int i = 0; i < renderExtraList.Count; i++)
		{
			RendererExtraData rendererExtraData = renderExtraList[i];
			Transform transform = ModelGo.transform.Find(rendererExtraData.RendererPath);
			if (transform == null)
			{
				continue;
			}
			Renderer component = transform.GetComponent<Renderer>();
			if (!(component == null))
			{
				ModelRenderer modelRenderer = new ModelRenderer();
				if (modelRenderer.Open(component, rendererExtraData))
				{
					RenderInfoList.Add(modelRenderer);
				}
			}
		}
	}

	private void SetupAnimation()
	{
		_animationComp = ModelGo.GetComponent<Animation>();
		if (_animationComp == null)
		{
			return;
		}
		_animationComp.playAutomatically = false;
		_animationComp.cullingType = AnimationCullingType.AlwaysAnimate;
		bool flag = true;
		foreach (AnimationState item in _animationComp)
		{
			item.normalizedSpeed = 0f;
			if (flag)
			{
				item.weight = 1f;
				item.enabled = true;
				_animationState = item;
				flag = false;
			}
			else
			{
				item.weight = 0f;
				item.enabled = false;
			}
		}
	}

	public void SetParent(ICreature parent)
	{
		if (!(InstanceGo == null) && !_effectModelAttached)
		{
			Transform attachTransform = Data.GetCommonData().GetAttachTransform(parent);
			if (!(attachTransform == null))
			{
				Vespa_Unity_Transform_Extension_Method.ParentRelative(InstanceGo.transform, attachTransform);
			}
		}
	}

	public void SetParent(Transform parentTm)
	{
		if (!(InstanceGo == null) && !_effectModelAttached)
		{
			Transform attachTransform = Data.GetCommonData().GetAttachTransform(parentTm);
			if (!(attachTransform == null))
			{
				Vespa_Unity_Transform_Extension_Method.ParentRelative(InstanceGo.transform, attachTransform);
			}
		}
	}

	public void Update(float deltaTime)
	{
		if (PlayState == EPlayState.End)
		{
			return;
		}
		PlayingTime += deltaTime;
		float elapsedTime = deltaTime;
		while (_sequenceList.Count > 0)
		{
			ISequence sequence = _sequenceList[0];
			elapsedTime = sequence.Update(elapsedTime);
			if (!sequence.IsEnd())
			{
				break;
			}
			sequence.ReleaseSelf();
			_sequenceList.RemoveAt(0);
		}
		if (!IsEnd && PlayingTime >= Data.GetCommonData().EndTime)
		{
			SetEnding();
		}
		UpdateEnd(deltaTime);
	}

	private void UpdateEnd(float deltaTime)
	{
		if (PlayState == EPlayState.Play || PlayState == EPlayState.End)
		{
			return;
		}
		if (PlayState == EPlayState.Ending)
		{
			if (_sequenceList.IsNullOrEmpty())
			{
				EndAll();
			}
		}
		else if (PlayState == EPlayState.EndingDisappear)
		{
			DisappearTime += deltaTime;
			EndingDisappearData endingDisappear = Data.GetEndingDisappear();
			if (DisappearTime > endingDisappear.TimeLength)
			{
				EndAll();
				return;
			}
			float num = Mathf.Clamp01(DisappearTime / Vespa_Float_Extension_Method.NoneZero(endingDisappear.TimeLength));
			_disappearAlpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(endingDisappear.AlphaCurve, num, 1f - num);
			Vespa_Common_Class_Extension_Method.Interface((IEffect)Owner).GetRenderGroupModifier().SetExtraAlpha(_disappearAlpha);
		}
		else
		{
			PlayState = EPlayState.End;
		}
	}

	public void SetClipFrame(float frame, bool forceUpdate)
	{
		if (!Mathf.Approximately(ClipFrame, frame) || forceUpdate)
		{
			ClipFrame = frame;
			if (_animationComp != null && _animationState != null)
			{
				_animationState.enabled = true;
				_animationState.normalizedTime = ClipFrame;
			}
			UpdateRenderMaterial();
		}
	}

	private void UpdateRenderMaterial()
	{
		for (int i = 0; i < RenderInfoList.Count; i++)
		{
			RenderInfoList[i].UpdateMaterial(ClipFrame, _disappearAlpha);
		}
	}
}
