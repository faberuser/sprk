using NVespa;
using NVespa.NUtil;
using UnityEngine;

namespace NTestingTools;

public class ControlChar : MonoBehaviour
{
	public int nameHashForStand = Animator.StringToHash("Stand");

	public int nameHashForRun = Animator.StringToHash("Run");

	public VirtualPad m_virtualPad;

	public Transform m_charTM;

	public Animator m_charAnimator;

	public Transform m_cameraTM;

	public float m_runSpeed = 3f;

	public Material m_normalMaterial;

	public Material m_emotion1Material;

	public Material m_emotion2Material;

	public Material m_emotion3Material;

	public GameObject m_faceUpper;

	public GameObject m_faceLower;

	private float _disableUpdateMaterialTime;

	private void Start()
	{
	}

	private void Update()
	{
		AnimatorStateInfo currentAnimatorStateInfo = m_charAnimator.GetCurrentAnimatorStateInfo(0);
		if (currentAnimatorStateInfo.IsName("Stand") || currentAnimatorStateInfo.IsName("Run"))
		{
			UpdateMaterial(m_normalMaterial);
			if (m_virtualPad.m_padDirection.sqrMagnitude < Mathf.Epsilon)
			{
				RunMotion(i_enable: false);
				return;
			}
			Vector3 vector = new Vector3(m_virtualPad.m_padDirection.x, 0f, m_virtualPad.m_padDirection.y);
			vector = m_cameraTM.localToWorldMatrix.MultiplyVector(vector);
			m_charTM.position += vector * MainTimer.deltaTime * m_runSpeed;
			m_charTM.forward = vector;
			RunMotion(i_enable: true);
		}
	}

	private void RunMotion(bool i_enable)
	{
		m_charAnimator.SetBool("Run", i_enable);
	}

	private void UpdateMaterial(Material i_material)
	{
		if (!(MainTimer.time < _disableUpdateMaterialTime))
		{
			if (m_faceUpper != null)
			{
				m_faceUpper.GetComponent<Renderer>().material = i_material;
			}
			if (m_faceLower != null)
			{
				m_faceLower.GetComponent<Renderer>().material = i_material;
			}
		}
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.Scale(Vector3.one * Screen.height / 800f);
		if (GUI.Button(new Rect(1080f, 500f, 200f, 100f), "Emotion 1"))
		{
			UpdateMaterial(m_emotion1Material);
			m_charAnimator.CrossFade("Emotion1", 0.1f);
			_disableUpdateMaterialTime = MainTimer.time + 0.3f;
		}
		if (GUI.Button(new Rect(1080f, 600f, 200f, 100f), "Emotion 2"))
		{
			UpdateMaterial(m_emotion2Material);
			m_charAnimator.CrossFade("Emotion2", 0.1f);
			_disableUpdateMaterialTime = MainTimer.time + 0.3f;
		}
		if (GUI.Button(new Rect(1080f, 700f, 200f, 100f), "Emotion 3"))
		{
			UpdateMaterial(m_emotion3Material);
			m_charAnimator.CrossFade("Emotion3", 0.1f);
			_disableUpdateMaterialTime = MainTimer.time + 0.3f;
		}
	}
}
