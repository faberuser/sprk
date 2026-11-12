using UnityEngine;

namespace NGameAsset;

public interface ICreatureMaterialModifier
{
	bool IsEnd { get; }

	bool IsDirty { get; }

	string RendererName { get; }

	string MaterialName { get; }

	void ApplyFieldValue(Material mtrl);

	void RevertFieldValue(Material sharedMtrl, Material mtrl);
}
