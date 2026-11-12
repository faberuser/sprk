using NGameAsset.NCreature;
using NVespa;
using UnityEngine;

namespace NGameAsset;

public interface ICreature
{
	void Close();

	ICreatureOpenData GetCreatureOpenData();

	void SetParent(Transform parentTm);

	void SetShaderTransform(ShaderTransformData shaderTransform, Transform pivotTm);

	void SetGrayscale(bool isEnable);

	RenderGroupModifier2 GetRenderGroupModifier();

	void ChainUpdate();

	void Visit(IVisitor visitor);

	void SetTimer(ILocalTimer timer);

	void SetRotation(Quaternion rotation, bool reset);

	void AddMaterialModifier(ICreatureMaterialModifier modifier);

	void ClearAnimation();

	void SetAnimation(AnimationStock stock);

	void ClearFacialAnimation(FacialAnimationData facialAnimation);

	IAnimationData GetAnimationData(AnimationKeyData key);

	float GetAnimationBlend(AnimationKeyData motionFrom, AnimationKeyData motionTo);

	Transform GetPivot(EPivotType pivot);

	Transform GetPivot(string name);

	Transform GetPivotSafe(string name);

	Transform FindTransform(string name, bool skipNullCaching = false);

	ITransformInstnace GetCutomizeItemInstance(CustomizeStock stock);
}
