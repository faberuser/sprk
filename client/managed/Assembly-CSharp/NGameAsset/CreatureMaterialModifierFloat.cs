using UnityEngine;

namespace NGameAsset;

public class CreatureMaterialModifierFloat : CreatureMaterialModifierT<float>
{
	public CreatureMaterialModifierFloat(string rendererName, string materialName, string propertyName)
		: base(rendererName, materialName, propertyName)
	{
	}

	public override void ApplyFieldValue(Material mtrl)
	{
		mtrl.SetFloat(base.PropertyId, _fieldValue);
		base.IsDirty = false;
	}

	public override void RevertFieldValue(Material sharedMtrl, Material mtrl)
	{
		float value = sharedMtrl.GetFloat(base.PropertyId);
		mtrl.SetFloat(base.PropertyId, value);
	}
}
