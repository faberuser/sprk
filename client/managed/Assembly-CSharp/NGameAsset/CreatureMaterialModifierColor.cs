using UnityEngine;

namespace NGameAsset;

public class CreatureMaterialModifierColor : CreatureMaterialModifierT<Color>
{
	public CreatureMaterialModifierColor(string rendererName, string materialName, string propertyName)
		: base(rendererName, materialName, propertyName)
	{
	}

	public override void ApplyFieldValue(Material mtrl)
	{
		mtrl.SetColor(base.PropertyId, _fieldValue);
		base.IsDirty = false;
	}

	public override void RevertFieldValue(Material sharedMtrl, Material mtrl)
	{
		Color color = sharedMtrl.GetColor(base.PropertyId);
		mtrl.SetColor(base.PropertyId, color);
	}
}
