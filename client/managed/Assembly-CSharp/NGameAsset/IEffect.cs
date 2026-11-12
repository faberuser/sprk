using NVespa;
using UnityEngine;

namespace NGameAsset;

public interface IEffect
{
	void Close(bool detachParent);

	bool IsEmpty();

	void Release();

	RenderGroupModifier2 GetRenderGroupModifier();

	void PlayPhase(string phaseName);

	void SetVisible(bool isVisible);

	void Visit(IVisitor visitor);

	void SetDummyParent(Vector3 position, Quaternion rotation, Vector3 scale, string dummyName = "");

	void SetParent(ICreature parent);

	void SetParent(Transform parentTm);

	void SetShaderTransform(ShaderTransformData shaderTransform, Transform pivotTm);

	void SetGrayscale(bool isEnable);

	void SetTimer(ILocalTimer timer);

	void Update();

	void Update(float deltaTime);

	bool IsEnd();

	Transform FindTransform(string name);
}
