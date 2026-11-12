using System;
using System.Collections;
using System.IO;
using NGame2.NUI.NWindow;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene;

public class LogoScene : MonoBehaviour
{
	public class CustomAndroidJavaObject : AndroidJavaObject
	{
		private bool _isExists;

		public CustomAndroidJavaObject(string className)
			: base(className)
		{
			AndroidJNI.FindClass(className);
			_isExists = true;
		}

		public bool IsExists()
		{
			return _isExists;
		}
	}

	public float FadeInTime = 0.5f;

	public float StayTime = 0.5f;

	public float FadeOutTime = 0.5f;

	public UIWidget LogoWidget;

	public bool RequireMoveScene = true;

	public AudioClip LogoAudioClip;

	[Header("Login BI")]
	public LoginBI LoginBI;

	private Aes128 _aes = new Aes128();

	private char[] tempSalt = new char[16]
	{
		'a', 'p', 'r', 's', 'f', 'n', 'k', 'w', 'a', 'e',
		'c', 'd', 'g', 'b', 'b', 'm'
	};

	private string[] CheckJavaClassList = new string[4] { "whwKCl7Fll+gXS2IbVlkZX8M6HzNWjK5nugd3BY7IOOHs1NMHZ8UmWqjxweBgautff4Yhwu/Jv1WwL5dyeUs1g==", "OIDrkM6f6O8+U+QhzegebFE+usvbogmSdRrk+jKYm0E=", "w4URXQN5071NA5ctTaX/l4RP7QDSzB7Q8C7c4aIBOWA=", "q4+olFbnRAGZI3eeJrDE7cnpJZpjTHabxE7ppGOHyg8mkGS3VJhVdYr+vd9yEqfN" };

	private string[] CheckFileList = new string[1] { "ea8JBpqic8YlZEvzJgt+SnYdpsBnOMeg7qcOx3uRnhdgWiuhBUDk4TdwHQPnV6/U" };

	private string[] CheckAssetsFileList = new string[2] { "4upgC3IQV8fyZkNu6RbIHYWDvjGYrTnS9t+Yakx1Rqc=", "uiH/gqQDCCtiusB+joS4MJ5AUQRDbHWG5q0BZHPoetM=" };

	public static bool IsLogoPlayed { get; set; }

	private void Salting()
	{
		for (int i = 0; i < tempSalt.Length; i++)
		{
			int num = ((i * 10303 % 5) ^ ((i / 2 == 0) ? (i + 1) : (i + 3))) + 33 + i;
			string text = Convert.ToBase64String(new byte[1] { (byte)num });
			tempSalt[i] = text[0];
		}
	}

	private void CheckJavaClass(string packageName, Action onFindAction)
	{
	}

	private void CheckModsLibFile(string path, Action onFindAction)
	{
		string path2 = Path.Combine(Application.dataPath, _aes.DecryptTextLegacy(path, new string(tempSalt)));
		path2 = Path.GetFullPath(path2);
		try
		{
			if (File.Exists(path2))
			{
				VespaDebug.LogError("CheckModsLIbFile - onFindAction");
				Vespa_System_Action_Extension_Method.fire(onFindAction);
			}
		}
		catch (Exception)
		{
		}
	}

	private void Start()
	{
		if (!IsLogoPlayed)
		{
			VespaDebug.InitUnityThreadId();
			VespaCoroutine.Start(this, coDisplayLogo());
		}
	}

	private void Quit()
	{
		LoadingManager.MobileQuit();
	}

	private IEnumerator coDisplayLogo()
	{
		VespaDebug.InitUnityThreadId();
		_ = SystemOptionData.Sound_EnableGameSound;
		yield return new WaitForSeconds(0.1f);
		Salting();
		string[] checkJavaClassList = CheckJavaClassList;
		foreach (string packageName in checkJavaClassList)
		{
			CheckJavaClass(packageName, Quit);
		}
		checkJavaClassList = CheckFileList;
		foreach (string path in checkJavaClassList)
		{
			CheckModsLibFile(path, Quit);
		}
		float elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += GlobalTimer.deltaTime;
			float alpha = Mathf.Clamp01(elapsedTime / Vespa_Float_Extension_Method.NoneZero(FadeInTime));
			if (LogoWidget != null)
			{
				LogoWidget.alpha = alpha;
			}
		}
		while (!(elapsedTime >= FadeInTime));
		elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += GlobalTimer.deltaTime;
		}
		while (!(elapsedTime >= StayTime));
		if (RequireMoveScene)
		{
			IsLogoPlayed = true;
			GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Login, LoadingManager.ELoadingType.Black, resetEnvironment: true);
			yield break;
		}
		elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += GlobalTimer.deltaTime;
			float num = Mathf.Clamp01(elapsedTime / Vespa_Float_Extension_Method.NoneZero(FadeOutTime));
			if (LogoWidget != null)
			{
				LogoWidget.alpha = 1f - num;
			}
		}
		while (!(elapsedTime >= FadeOutTime));
		if (LoginBI != null)
		{
			LoginBI.Play();
		}
		else
		{
			LoginBI.IsLoginBINull = true;
		}
		IsLogoPlayed = true;
	}
}
