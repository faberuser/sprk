using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NUtil;

public class ResourceContainer : MonoBehaviour, IInspectorGuiDrawer
{
	[Serializable]
	public class ResourceType
	{
		public string m_name;

		public UnityEngine.Object m_resource;
	}

	public UnityEngine.Object m_default;

	[HideInInspector]
	public List<ResourceType> m_listResource = new List<ResourceType>();

	private Dictionary<string, UnityEngine.Object> _dicResource;

	public UnityEngine.Object GetResource(string i_name)
	{
		return GetFromDictionary(i_name);
	}

	private UnityEngine.Object GetFromList(string i_name)
	{
		for (int i = 0; i < m_listResource.Count; i++)
		{
			ResourceType resourceType = m_listResource[i];
			if (resourceType.m_name == i_name)
			{
				return resourceType.m_resource;
			}
		}
		return m_default;
	}

	private UnityEngine.Object GetFromDictionary(string i_name)
	{
		if (_dicResource == null)
		{
			_dicResource = new Dictionary<string, UnityEngine.Object>();
			for (int i = 0; i < m_listResource.Count; i++)
			{
				ResourceType resourceType = m_listResource[i];
				_dicResource.Add(resourceType.m_name, resourceType.m_resource);
			}
		}
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dicResource, i_name, m_default);
	}
}
