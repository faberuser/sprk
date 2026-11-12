using System.Collections.Generic;
using NShader;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView.NDamageOn3d;

public class RenderInstancePool : SceneAutoBehaviour<RenderInstancePool>
{
	protected class MemberData
	{
		internal Shader[] ShaderArray;

		internal Queue<RenderInstance>[] UnusedQueueArray;

		internal MemberData()
		{
			UnusedQueueArray = new Queue<RenderInstance>[7];
			ShaderArray = new Shader[7];
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(UnusedQueueArray); i++)
			{
				UnusedQueueArray[i] = new Queue<RenderInstance>();
				RenderInstance.EShaderType eShaderType = (RenderInstance.EShaderType)i;
				ShaderArray[i] = Shader.Find("Vespa/DamageDisplay/" + Vespa_String_Extension_Method.GetEnumString(eShaderType));
			}
		}

		internal void OnDestroy()
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(UnusedQueueArray); i++)
			{
				Queue<RenderInstance> queue = UnusedQueueArray[i];
				while (queue.Count > 0)
				{
					queue.Dequeue().Release();
				}
			}
		}
	}

	private static MaterialPropertyBlock propertyBlock;

	protected MemberData _member;

	public static MaterialPropertyBlock PropertyBlock
	{
		get
		{
			if (propertyBlock == null)
			{
				propertyBlock = new MaterialPropertyBlock();
			}
			return propertyBlock;
		}
		set
		{
			propertyBlock = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_ = PropertyBlock;
		_member = new MemberData();
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			_member.OnDestroy();
		}
	}

	public void UpdateCommonPropertyBlock(float extraAlpha)
	{
		PropertyBlock.SetFloat(Define.EXTRAALPHA_ID, extraAlpha);
		PropertyBlock.SetFloat(Define.GLOBALTIME_ID, GlobalTimer.time);
	}

	public RenderInstance CreateRender(RenderInstance.EShaderType shaderType)
	{
		Queue<RenderInstance> queue = _member.UnusedQueueArray[(int)shaderType];
		if (queue.Count > 0)
		{
			return queue.Dequeue();
		}
		Shader shader = _member.ShaderArray[(int)shaderType];
		return new RenderInstance(shaderType, shader);
	}

	public static void ReleaseRender(RenderInstance render)
	{
		if (!SceneAutoBehaviour<RenderInstancePool>.isValidInstance)
		{
			render.Release();
			return;
		}
		render.Reset();
		SceneAutoBehaviour<RenderInstancePool>.instance._member.UnusedQueueArray[(int)render.ShaderType].Enqueue(render);
	}
}
