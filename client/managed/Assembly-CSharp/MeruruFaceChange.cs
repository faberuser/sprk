using System.Collections.Generic;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

public class MeruruFaceChange : MonoBehaviour
{
	public GameObject m_faceUp;

	public GameObject m_faceDown;

	public List<Material> m_listFace;

	private bool _enableUpside = true;

	private bool _enableDownside = true;

	private int _lastIndex;

	private void Start()
	{
		SceneAutoBehaviour<GuiManager>.instance.Add("FaceUpdate", this, DrawGUI);
	}

	private void Update()
	{
	}

	private void DrawGUI()
	{
		int num = _lastIndex;
		using (new BlockManager().Horizontal())
		{
			_enableUpside = GUILayout.Toggle(_enableUpside, "Upside", GUILayout.Height(50f));
			_enableDownside = GUILayout.Toggle(_enableDownside, "Downside", GUILayout.Height(50f));
		}
		using (new BlockManager().Horizontal())
		{
			if (GUILayout.Button("<", GUILayout.Height(50f)))
			{
				num = (_lastIndex - 1 + m_listFace.Count) % m_listFace.Count;
			}
			GUILayout.Box(_lastIndex.ToString());
			if (GUILayout.Button(">", GUILayout.Height(50f)))
			{
				num = (_lastIndex + 1) % m_listFace.Count;
			}
		}
		for (int i = 0; i < m_listFace.Count; i++)
		{
			if (GUILayout.Button(m_listFace[i].name))
			{
				num = i;
			}
		}
		if (num != _lastIndex)
		{
			Material material = m_listFace[num];
			if (_enableUpside)
			{
				m_faceUp.GetComponent<Renderer>().material = material;
			}
			if (_enableDownside)
			{
				m_faceDown.GetComponent<Renderer>().material = material;
			}
			_lastIndex = num;
		}
	}
}
