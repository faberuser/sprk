using UnityEngine;

namespace NVespa.NRenderProxy;

public interface IMaterialFactory
{
	Material[] CloneMaterials(Material[] sharedMaterials);

	void ReleaseMaterials(Material[] materials, Material[] sharedMaterials);
}
