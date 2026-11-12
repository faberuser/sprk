using System.Collections;
using System.Collections.Generic;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa.NCamera;

public class TargetTextureManager : GlobalOnceBehaviour<TargetTextureManager>
{
	private List<Tuple<Camera, RenderTexture>> _backupList = new List<Tuple<Camera, RenderTexture>>();

	private void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus)
		{
			BackupTargetTexture();
		}
		else
		{
			VespaCoroutine.Start(this, coRestoreTargetTexture());
		}
	}

	private void BackupTargetTexture()
	{
		Debug.Log("Backup TargetTexture begin");
		_backupList.Clear();
		Camera[] array = Object.FindObjectsOfType<Camera>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			Camera camera = array[i];
			if (!(camera == null) && !(camera.targetTexture == null))
			{
				_backupList.Add(Tuple.Create(camera, camera.targetTexture));
				camera.targetTexture = null;
			}
		}
		Debug.Log("Backup TargetTexture end");
	}

	private IEnumerator coRestoreTargetTexture()
	{
		Debug.Log("Restore TargetTexture begin");
		yield return null;
		float beginTime = Time.realtimeSinceStartup;
		int beginFrameCount = Time.frameCount;
		int tryCount = 0;
		while (!__Extension_Method_ICollection__.IsNullOrEmpty(_backupList))
		{
			yield return null;
			if (Time.realtimeSinceStartup - beginTime > 1f && Time.frameCount - beginFrameCount > 10)
			{
				_backupList.Clear();
				break;
			}
			tryCount++;
			Debug.Log("TryCount: " + tryCount);
			for (int i = 0; i < _backupList.Count; i++)
			{
				Tuple<Camera, RenderTexture> tuple = _backupList[i];
				Camera item = tuple.Item1;
				RenderTexture item2 = tuple.Item2;
				if (item == null || item2 == null)
				{
					_backupList.RemoveAt(i);
					i--;
				}
				else if (item.targetTexture != item2)
				{
					if (item.targetTexture == null)
					{
						item.targetTexture = item2;
					}
					_backupList.RemoveAt(i);
					i--;
				}
			}
		}
		Debug.Log("Restore TargetTexture end");
	}
}
