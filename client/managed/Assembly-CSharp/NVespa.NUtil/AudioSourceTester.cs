using System.Collections;
using UnityEngine;

namespace NVespa.NUtil;

public class AudioSourceTester : MonoBehaviour, IInspectorGuiDrawer
{
	public float Delay = 1f;

	public bool IsPlaying { get; private set; }

	public void Loop()
	{
		IsPlaying = true;
		StopAllCoroutines();
		StartCoroutine(coLoop());
	}

	public void Stop()
	{
		IsPlaying = false;
		StopAllCoroutines();
	}

	private IEnumerator coLoop()
	{
		AudioSource audioSource = GetComponent<AudioSource>();
		while (true)
		{
			audioSource.loop = false;
			audioSource.Play();
			while (audioSource.isPlaying)
			{
				yield return null;
			}
			yield return new WaitForSeconds(Delay);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
