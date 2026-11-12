using UnityEngine;

namespace NVespa.NNGUIExtension;

[ExecuteInEditMode]
[AddComponentMenu("Vespa/NGUI Extension/UI Simple Component")]
public class UISimpleComponent : MonoBehaviour
{
	public Component[] Components;

	public T GetComponent<T>(int index) where T : Component
	{
		if (index < __Extension_Method_ICollection__.COUNT(Components) && (bool)Components[index])
		{
			return Components[index].GetComponent<T>();
		}
		return null;
	}
}
