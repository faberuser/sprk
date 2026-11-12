using System.Collections.Generic;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView.NDamageOn3d;

public class RenderContainer
{
	private List<RenderInstance> _displayList = new List<RenderInstance>();

	public int Count => __Extension_Method_ICollection__.COUNT(_displayList);

	public void OnDestroy()
	{
		for (int i = 0; i < _displayList.Count; i++)
		{
			RenderInstancePool.ReleaseRender(_displayList[i]);
		}
		_displayList.Clear();
	}

	public int UpdateAndRender(int renderQueue)
	{
		float time = GlobalTimer.time;
		for (int i = 0; i < _displayList.Count; i++)
		{
			RenderInstance renderInstance = _displayList[i];
			if (renderInstance.EndTime < time)
			{
				RenderInstancePool.ReleaseRender(renderInstance);
				_displayList.RemoveAt(i);
				i--;
			}
			else
			{
				renderInstance.Render(renderQueue);
				renderQueue++;
			}
		}
		return renderQueue;
	}

	public void Add(RenderInstance.EShaderType shaderType, GeometryBuilder.GeometryData geometry, float durationTime)
	{
		RenderInstance renderInstance = null;
		if (_displayList.Count > 0)
		{
			renderInstance = _displayList[_displayList.Count - 1];
			if (renderInstance.ShaderType != shaderType || geometry.CreateTime - renderInstance.CreateTime > 1f)
			{
				renderInstance = null;
			}
		}
		if (renderInstance == null)
		{
			renderInstance = SceneAutoBehaviour<RenderInstancePool>.instance.CreateRender(shaderType);
			_displayList.Add(renderInstance);
		}
		renderInstance.pendingList.Add(geometry);
		renderInstance.EndTime = Mathf.Max(renderInstance.EndTime, geometry.CreateTime + durationTime);
	}
}
