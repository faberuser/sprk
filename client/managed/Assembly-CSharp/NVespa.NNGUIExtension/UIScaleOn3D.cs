using UnityEngine;

namespace NVespa.NNGUIExtension;

[ExecuteInEditMode]
[AddComponentMenu("Vespa/NGUI Extension/UI Scale On 3D")]
public class UIScaleOn3D : MonoBehaviour
{
	public float MultiplyScale = 1f;

	public int ScreenHeight = 720;

	[HideInInspector]
	public float SubMultiplyScale = 1f;

	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
	}

	private void Update()
	{
		if ((float)ScreenHeight > 0f)
		{
			float num = 2f / (float)ScreenHeight * MultiplyScale * SubMultiplyScale;
			Vector3 localScale = base.transform.localScale;
			if (Mathf.Abs(localScale.x - num) > float.Epsilon || Mathf.Abs(localScale.y - num) > float.Epsilon || Mathf.Abs(localScale.z - num) > float.Epsilon)
			{
				base.transform.localScale = new Vector3(num, num, num);
			}
		}
	}
}
