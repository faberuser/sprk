using NGame2.NNodeTree.NObject.NMotionTrail;
using NGameAsset.NCreature;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NObject;

public class MotionTrailObject : ObjectBase, IInspectorGuiDrawer
{
	public bool Emit;

	public CreateData CreateData;

	public AnimationSampleData SampleData;

	public LocalTimer LocalTimer;

	public Transform ParentTm;

	public int Layer;

	private TrailRender _render;

	public bool IsEmpty => _render.Trail.PointBuffer.Count < 2;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		FinalizeMotionTrail();
	}

	public override void ChainUpdate()
	{
		if (_render != null)
		{
			_render.ChainUpdate();
		}
	}

	public override void Visit(IVisitor visitor)
	{
	}

	public bool Open(CreateData createData, Transform parentTm, LocalTimer localTimer, int layer)
	{
		if (parentTm == null)
		{
			return false;
		}
		Builder builder = SingletonAuto<BuilderContainer>.instance.Get(createData.LinkHeroCode.Code);
		if (builder == null)
		{
			return false;
		}
		AnimationSampleData animationSampleData = builder.GetAnimationSampleData(createData.SampleName);
		if (animationSampleData == null)
		{
			return false;
		}
		Emit = true;
		CreateData = createData;
		SampleData = animationSampleData;
		LocalTimer = localTimer;
		ParentTm = parentTm;
		Layer = layer;
		_render = new TrailRender(this);
		if (!_render.Open())
		{
			return false;
		}
		return true;
	}

	public void FinalizeMotionTrail()
	{
		if (_render != null)
		{
			_render.Close();
			_render = null;
		}
	}

	public void SetFrame(float frame)
	{
		_render.SetFrame(Mathf.RoundToInt(frame * 1000f));
	}

	public override Transform FindTransform(string nodeName)
	{
		return base.transform;
	}

	protected override void OnCreateDerived()
	{
	}

	protected override void OnReleaseDerived()
	{
	}
}
