using System.Collections;
using System.Text;
using LitJson;
using NVespa;
using NVespa.NGlobal;
using NVespa.NWeb;
using UnityEngine;
using UnityEngine.Networking;

public sealed class SprkStorageReporter : MonoBehaviour
{
	private bool busy;

	private float next;

	private string reported;

	private void Update()
	{
		if (!busy && !(Time.realtimeSinceStartup < next))
		{
			WebServiceRequester requester = SingletonUpdater<WebServiceManager>.instance.Requester;
			if (requester != null && !string.IsNullOrEmpty(requester.SessionKey) && !(requester.SessionKey == reported))
			{
				next = Time.realtimeSinceStartup + 60f;
				StartCoroutine(Report(requester.Host, requester.SessionKey));
			}
		}
	}

	private IEnumerator Report(string host, string session)
	{
		busy = true;
		try
		{
			string[] array = SprkStorageMigration.ValidateReceipt(SprkStorageMigration.Receipt).Split('\n');
			string s = "SessionKey=" + UnityWebRequest.EscapeURL(session) + "&Schema=1&Installation=" + array[1].Substring(13) + "&Files=" + array[3].Substring(6) + "&Preferences=" + array[4].Substring(12) + "&Source=" + array[5].Substring(7);
			using UnityWebRequest web = new UnityWebRequest(host.TrimEnd('/') + "/client/storage_status", "POST");
			web.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(s));
			web.downloadHandler = new DownloadHandlerBuffer();
			web.timeout = 15;
			web.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
			yield return web.SendWebRequest();
			if (web.isNetworkError || web.isHttpError)
			{
				yield break;
			}
			JsonData jsonData = JsonMapper.ToObject(web.downloadHandler.text);
			if (jsonData != null)
			{
				if (jsonData["Result"].valueStr == "Success")
				{
					reported = session;
				}
				jsonData.ReleaseAll();
			}
		}
		finally
		{
			busy = false;
		}
	}
}
