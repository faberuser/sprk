using System;
using System.Collections;
using System.IO;
using UnityEngine;

public sealed class SprkIllustrationProbeRunner : MonoBehaviour
{
	public void Begin(string output)
	{
		StartCoroutine(Guard(SprkIllustrationUpscale.Probe(output), output));
	}

	private IEnumerator Guard(IEnumerator routine, string output)
	{
		while (true)
		{
			object current;
			try
			{
				if (!routine.MoveNext())
				{
					break;
				}
				current = routine.Current;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(output, "FAIL.txt"), ex.ToString());
				Debug.LogException(ex);
				Application.Quit();
				break;
			}
			yield return current;
		}
	}
}
