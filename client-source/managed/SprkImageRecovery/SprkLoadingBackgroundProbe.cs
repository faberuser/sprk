using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NGame2.NUI.NHelper;
using NGame2.NUI.NWindow.NLoading;
using NGameAsset;
using UnityEngine;

public sealed class SprkLoadingBackgroundProbe : MonoBehaviour
{
	private string report;

	private GameObject viewObject;

	private readonly List<string> passed = new List<string>();

	private const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

	private static readonly MethodInfo initialize = typeof(LoadingGamePlayTipComponent).GetMethod("InitBackgroundImage", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo nativeAsset = typeof(LoadingGamePlayTipComponent).GetField("_backgroundImageAsset", BindingFlags.Instance | BindingFlags.NonPublic);

	public void Run(string path)
	{
		report = path;
		StartCoroutine(Observe(Check()));
	}

	private IEnumerator Observe(IEnumerator checks)
	{
		Exception ex;
		while (true)
		{
			object obj = null;
			bool flag = false;
			ex = null;
			try
			{
				flag = checks.MoveNext();
				if (flag)
				{
					obj = checks.Current;
				}
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
			if (ex != null || !flag)
			{
				break;
			}
			yield return obj;
		}
		string text = ((ex == null) ? "" : ex.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"")
			.Replace("\r", "")
			.Replace("\n", "\\n"));
		File.WriteAllText(report, "{\"success\":" + ((ex == null) ? "true" : "false") + ",\"checks\":[\"" + string.Join("\",\"", passed.ToArray()) + "\"],\"error\":\"" + text + "\"}");
		if (viewObject != null)
		{
			UnityEngine.Object.Destroy(viewObject);
		}
		Application.Quit();
	}

	private void Require(bool value, string name)
	{
		if (!value)
		{
			throw new InvalidOperationException(name);
		}
		passed.Add(name);
	}

	private IEnumerator Check()
	{
		viewObject = new GameObject("Loading tip test");
		GameObject gameObject = new GameObject("Background");
		gameObject.transform.SetParent(viewObject.transform, worldPositionStays: false);
		UITexture texture = gameObject.AddComponent<UITexture>();
		LoadingGamePlayTipComponent view = viewObject.AddComponent<LoadingGamePlayTipComponent>();
		view.BackgroundImageTexture = texture;
		initialize.Invoke(view, new object[1] { "OrvelTown" });
		Require(texture.mainTexture != null, "opaque before asynchronous completion");
		float deadline = Time.realtimeSinceStartup + 15f;
		while ((texture.mainTexture == null || texture.mainTexture == Texture2D.blackTexture) && Time.realtimeSinceStartup < deadline)
		{
			yield return null;
		}
		Require(texture.mainTexture != null && texture.mainTexture != Texture2D.blackTexture, "native background atlas loads");
		Texture original = texture.mainTexture;
		texture.mainTexture = null;
		yield return null;
		yield return null;
		Require(texture.mainTexture == original, "lost binding repaired without reloading");
		texture.GetComponent<PlainAssetHolder>().Close();
		AssetTexture assetTexture = new AssetTexture();
		assetTexture.Open("LoadingTip/GamePlayTip/SPRK_MISSING_TEST");
		nativeAsset.SetValue(view, assetTexture);
		SprkImageRecovery.WatchLoadingTip(view);
		Require(texture.mainTexture == Texture2D.blackTexture, "failed request has opaque fallback");
		deadline = Time.realtimeSinceStartup + 4f;
		while (Time.realtimeSinceStartup < deadline)
		{
			yield return null;
		}
		SprkLoadingBackground component = view.GetComponent<SprkLoadingBackground>();
		Require((int)typeof(SprkLoadingBackground).GetField("attempts", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(component) == 3, "failed requests retry exactly three times");
		object exhausted = nativeAsset.GetValue(view);
		deadline = Time.realtimeSinceStartup + 1f;
		while (Time.realtimeSinceStartup < deadline)
		{
			yield return null;
		}
		Require(exhausted == nativeAsset.GetValue(view), "exhausted request does not loop");
		initialize.Invoke(view, new object[1] { "OrvelTown" });
		deadline = Time.realtimeSinceStartup + 15f;
		while ((texture.mainTexture == null || texture.mainTexture == Texture2D.blackTexture) && Time.realtimeSinceStartup < deadline)
		{
			yield return null;
		}
		Require(texture.mainTexture != null && texture.mainTexture != Texture2D.blackTexture, "next loading screen recovers original art");
		viewObject.SetActive(value: false);
		typeof(LoadingGamePlayTipComponent).GetMethod("NVespa.NPrefab2.InitOnCreate.OnRelease", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, null);
		Require(nativeAsset.GetValue(view) == null && texture.mainTexture == null, "native pool release clears asset and binding");
		viewObject.SetActive(value: true);
		initialize.Invoke(view, new object[1] { "OrvelTown" });
		deadline = Time.realtimeSinceStartup + 15f;
		while ((texture.mainTexture == null || texture.mainTexture == Texture2D.blackTexture) && Time.realtimeSinceStartup < deadline)
		{
			yield return null;
		}
		Require(texture.mainTexture != null && texture.mainTexture != Texture2D.blackTexture, "pooled loading screen reloads art");
	}
}
