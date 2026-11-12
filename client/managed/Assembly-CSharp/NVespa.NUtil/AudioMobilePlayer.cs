using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa.NUtil;

public class AudioMobilePlayer : MonoBehaviour, IInspectorGuiDrawer
{
	private AudioSourceTester[] _testers;

	private void Start()
	{
		_testers = Object.FindObjectsOfType<AudioSourceTester>();
		GlobalAutoBehaviour<GuiManager>.instance.Add("Audio Test Player", this, delegate
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_testers); i++)
			{
				AudioSourceTester audioSourceTester = _testers[i];
				if (!Vespa_Common_Class_Extension_Method.IsUnityNull(audioSourceTester))
				{
					using (new BlockManager().Horizontal())
					{
						if (GUILayout.Button("Loop", GUILayout.Width(70f)))
						{
							audioSourceTester.Loop();
						}
						if (GUILayout.Button("Stop", GUILayout.Width(70f)))
						{
							audioSourceTester.Stop();
						}
						string text = Vespa_Unity_Object_Extension_Method.NAME(audioSourceTester);
						if (audioSourceTester.IsPlaying)
						{
							text += " - play";
						}
						GUILayout.Label(text);
					}
				}
			}
		});
		GlobalAutoBehaviour<GuiManager>.instance.SetEnable(enabled: true);
	}

	private void Update()
	{
	}
}
