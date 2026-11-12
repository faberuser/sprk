using UnityEngine;

namespace NGameAsset;

public abstract class CreatureMaterialModifierT<T> : ICreatureMaterialModifier
{
	protected T _fieldValue;

	public bool IsEnd { get; private set; }

	public string RendererName { get; private set; }

	public string MaterialName { get; private set; }

	public int PropertyId { get; private set; }

	public bool IsDirty { get; protected set; }

	public CreatureMaterialModifierT(string rendererName, string materialName, string propertyName)
	{
		RendererName = rendererName;
		MaterialName = materialName;
		PropertyId = Shader.PropertyToID(propertyName);
	}

	public void SetFieldValue(T value)
	{
		_fieldValue = value;
		IsDirty = true;
	}

	public void End()
	{
		IsEnd = true;
	}

	public abstract void ApplyFieldValue(Material mtrl);

	public abstract void RevertFieldValue(Material sharedMtrl, Material mtrl);
}
