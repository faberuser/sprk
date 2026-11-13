using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NGame2.NUI.NAtlasAssignComponent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.NJit;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkLoginSettingsProbe : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return new WaitForSecondsRealtime(4f);
		LoginManagement manager = SceneAutoBehaviour<LoginManagement>.instance;
		if (manager == null)
		{
			throw new Exception("No native login management");
		}
		LoginAccountInfo instance = manager.GetInstance<LoginAccountInfo>();
		for (int i = 0; i < 8; i++)
		{
			SprkLoginSettings.Configure(instance);
		}
		if (instance.PatchValidationRoot.activeSelf || instance.VoiceButtonRoot.activeSelf)
		{
			throw new Exception("Legacy buttons visible");
		}
		UIButton button = instance.LanguageButtonRoot.GetComponentInChildren<UIButton>(includeInactive: true);
		if (button.onClick.Count != 1)
		{
			throw new Exception("Duplicate Settings callbacks");
		}
		UILabel[] componentsInChildren = instance.LanguageButtonRoot.GetComponentsInChildren<UILabel>(includeInactive: true);
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (componentsInChildren[j].text != "Settings")
			{
				throw new Exception("Settings caption");
			}
		}
		bool patchReady = TableRoot.PatchDownloadComplete;
		EventDelegate.Execute(button.onClick);
		yield return new WaitForSecondsRealtime(1f);
		GameOptionWindow settings = manager.GetInstance<GameOptionWindow>();
		if (settings == null || !settings.IsOpen())
		{
			throw new Exception("Native settings did not open");
		}
		componentsInChildren = settings.GetComponentsInChildren<UILabel>(includeInactive: true);
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			string text = componentsInChildren[j].text ?? "";
			if (text.StartsWith("@") || text.StartsWith("OPTION_") || text == "CONTENT_GAMECONFIG" || text == "COMMON_ETC")
			{
				throw new Exception("Untranslated settings label: " + text);
			}
		}
		GameOptionBaseComponent[] rows = settings.GetComponentsInChildren<GameOptionBaseComponent>(includeInactive: true);
		if (rows.Length < 20)
		{
			throw new Exception("Native settings rows missing: " + rows.Length);
		}
		yield return VerifyLoginNotifications(rows);
		int num = 0;
		UISpriteAtlasAssigner[] componentsInChildren2 = settings.GetComponentsInChildren<UISpriteAtlasAssigner>(includeInactive: true);
		foreach (UISpriteAtlasAssigner uISpriteAtlasAssigner in componentsInChildren2)
		{
			if ((!(uISpriteAtlasAssigner.AtlasName != "GameOption") || !(uISpriteAtlasAssigner.AtlasName != "PayShop")) && !(uISpriteAtlasAssigner.Target == null))
			{
				if (uISpriteAtlasAssigner.Target.atlas == null || uISpriteAtlasAssigner.Target.atlas.GetSprite(uISpriteAtlasAssigner.SpriteName) == null)
				{
					throw new Exception("Missing settings artwork: " + uISpriteAtlasAssigner.SpriteName);
				}
				num++;
			}
		}
		if (num == 0)
		{
			throw new Exception("Settings artwork was not checked");
		}
		if (TableRoot.PatchDownloadComplete != patchReady)
		{
			throw new Exception("Settings changed download state");
		}
		if (settings.EtcInfo.Button_Coupon.gameObject.activeSelf || settings.EtcInfo.Button_Logout.gameObject.activeSelf)
		{
			throw new Exception("Lobby actions visible at login");
		}
		GameOptionBaseComponent gameOptionBaseComponent = Array.Find(rows, (GameOptionBaseComponent r) => r.gameOptionData != null && r.gameOptionData.PostProcessFunction == "SetTextureQuality");
		if (gameOptionBaseComponent == null)
		{
			throw new Exception("Native texture row missing");
		}
		ESpecType spec = SystemOptionData.Spec;
		ESpecType eSpecType = ((spec == ESpecType.Low) ? ESpecType.Mid : ESpecType.Low);
		gameOptionBaseComponent.SetTextureQuality(eSpecType);
		if (SystemOptionData.Spec != eSpecType)
		{
			throw new Exception("Login texture change failed");
		}
		gameOptionBaseComponent.SetTextureQuality(spec);
		GameOptionWindow.TabUIInfo[] mainTabs = settings.MainTabs;
		for (int num2 = 0; num2 < mainTabs.Length; num2++)
		{
			mainTabs[num2].Toggle.Set(state: true);
			yield return null;
		}
		settings.ReOpen();
		yield return null;
		settings.Close();
		yield return null;
		EventDelegate.Execute(button.onClick);
		yield return null;
		if (!settings.IsOpen() || settings.GetComponents<SprkLoginSettings.LoginContext>().Length != 1)
		{
			throw new Exception("Reopen or duplicate context");
		}
		string path = Environment.GetEnvironmentVariable("SPRK_LOGIN_SETTINGS_TEST");
		Directory.CreateDirectory(path);
		Camera[] allCameras = Camera.allCameras;
		foreach (Camera camera in allCameras)
		{
			if (camera.name == "BI UI Camera")
			{
				camera.gameObject.SetActive(value: false);
			}
		}
		yield return new WaitForSecondsRealtime(1f);
		yield return new WaitForEndOfFrame();
		List<string> list = new List<string>();
		Transform parent = settings.transform;
		while (parent != null)
		{
			list.Add(parent.name + " active=" + parent.gameObject.activeInHierarchy + " pos=" + parent.localPosition.ToString() + " scale=" + parent.localScale.ToString());
			parent = parent.parent;
		}
		UIPanel[] componentsInChildren3 = settings.GetComponentsInChildren<UIPanel>(includeInactive: true);
		foreach (UIPanel uIPanel in componentsInChildren3)
		{
			list.Add("Panel " + uIPanel.name + " alpha=" + uIPanel.alpha + " depth=" + uIPanel.depth + " active=" + uIPanel.gameObject.activeInHierarchy);
		}
		allCameras = Camera.allCameras;
		foreach (Camera camera2 in allCameras)
		{
			list.Add("Camera " + camera2.name + " mask=" + camera2.cullingMask + " depth=" + camera2.depth + " rect=" + camera2.rect.ToString());
		}
		File.WriteAllLines(Path.Combine(path, "diagnostics.txt"), list.ToArray());
		Texture2D texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		texture2D.Apply();
		File.WriteAllBytes(Path.Combine(path, "settings.png"), texture2D.EncodeToPNG());
		UnityEngine.Object.Destroy(texture2D);
		settings.GetComponent<SprkLoginSettings.LoginContext>();
		SprkLoginSettings.PrepareSettings(settings, null);
		if (settings.GetComponent<SprkLoginSettings.LoginContext>() != null || !settings.EtcInfo.Button_Coupon.gameObject.activeSelf)
		{
			throw new Exception("Login cleanup leaked into a reused in-game panel");
		}
		GameOptionWindow.TabUIInfo[] mainTabs2 = settings.MainTabs;
		foreach (GameOptionWindow.TabUIInfo tabUIInfo in mainTabs2)
		{
			if (tabUIInfo.GameOptionTab == GameOptionTab.Etc)
			{
				tabUIInfo.Toggle.Set(state: true);
			}
		}
		yield return null;
		for (int num3 = 0; num3 < 8; num3++)
		{
			SprkAccounts.CleanSettings(settings);
		}
		Transform parent2 = settings.EtcInfo.Button_Logout.parent;
		Transform transform = parent2.Find("Button_ChangeLanguage");
		Transform button_Coupon = settings.EtcInfo.Button_Coupon;
		Transform transform2 = parent2.Find("Button_ChangePassword");
		Transform transform3 = parent2.Find("Button_DeleteAccount");
		Transform transform4 = parent2.Find("Button_SwitchAccount");
		Transform button_Logout = settings.EtcInfo.Button_Logout;
		if (transform == null || transform2 == null || transform3 == null || transform4 == null)
		{
			throw new Exception("Others controls missing");
		}
		if (!(transform.localPosition.x < transform4.localPosition.x) || !(transform4.localPosition.x < button_Logout.localPosition.x) || button_Coupon.localPosition.x != transform.localPosition.x || button_Coupon.localPosition.y <= transform.localPosition.y)
		{
			throw new Exception("Coupon must be above Language");
		}
		if (transform2.localPosition.x != transform4.localPosition.x || transform3.localPosition.x != button_Logout.localPosition.x || transform2.localPosition.y <= transform4.localPosition.y || transform3.localPosition.y != transform2.localPosition.y || button_Coupon.localPosition.y != transform2.localPosition.y)
		{
			throw new Exception("Others upper row alignment");
		}
		UIButton component = transform3.GetComponent<UIButton>();
		UIButtonColor.State[] array = new UIButtonColor.State[3]
		{
			UIButtonColor.State.Normal,
			UIButtonColor.State.Hover,
			UIButtonColor.State.Pressed
		};
		for (int j = 0; j < array.Length; j++)
		{
			UIButtonColor.State state = array[j];
			component.SetState(state, immediate: true);
			UISprite component2 = component.tweenTarget.GetComponent<UISprite>();
			if (component2 == null || component2.spriteName != "Common_Button7_Cancel" || component2.atlas == null || component2.atlas.GetSprite(component2.spriteName) == null)
			{
				throw new Exception("Delete background is not red: " + state.ToString() + " target=" + component.tweenTarget.name + " sprite=" + ((component2 == null) ? "null" : component2.spriteName) + " atlas=" + ((component2 == null || component2.atlas == null) ? "null" : component2.atlas.name));
			}
		}
		component.SetState(UIButtonColor.State.Normal, immediate: true);
		Transform[] array2 = new Transform[3] { transform2, transform3, transform4 };
		for (int j = 0; j < array2.Length; j++)
		{
			if (array2[j].GetComponent<UIButton>().onClick.Count != 1)
			{
				throw new Exception("Duplicate account callbacks");
			}
		}
		yield return new WaitForEndOfFrame();
		texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		texture2D.Apply();
		File.WriteAllBytes(Path.Combine(path, "others.png"), texture2D.EncodeToPNG());
		UnityEngine.Object.Destroy(texture2D);
		File.WriteAllText(Path.Combine(path, "others-result.txt"), "PASS: native Others prefab Coupon above Language, upper alignment, eight refreshes, single callbacks and red Normal/Hover/Pressed backgrounds.");
		File.WriteAllText(Path.Combine(path, "result.txt"), "PASS: native login buttons cleaned, one Settings callback, native settings opened before login with " + rows.Length + " rows; translated labels, GameOption and PayShop artwork, all tabs, login texture changes, refresh, close/reopen, pool cleanup and download-state preservation; visible panel screenshot exported.");
		Application.Quit();
	}

	private IEnumerator VerifyLoginNotifications(GameOptionBaseComponent[] rows)
	{
		List<string> errors = new List<string>();
		Application.LogCallback capture = (message, stack, type) =>
		{
			if (message.Contains("Response error") || message.Contains("temporary data error"))
			{
				errors.Add(message);
			}
		};
		Application.logMessageReceived += capture;
		try
		{
			int checkedRows = 0;
			foreach (GameOptionBaseComponent row in rows)
			{
				string callback = row.gameOptionData?.PostProcessFunction;
				if (callback == null || !callback.StartsWith("Request") || !callback.EndsWith("NotificationValue"))
				{
					continue;
				}
				bool original = (bool)row.GetOptionValue(row.gameOptionData.ClassName, row.gameOptionData.FieldName);
				row.SetOptionValue(!original, null, callBack: false);
				yield return null;
				row.SetOptionValue(original, null, callBack: false);
				yield return null;
				checkedRows++;
			}
			yield return new WaitForSecondsRealtime(2f);
			if (checkedRows != 4 || errors.Count != 0)
			{
				throw new Exception("Login notification callbacks: rows=" + checkedRows + " errors=" + string.Join("; ", errors.ToArray()));
			}
		}
		finally
		{
			Application.logMessageReceived -= capture;
		}
	}
}
