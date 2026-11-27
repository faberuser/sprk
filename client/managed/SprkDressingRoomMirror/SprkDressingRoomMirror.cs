using System.Collections.Generic;
using NGame2.NUI.NRender3D;
using UnityEngine;

// Turns the standing mirror of the Dressing Room (Creature_NPC_WardrobeBG, mesh "Wardrobe_Reflection")
// into a real mirror. The asset only has a painted gradient; we replace its _MainTex with a live
// render of the scene seen through the mirror.
//
// How: a hidden camera sits at the viewer's position reflected through the mirror plane and looks
// through the mirror rectangle with an off-axis ("portal") frustum whose near plane is the mirror.
// It renders into the sub-rectangle of a render texture that the mirror's UVs cover, so the game's
// own shader samples the reflection through the mesh UVs and no custom shader is needed.
public static class SprkDressingRoomMirror
{
	private const string MirrorName = "Wardrobe_Reflection";

	private const float RescanSeconds = 0.5f;

	// Views without the wardrobe (Heroes, See Motions) stop searching after this long.
	private const float ScanSeconds = 15f;

	// Keeps the mirror quad itself (and anything touching the wall) out of its own reflection.
	private const float NearOffset = 0.002f;

	// 1 stands the mirror fully upright, 0 keeps the shipped 15 degree lean.
	private const float UprightFraction = 1f;

	// How far around the glass (sides, bottom) and above it the frame and stand reach.
	private const float FrameMargin = 0.15f;

	private const float FrameTop = 0.7f;

	private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

	// Fallback mirror rectangle in mesh space, read from the shipped asset, for a non-readable mesh.
	private static readonly Vector3 FallbackA = new Vector3(-1.80666649f, 3.57129502f, 0.0454295352f);

	private static readonly Vector3 FallbackB = new Vector3(-2.62929249f, 3.09635186f, 0.0454295352f);

	private static readonly Vector3 FallbackC = new Vector3(-2.09675574f, 4.07374477f, 2.21068668f);

	private static readonly Rect FallbackUv = Rect.MinMaxRect(0.0469198264f, 0.0344949588f, 0.50298059f, 0.97617209f);

	private sealed class State
	{
		public RenderTargetCinema Cinema;

		public Camera Source;

		public MeshRenderer Mirror;

		// Mirror corners in mesh space: A = (uMin, vMin), B = (uMax, vMin), C = (uMin, vMax).
		public Vector3 A;

		public Vector3 B;

		public Vector3 C;

		public Rect Uv;

		public Texture OriginalTexture;

		public Camera Reflection;

		public RenderTexture Target;

		public MaterialPropertyBlock Block;

		public float NextScan;

		public float ScanUntil;

		public bool Reported;

		public readonly List<Swap> Upright = new List<Swap>();
	}

	private sealed class Swap
	{
		public MeshFilter Filter;

		public Mesh Original;

		public Mesh Copy;
	}

	private static readonly List<State> states = new List<State>();

	private static bool hooked;

	private static bool rendering;

	// Called every LateUpdate by the hero model views (Dressing Room, Heroes, See Motions...).
	public static void Track(RenderTargetCinema cinema)
	{
		if (cinema == null || cinema.MainCamera == null)
		{
			return;
		}
		State state = Find(cinema);
		if (state == null)
		{
			state = new State
			{
				Cinema = cinema,
				Block = new MaterialPropertyBlock(),
				ScanUntil = Time.unscaledTime + ScanSeconds
			};
			states.Add(state);
			Hook();
		}
		Camera source = cinema.MainCamera;
		state.Source = source;
		if (state.Mirror != null)
		{
			if (IsDrawnBy(state.Mirror, source))
			{
				return;
			}
			// The wardrobe went back to its pool or the scene changed: look again for a while.
			Unbind(state);
			state.ScanUntil = Time.unscaledTime + ScanSeconds;
		}
		state.Mirror = null;
		float now = Time.unscaledTime;
		if (now < state.NextScan || now > state.ScanUntil)
		{
			if (now > state.ScanUntil && !state.Reported)
			{
				state.Reported = true;
				string candidates = Describe(null);
				if (candidates != "none")
				{
					Debug.LogWarning("SprkDressingRoomMirror: no " + MirrorName + " drawn by " + source.name + " (mask " + source.cullingMask + "); candidates: " + candidates);
				}
			}
			return;
		}
		state.NextScan = now + RescanSeconds;
		// The room is built by the cinema's node tree, which does not parent its models under the
		// cinema, so search the scene for the mirror on a layer this cinema's camera draws.
		MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
		for (int i = 0; i < renderers.Length; i++)
		{
			MeshRenderer renderer = renderers[i];
			if (renderer.name == MirrorName && IsDrawnBy(renderer, source) && Bind(state, renderer))
			{
				Debug.LogWarning("SprkDressingRoomMirror: bound " + Describe(renderer) + " to " + source.name);
				break;
			}
		}
	}

	private static bool IsDrawnBy(MeshRenderer renderer, Camera camera)
	{
		return renderer.gameObject.activeInHierarchy && (camera.cullingMask & (1 << renderer.gameObject.layer)) != 0;
	}

	private static string Describe(MeshRenderer only)
	{
		System.Text.StringBuilder text = new System.Text.StringBuilder();
		MeshRenderer[] renderers = (only != null) ? new MeshRenderer[1] { only } : Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < renderers.Length; i++)
		{
			MeshRenderer renderer = renderers[i];
			if (renderer.name != MirrorName)
			{
				continue;
			}
			string path = renderer.name;
			for (Transform parent = renderer.transform.parent; parent != null; parent = parent.parent)
			{
				path = parent.name + "/" + path;
			}
			Material material = renderer.sharedMaterial;
			text.Append(path).Append(" layer=").Append(renderer.gameObject.layer).Append(" active=").Append(renderer.gameObject.activeInHierarchy)
				.Append(" shader=").Append((material != null && material.shader != null) ? material.shader.name : "none").Append("; ");
		}
		return (text.Length == 0) ? "none" : text.ToString();
	}

	// Called when the cinema closes.
	public static void Release(RenderTargetCinema cinema)
	{
		for (int i = states.Count - 1; i >= 0; i--)
		{
			State state = states[i];
			if (state.Cinema == null || state.Cinema == cinema)
			{
				Unbind(state);
				if (state.Reflection != null)
				{
					Object.Destroy(state.Reflection.gameObject);
				}
				if (state.Target != null)
				{
					state.Target.Release();
					Object.Destroy(state.Target);
				}
				states.RemoveAt(i);
			}
		}
		if (states.Count == 0 && hooked)
		{
			Camera.onPreCull -= OnPreCull;
			hooked = false;
		}
	}

	private static State Find(RenderTargetCinema cinema)
	{
		for (int i = 0; i < states.Count; i++)
		{
			if (states[i].Cinema == cinema)
			{
				return states[i];
			}
		}
		return null;
	}

	private static void Hook()
	{
		if (!hooked)
		{
			Camera.onPreCull += OnPreCull;
			hooked = true;
		}
	}

	private static bool Bind(State state, MeshRenderer renderer)
	{
		StandUpright(state, renderer);
		MeshFilter filter = renderer.GetComponent<MeshFilter>();
		Mesh mesh = (filter != null) ? filter.sharedMesh : null;
		if (!ReadCorners(mesh, state))
		{
			state.A = FallbackA;
			state.B = FallbackB;
			state.C = FallbackC;
			state.Uv = FallbackUv;
		}
		state.Mirror = renderer;
		Material material = renderer.sharedMaterial;
		state.OriginalTexture = (material != null && material.HasProperty(MainTexId)) ? material.GetTexture(MainTexId) : null;
		return true;
	}

	private static bool ReadCorners(Mesh mesh, State state)
	{
		if (mesh == null || !mesh.isReadable)
		{
			return false;
		}
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv;
		if (vertices.Length < 3 || uv.Length != vertices.Length)
		{
			return false;
		}
		Vector2 min = uv[0];
		Vector2 max = uv[0];
		for (int i = 1; i < uv.Length; i++)
		{
			min = Vector2.Min(min, uv[i]);
			max = Vector2.Max(max, uv[i]);
		}
		if (max.x - min.x < 1e-4f || max.y - min.y < 1e-4f)
		{
			return false;
		}
		state.A = Closest(vertices, uv, new Vector2(min.x, min.y));
		state.B = Closest(vertices, uv, new Vector2(max.x, min.y));
		state.C = Closest(vertices, uv, new Vector2(min.x, max.y));
		state.Uv = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
		return Vector3.Cross(state.B - state.A, state.C - state.A).sqrMagnitude > 1e-8f;
	}

	private static Vector3 Closest(Vector3[] vertices, Vector2[] uv, Vector2 target)
	{
		int best = 0;
		float bestDistance = float.MaxValue;
		for (int i = 0; i < uv.Length; i++)
		{
			float distance = (uv[i] - target).sqrMagnitude;
			if (distance < bestDistance)
			{
				bestDistance = distance;
				best = i;
			}
		}
		return vertices[best];
	}

	// The shipped mirror leans back about 15 degrees, so it mostly reflects the ceiling. Rotate the
	// mirror and its frame (separate pieces of the room's furniture mesh) about the line where the
	// stand meets the floor until it stands upright. Copies of the meshes are edited and the
	// originals are put back in Unbind.
	private static void StandUpright(State state, MeshRenderer mirror)
	{
		MeshFilter mirrorFilter = mirror.GetComponent<MeshFilter>();
		Mesh mirrorMesh = (mirrorFilter != null) ? mirrorFilter.sharedMesh : null;
		if (UprightFraction <= 0f || mirrorMesh == null || !ReadCorners(mirrorMesh, state) || mirror.transform.parent == null)
		{
			return;
		}
		Transform mirrorTransform = mirror.transform;
		Vector3 up = mirrorTransform.InverseTransformDirection(Vector3.up).normalized;
		Vector3 axis = (state.B - state.A).normalized;
		if (Mathf.Abs(Vector3.Dot(axis, up)) > 0.1f)
		{
			return;
		}
		Vector3 normal = Vector3.Cross(state.B - state.A, state.C - state.A).normalized;
		Vector3 level = normal - Vector3.Dot(normal, up) * up;
		if (level.sqrMagnitude < 1e-4f)
		{
			return;
		}
		float angle = Vector3.SignedAngle(normal, level.normalized, axis) * UprightFraction;
		if (Mathf.Abs(angle) < 0.5f)
		{
			return;
		}
		// Everything fully inside this box around the glass belongs to the mirror (frame, stand, studs).
		Bounds box = mirrorMesh.bounds;
		box.Expand(2f * FrameMargin);
		box.Encapsulate(box.max + up * FrameTop);
		MeshFilter[] filters = mirrorTransform.parent.GetComponentsInChildren<MeshFilter>(includeInactive: true);
		List<Vector3[]> selected = new List<Vector3[]>();
		List<MeshFilter> selectedFilters = new List<MeshFilter>();
		List<bool[]> selectedMasks = new List<bool[]>();
		float lowest = float.MaxValue;
		for (int i = 0; i < filters.Length; i++)
		{
			MeshFilter filter = filters[i];
			Mesh mesh = filter.sharedMesh;
			if (mesh == null || !mesh.isReadable)
			{
				continue;
			}
			Matrix4x4 toMirror = mirrorTransform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
			Vector3[] vertices = mesh.vertices;
			for (int v = 0; v < vertices.Length; v++)
			{
				vertices[v] = toMirror.MultiplyPoint3x4(vertices[v]);
			}
			bool[] mask = (filter == mirrorFilter) ? All(vertices.Length) : SelectInside(mesh, vertices, box);
			if (mask == null)
			{
				continue;
			}
			for (int v = 0; v < vertices.Length; v++)
			{
				if (mask[v])
				{
					lowest = Mathf.Min(lowest, Vector3.Dot(vertices[v], up));
				}
			}
			selected.Add(vertices);
			selectedFilters.Add(filter);
			selectedMasks.Add(mask);
		}
		if (selectedFilters.Count < 2)
		{
			// Only the glass was found; tilting it out of its frame would look broken.
			return;
		}
		Vector3 pivot = state.A + up * (lowest - Vector3.Dot(state.A, up));
		Quaternion rotation = Quaternion.AngleAxis(angle, axis);
		for (int i = 0; i < selectedFilters.Count; i++)
		{
			MeshFilter filter = selectedFilters[i];
			Mesh original = filter.sharedMesh;
			Matrix4x4 fromMirror = filter.transform.worldToLocalMatrix * mirrorTransform.localToWorldMatrix;
			Matrix4x4 toMirror = mirrorTransform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
			Vector3[] vertices = selected[i];
			bool[] mask = selectedMasks[i];
			Vector3[] normals = original.normals;
			Vector4[] tangents = original.tangents;
			Vector3[] local = original.vertices;
			for (int v = 0; v < vertices.Length; v++)
			{
				if (!mask[v])
				{
					continue;
				}
				local[v] = fromMirror.MultiplyPoint3x4(pivot + rotation * (vertices[v] - pivot));
				if (v < normals.Length)
				{
					normals[v] = fromMirror.MultiplyVector(rotation * toMirror.MultiplyVector(normals[v])).normalized;
				}
				if (v < tangents.Length)
				{
					Vector3 tangent = fromMirror.MultiplyVector(rotation * toMirror.MultiplyVector(tangents[v])).normalized;
					tangents[v] = new Vector4(tangent.x, tangent.y, tangent.z, tangents[v].w);
				}
			}
			Mesh copy = Object.Instantiate(original);
			copy.name = original.name + " (SPRK upright)";
			copy.hideFlags = HideFlags.HideAndDontSave;
			copy.vertices = local;
			if (normals.Length == local.Length)
			{
				copy.normals = normals;
			}
			if (tangents.Length == local.Length)
			{
				copy.tangents = tangents;
			}
			copy.RecalculateBounds();
			filter.sharedMesh = copy;
			state.Upright.Add(new Swap
			{
				Filter = filter,
				Original = original,
				Copy = copy
			});
		}
	}

	private static bool[] All(int count)
	{
		bool[] mask = new bool[count];
		for (int i = 0; i < count; i++)
		{
			mask[i] = true;
		}
		return mask;
	}

	// Marks the vertices of every connected piece of the mesh that lies entirely inside the box.
	private static bool[] SelectInside(Mesh mesh, Vector3[] vertices, Bounds box)
	{
		int count = vertices.Length;
		int[] parent = new int[count];
		for (int i = 0; i < count; i++)
		{
			parent[i] = i;
		}
		int[] triangles = mesh.triangles;
		for (int t = 0; t + 2 < triangles.Length; t += 3)
		{
			Union(parent, triangles[t], triangles[t + 1]);
			Union(parent, triangles[t + 1], triangles[t + 2]);
		}
		// UV seams split vertices; pieces that share positions are one piece.
		Dictionary<Vector3Int, int> positions = new Dictionary<Vector3Int, int>();
		for (int i = 0; i < count; i++)
		{
			Vector3 v = vertices[i] * 10000f;
			Vector3Int key = new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
			if (positions.TryGetValue(key, out int other))
			{
				Union(parent, i, other);
			}
			else
			{
				positions[key] = i;
			}
		}
		Dictionary<int, bool> inside = new Dictionary<int, bool>();
		for (int i = 0; i < count; i++)
		{
			int root = Find(parent, i);
			bool contained = box.Contains(vertices[i]);
			inside[root] = (!inside.TryGetValue(root, out bool previous) || previous) && contained;
		}
		bool[] mask = new bool[count];
		bool any = false;
		for (int i = 0; i < count; i++)
		{
			mask[i] = inside[Find(parent, i)];
			any |= mask[i];
		}
		return any ? mask : null;
	}

	private static int Find(int[] parent, int i)
	{
		while (parent[i] != i)
		{
			parent[i] = parent[parent[i]];
			i = parent[i];
		}
		return i;
	}

	private static void Union(int[] parent, int a, int b)
	{
		int rootA = Find(parent, a);
		int rootB = Find(parent, b);
		if (rootA != rootB)
		{
			parent[rootA] = rootB;
		}
	}

	private static void RestoreMeshes(State state)
	{
		for (int i = 0; i < state.Upright.Count; i++)
		{
			Swap swap = state.Upright[i];
			if (swap.Filter != null && swap.Filter.sharedMesh == swap.Copy)
			{
				swap.Filter.sharedMesh = swap.Original;
			}
			if (swap.Copy != null)
			{
				Object.Destroy(swap.Copy);
			}
		}
		state.Upright.Clear();
	}

	private static void Unbind(State state)
	{
		RestoreMeshes(state);
		MeshRenderer mirror = state.Mirror;
		if (mirror != null)
		{
			// Only our texture is removed; anything else the game put in the block stays.
			mirror.GetPropertyBlock(state.Block);
			if (state.OriginalTexture != null)
			{
				state.Block.SetTexture(MainTexId, state.OriginalTexture);
			}
			mirror.SetPropertyBlock(state.Block);
		}
		state.Mirror = null;
		state.OriginalTexture = null;
	}

	private static void OnPreCull(Camera camera)
	{
		if (rendering)
		{
			return;
		}
		for (int i = 0; i < states.Count; i++)
		{
			State state = states[i];
			if (state.Source == camera)
			{
				if (state.Cinema == null)
				{
					return;
				}
				Render(state, camera);
				return;
			}
		}
	}

	private static void Render(State state, Camera source)
	{
		MeshRenderer mirror = state.Mirror;
		if (mirror == null || !mirror.enabled || !mirror.gameObject.activeInHierarchy)
		{
			return;
		}
		Matrix4x4 localToWorld = mirror.localToWorldMatrix;
		Vector3 a = localToWorld.MultiplyPoint3x4(state.A);
		Vector3 b = localToWorld.MultiplyPoint3x4(state.B);
		Vector3 c = localToWorld.MultiplyPoint3x4(state.C);
		Vector3 right = b - a;
		float width = right.magnitude;
		if (width < 1e-4f)
		{
			return;
		}
		right /= width;
		Vector3 up = c - a;
		up -= Vector3.Dot(up, right) * right;
		float height = up.magnitude;
		if (height < 1e-4f)
		{
			return;
		}
		up /= height;
		Vector3 normal = Vector3.Cross(right, up);
		Vector3 eye = source.transform.position;
		float side = Vector3.Dot(eye - a, normal);
		float distance = Mathf.Abs(side);
		if (distance < 1e-3f)
		{
			return;
		}
		// The viewer reflected through the mirror plane looks back through the mirror rectangle.
		Vector3 virtualEye = eye - 2f * side * normal;
		Vector3 back = (side > 0f) ? -normal : normal;
		Matrix4x4 view = Matrix4x4.identity;
		view.SetRow(0, new Vector4(right.x, right.y, right.z, 0f - Vector3.Dot(right, virtualEye)));
		view.SetRow(1, new Vector4(up.x, up.y, up.z, 0f - Vector3.Dot(up, virtualEye)));
		view.SetRow(2, new Vector4(back.x, back.y, back.z, 0f - Vector3.Dot(back, virtualEye)));
		view.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
		// Unity view matrices are right-handed (determinant -1 against the left-handed world).
		// A mirrored basis renders with reversed winding, so culling must be inverted.
		bool mirrored = Vector3.Dot(Vector3.Cross(right, up), back) > 0f;
		float near = distance + NearOffset;
		float far = Mathf.Max(source.farClipPlane, near + 1f) + distance;
		float scale = near / distance;
		Vector3 toCorner = a - virtualEye;
		float left = Vector3.Dot(right, toCorner) * scale;
		float bottom = Vector3.Dot(up, toCorner) * scale;
		Matrix4x4 projection = Frustum(left, left + width * scale, bottom, bottom + height * scale, near, far);

		Camera reflection = EnsureCamera(state, source);
		RenderTexture target = EnsureTarget(state, source);
		reflection.cullingMask = source.cullingMask;
		reflection.clearFlags = source.clearFlags;
		reflection.backgroundColor = source.backgroundColor;
		reflection.allowHDR = source.allowHDR;
		reflection.allowMSAA = source.allowMSAA;
		reflection.targetTexture = target;
		reflection.rect = state.Uv;
		reflection.transform.position = virtualEye;
		reflection.transform.rotation = Quaternion.LookRotation(-back, up);
		reflection.worldToCameraMatrix = view;
		reflection.projectionMatrix = projection;

		bool invertCulling = GL.invertCulling;
		rendering = true;
		try
		{
			GL.invertCulling = mirrored != invertCulling;
			reflection.Render();
		}
		finally
		{
			GL.invertCulling = invertCulling;
			rendering = false;
		}

		// Re-applied each frame: the game's render groups may rewrite the renderer's block.
		mirror.GetPropertyBlock(state.Block);
		state.Block.SetTexture(MainTexId, target);
		mirror.SetPropertyBlock(state.Block);
	}

	private static Matrix4x4 Frustum(float left, float right, float bottom, float top, float near, float far)
	{
		Matrix4x4 m = Matrix4x4.zero;
		m[0, 0] = 2f * near / (right - left);
		m[0, 2] = (right + left) / (right - left);
		m[1, 1] = 2f * near / (top - bottom);
		m[1, 2] = (top + bottom) / (top - bottom);
		m[2, 2] = (0f - (far + near)) / (far - near);
		m[2, 3] = -2f * far * near / (far - near);
		m[3, 2] = -1f;
		return m;
	}

	private static Camera EnsureCamera(State state, Camera source)
	{
		if (state.Reflection == null)
		{
			GameObject go = new GameObject("SprkDressingRoomMirror");
			go.hideFlags = HideFlags.HideAndDontSave;
			Camera camera = go.AddComponent<Camera>();
			camera.CopyFrom(source);
			camera.enabled = false;
			state.Reflection = camera;
		}
		return state.Reflection;
	}

	private static RenderTexture EnsureTarget(State state, Camera source)
	{
		// The mirror fills about half the texture width and its full height.
		int size = (source.pixelHeight > 1200) ? 2048 : 1024;
		if (state.Target != null && state.Target.width == size)
		{
			return state.Target;
		}
		if (state.Target != null)
		{
			state.Target.Release();
			Object.Destroy(state.Target);
		}
		RenderTexture sourceTarget = source.targetTexture;
		RenderTextureDescriptor descriptor = (sourceTarget != null) ? sourceTarget.descriptor : new RenderTextureDescriptor(size, size, RenderTextureFormat.Default, 24);
		descriptor.width = size;
		descriptor.height = size;
		descriptor.useMipMap = false;
		descriptor.autoGenerateMips = false;
		if (descriptor.depthBufferBits < 16)
		{
			descriptor.depthBufferBits = 24;
		}
		state.Target = new RenderTexture(descriptor)
		{
			name = "SprkDressingRoomMirror",
			hideFlags = HideFlags.HideAndDontSave,
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Bilinear
		};
		state.Target.Create();
		return state.Target;
	}
}
