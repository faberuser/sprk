using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

public sealed class SprkEclipseGaugeProbe : MonoBehaviour
{
	public static string Status = "Not started";
	private EclipseKeyGaugeComponent gauge;
	private int exchange;
	private static readonly BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;

	public static void Begin()
	{
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" ||
			!(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE") ?? "").StartsWith("eclipse-gauge-"))
			throw new InvalidOperationException("Eclipse gauge probe requires an isolated eclipse-gauge profile at Login");
		if (Status == "Running") throw new InvalidOperationException("Gauge probe already running");
		Status = "Running";
		new GameObject("EclipseGaugeProbe").AddComponent<SprkEclipseGaugeProbe>().StartCoroutine("Drive");
	}

	private IEnumerator Drive()
	{
		IEnumerator test = Test();
		while (true)
		{
			object current = null;
			bool moved;
			try { moved = test.MoveNext(); if (moved) current = test.Current; }
			catch (Exception error) { Status = "FAIL: " + error.Message; break; }
			if (!moved) { Status = "PASS: static zero/partial/full gauge and animated sub-percent, partial, single/multiple ticket conversion and capped keys; completion fires once"; break; }
			yield return current;
		}
		if (gauge != null)
		{
			gauge.StopAllCoroutines();
			gauge.gameObject.SetActive(false);
			yield return null;
			Destroy(gauge.gameObject);
		}
		Destroy(gameObject);
	}

	private void Reset(int tickets, long points)
	{
		gauge.StopAllCoroutines();
		for (int i = 0; i < 5; i++)
		{
			var key = gauge.eclipseDummyKeys.Count > i ? gauge.eclipseDummyKeys[i] : Instantiate(gauge._eclipseDummyKey, gauge.Grid_Root.transform);
			key.gameObject.SetActive(true);
			// The fixture exercises gauge/ticket state, without registering particle
			// draw calls in Login's unrelated UI panel.
			foreach (var widget in key.GetComponentsInChildren<UIWidgetExtraRenderQ>(true)) widget.enabled = false;
			key.SetKeyState(i < tickets);
			if (gauge.eclipseDummyKeys.Count <= i) gauge.eclipseDummyKeys.Add(key);
		}
		gauge.RedMoonRoot.SetActive(true);
		typeof(EclipseKeyGaugeComponent).GetField("_maxEclipsePoint", Hidden).SetValue(gauge, exchange);
		typeof(EclipseKeyGaugeComponent).GetMethod("InitGauge", Hidden).Invoke(gauge, new object[] { points });
	}

	private void Check(int tickets, long points)
	{
		float expected = tickets == 5 ? 1f : (float)(Math.Truncate((double)points / exchange * 100) / 100);
		if (Mathf.Abs(gauge.Sprite_Gauge.fillAmount - expected) > 0.001f || gauge.eclipseDummyKeys.Count(key => !key.IsEmptyKey()) != tickets)
			throw new Exception("Wrong gauge: tickets=" + tickets + " points=" + points + " fill=" + gauge.Sprite_Gauge.fillAmount);
	}

	private IEnumerator Test()
	{
		NVespa.SingletonAuto<NClientData.LocalizationLoader>.instance.SetEnableLoadFromPatch(true);
		exchange = GameRoot.Instance.CurrencyTypeDataContainer.GetCurrencyData(CurrencyType.EclipsePoint).ExchangeCostValue;
		gauge = EclipseKeyGaugeComponent.PrefabAuto.Factory.Create(SceneAutoBehaviour<CommonManagement>.instance.transform);
		foreach (var widget in gauge.GetComponentsInChildren<UIWidgetExtraRenderQ>(true)) widget.enabled = false;
		foreach (var sample in new[] { Tuple.Create(0, 0L), Tuple.Create(0, 100L), Tuple.Create(0, 1400L), Tuple.Create(0, (long)exchange / 2), Tuple.Create(1, 0L), Tuple.Create(5, 0L) })
		{
			Reset(sample.Item1, sample.Item2); Check(sample.Item1, sample.Item2);
		}
		foreach (var sample in new[] { Tuple.Create(0, 0L, 100L), Tuple.Create(0, 0L, 1400L), Tuple.Create(0, 0L, (long)exchange / 2), Tuple.Create(0, 0L, (long)exchange), Tuple.Create(0, 0L, 2L * exchange + 100), Tuple.Create(4, (long)exchange - 100, 100L), Tuple.Create(5, 0L, 100L) })
		{
			Reset(sample.Item1, sample.Item2);
			int completed = 0;
			var routine = (IEnumerator)typeof(EclipseKeyGaugeComponent).GetMethod("coFillGauge", Hidden).Invoke(gauge, new object[] { sample.Item3, (Action)(() => completed++) });
			gauge.StartCoroutine(routine);
			float deadline = Time.realtimeSinceStartup + 20f;
			while (completed == 0 && Time.realtimeSinceStartup < deadline) yield return null;
			if (completed != 1) throw new Exception("Gauge animation did not complete exactly once: tickets=" + sample.Item1 + " gain=" + sample.Item3 + " fill=" + gauge.Sprite_Gauge.fillAmount);
			int tickets = Math.Min(5, sample.Item1 + (int)((sample.Item2 + sample.Item3) / exchange));
			Check(tickets, (sample.Item2 + sample.Item3) % exchange);
		}
	}
}
