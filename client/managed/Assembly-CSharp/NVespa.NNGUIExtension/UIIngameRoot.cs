using UnityEngine;

namespace NVespa.NNGUIExtension;

[ExecuteInEditMode]
[AddComponentMenu("Vespa/NGUI Extension/Root")]
public class UIIngameRoot : MonoBehaviour
{
	public float m_additionalScale = 1f;

	public int m_manualHeight = 720;

	private Transform _trans;

	protected virtual void Awake()
	{
		_trans = base.transform;
	}

	protected virtual void Start()
	{
	}

	private void Update()
	{
		if (_trans != null && (float)m_manualHeight > 0f)
		{
			float num = 2f / (float)m_manualHeight * m_additionalScale;
			Vector3 localScale = _trans.localScale;
			if (!(Mathf.Abs(localScale.x - num) <= float.Epsilon) || !(Mathf.Abs(localScale.y - num) <= float.Epsilon) || !(Mathf.Abs(localScale.z - num) <= float.Epsilon))
			{
				_trans.localScale = new Vector3(num, num, num);
			}
		}
	}
}
