using System;
using System.Collections.Generic;
using UnityEngine;

namespace NShader;

public class DataSet : MonoBehaviour
{
	[Serializable]
	public class Toon
	{
		public Color m_outlineColor;

		public float m_outlineWidth;
	}

	[Serializable]
	public class NamedBoolean : Tuple<string, bool>
	{
		public NamedBoolean(string i_name, bool i_value)
			: base(i_name, i_value)
		{
		}
	}

	[Serializable]
	public class NamedFloat : Tuple<string, float>
	{
		public NamedFloat(string i_name, float i_value)
			: base(i_name, i_value)
		{
		}
	}

	[Serializable]
	public class NamedColor : Tuple<string, Color>
	{
		public NamedColor(string i_name, Color i_value)
			: base(i_name, i_value)
		{
		}
	}

	[Serializable]
	public class TempValue
	{
		public List<NamedBoolean> m_listBoolean = new List<NamedBoolean>();

		public List<NamedFloat> m_listFloat = new List<NamedFloat>();

		public List<NamedColor> m_listColor = new List<NamedColor>();
	}

	public const string PrefabPath = "[0.GameShared]/Shader/Resources/Vespa/DataSet";

	public static DataSet instanceInternal;

	public Toon m_toon;

	public TempValue m_tempValue;

	public static DataSet instance
	{
		get
		{
			if (instanceInternal != null)
			{
				return instanceInternal;
			}
			GameObject gameObject = VespaResource.Load("[0.GameShared]/Shader/Resources/Vespa/DataSet", typeof(GameObject)) as GameObject;
			if (gameObject == null)
			{
				return null;
			}
			instanceInternal = gameObject.GetComponent<DataSet>();
			return instanceInternal;
		}
	}
}
