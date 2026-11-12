using System.Collections.Generic;
using NGameAsset.NCreature;
using UnityEngine;

namespace NGameAsset;

public interface ICreatureOpenData : IAnimationOpenData
{
	CreatureStock GetStock();

	CreatureBaseData GetBaseData();

	List<MotionSequenceData> GetMotionSequenceList();

	IAnimationBlendData GetAnimationBlend();

	IModelData GetModelData();

	GameObject GetModelAsset();

	Material GetModelHairReplacementAsset();

	IHairData GetHairData();

	GameObject GetHairAsset();

	Material GetHairMaterialAsset();

	int GetFacialModelCount();

	IFacialModelData GetFacialModelData(int index);

	Mesh GetFacialModelMesh(int index);

	int GetSelfEffectCount();

	SelfEffectData GetSelfEffectData(int index);

	int GetPivotCount();

	IPivotData GetPivotData(int index);

	int GetSubCreatureCount();

	ISubCreatureData GetSubCreatureData(int index);

	int GetWeaponCount();

	WeaponInstance.IOpenData GetWeaponOpenData(int index);

	int GetCustomizeItemCount();

	CustomizeItemInstance.OpenData GetCustomizeItemInfo(int index);
}
