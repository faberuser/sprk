using UnityEngine;

namespace NVespa;

public class MergeBounds
{
	public bool m_isValid;

	public Bounds m_bounds;

	public void Merge(Bounds i_bounds)
	{
		if (!m_isValid)
		{
			m_bounds = i_bounds;
			m_isValid = true;
		}
		else
		{
			m_bounds.SetMinMax(Vector3.Min(m_bounds.min, i_bounds.min), Vector3.Max(m_bounds.max, i_bounds.max));
		}
	}

	public void Merge(Vector3 i_pos)
	{
		if (!m_isValid)
		{
			m_bounds = new Bounds(i_pos, Vector3.zero);
			m_isValid = true;
		}
		else
		{
			m_bounds.SetMinMax(Vector3.Min(m_bounds.min, i_pos), Vector3.Max(m_bounds.max, i_pos));
		}
	}

	public void BlendMerge(Bounds i_bounds, float i_weight)
	{
		if (!m_isValid)
		{
			m_bounds = i_bounds;
			m_isValid = true;
		}
		else
		{
			Vector3 center = Vector3.Lerp(m_bounds.center, i_bounds.center, i_weight);
			Vector3 size = i_bounds.size * i_weight;
			Merge(new Bounds(center, size));
		}
	}
}
