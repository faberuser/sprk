using System.Collections.Generic;
using UnityEngine;

namespace NVespa;

public class UnityTimeScaler
{
	public enum EBaseTimeScale
	{
		Normal,
		x_1_5,
		x_2,
		x_5,
		x_10,
		x_0_5,
		x_0_2,
		x_0_1
	}

	private class ScaleEntry
	{
		public WeakRef<object> EventObj = new WeakRef<object>();

		public float TimeScale;
	}

	public static LocalAccountEnum<EBaseTimeScale> BaseTimeScale = new LocalAccountEnum<EBaseTimeScale>(EBaseTimeScale.Normal, typeof(UnityTimeScaler).FullName + "BaseTimeScale");

	private static List<ScaleEntry> _scaleList = new List<ScaleEntry>();

	public static void Reset()
	{
		_scaleList.Clear();
		UpdateUnityScale();
	}

	public static void SetScale(object key, float value)
	{
		if (key != null)
		{
			if (Mathf.Approximately(value, 1f))
			{
				RemoveEntry(key);
			}
			else
			{
				UpdateEntry(key, value);
			}
		}
	}

	private static void RemoveEntry(object key)
	{
		for (int i = 0; i < _scaleList.Count; i++)
		{
			if (_scaleList[i].EventObj.Value == key)
			{
				_scaleList.RemoveAt(i);
				UpdateUnityScale();
				break;
			}
		}
	}

	private static void UpdateEntry(object key, float timeScale)
	{
		for (int i = 0; i < _scaleList.Count; i++)
		{
			ScaleEntry scaleEntry = _scaleList[i];
			if (scaleEntry.EventObj.Value == key)
			{
				scaleEntry.TimeScale = timeScale;
				UpdateUnityScale();
				return;
			}
		}
		ScaleEntry scaleEntry2 = new ScaleEntry();
		scaleEntry2.EventObj.Value = key;
		scaleEntry2.TimeScale = timeScale;
		_scaleList.Add(scaleEntry2);
		UpdateUnityScale();
	}

	public static void UpdateUnityScale()
	{
		float num = 1f;
		for (int i = 0; i < _scaleList.Count; i++)
		{
			ScaleEntry scaleEntry = _scaleList[i];
			if (scaleEntry.EventObj.Value == null)
			{
				_scaleList.RemoveAt(i);
				i--;
			}
			else
			{
				num = ((i != 0) ? Mathf.Min(num, scaleEntry.TimeScale) : scaleEntry.TimeScale);
			}
		}
		if (BaseTimeScale.Value == EBaseTimeScale.Normal)
		{
			Time.timeScale = num;
		}
		else if (BaseTimeScale.Value == EBaseTimeScale.x_1_5)
		{
			Time.timeScale = num * 1.5f;
		}
		else if (BaseTimeScale.Value == EBaseTimeScale.x_2)
		{
			Time.timeScale = num * 2f;
		}
		else if (BaseTimeScale.Value == EBaseTimeScale.x_5)
		{
			Time.timeScale = num * 5f;
		}
		else if (BaseTimeScale.Value == EBaseTimeScale.x_10)
		{
			Time.timeScale = num * 10f;
		}
		else if (BaseTimeScale.Value == EBaseTimeScale.x_0_5)
		{
			Time.timeScale = num * 0.5f;
		}
		else if (BaseTimeScale.Value == EBaseTimeScale.x_0_2)
		{
			Time.timeScale = num * 0.2f;
		}
		else if (BaseTimeScale.Value == EBaseTimeScale.x_0_1)
		{
			Time.timeScale = num * 0.1f;
		}
		else
		{
			Time.timeScale = num;
		}
	}
}
