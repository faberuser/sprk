using UnityEngine;

namespace NVespa.NRenderProxy;

public class DefaultMaterialFactory : IMaterialFactory
{
	public static DefaultMaterialFactory Default = new DefaultMaterialFactory();

	Material[] IMaterialFactory.CloneMaterials(Material[] sharedMaterials)
	{
		return CloneMaterials(sharedMaterials);
	}

	void IMaterialFactory.ReleaseMaterials(Material[] materials, Material[] sharedMaterials)
	{
		ReleaseMaterials(materials);
	}

	public static Material[] CloneMaterials(Material[] sharedMaterials)
	{
		Material[] array = new Material[sharedMaterials.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (sharedMaterials[i] == null)
			{
				array[i] = null;
				continue;
			}
			Material material = new Material(sharedMaterials[i]);
			array[i] = material;
		}
		return array;
	}

	public static void ReleaseMaterials(Material[] materials)
	{
		for (int i = 0; i < materials.Length; i++)
		{
			if (!(materials[i] == null))
			{
				VespaObject.DestroyObject(materials[i]);
			}
		}
	}
}
