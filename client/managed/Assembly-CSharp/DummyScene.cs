using System.Collections.Generic;
using UnityEngine;

public class DummyScene : MonoBehaviour
{
	public string[] Hosts = new string[3] { "localhost:3000", "192.168.0.200:3000", "git-kr.vespainc.com:3000" };

	public List<DummyClient> DummyClients = new List<DummyClient>();

	private int _currentHostIndex;

	private int _currentDummyClientId;

	public string Host
	{
		get
		{
			if (Hosts == null || _currentHostIndex >= Hosts.Length)
			{
				return string.Empty;
			}
			return Hosts[_currentHostIndex];
		}
	}

	private void Awake()
	{
	}

	private void OnGUI()
	{
		GUILayout.BeginVertical();
		if (Hosts != null && Hosts.Length != 0)
		{
			GUILayout.Space(10f);
			if (GUILayout.Button("Host : " + Host))
			{
				_currentHostIndex = (_currentHostIndex + 1) % Hosts.Length;
			}
			GUILayout.Space(10f);
		}
		if (GUILayout.Button("Add Dummy"))
		{
			int id = ++_currentDummyClientId;
			DummyClient dummyClient = new GameObject("DummyClient" + id).AddComponent<DummyClient>();
			dummyClient.Init(Host, id);
			DummyClients.Add(dummyClient);
		}
		GUILayout.Space(10f);
		foreach (DummyClient dummyClient2 in DummyClients)
		{
			dummyClient2.DrawGui();
			GUILayout.Space(10f);
		}
		GUILayout.EndVertical();
	}
}
