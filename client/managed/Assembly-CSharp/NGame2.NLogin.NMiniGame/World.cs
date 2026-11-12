using UnityEngine;

namespace NGame2.NLogin.NMiniGame;

public class World
{
	public static readonly Vector2 SONIA_WIDTH_HEIGHT = new Vector2(90f, 140f);

	public static readonly Vector2 OBSTACLE_WIDTH_HEIGHT_SHORT = new Vector2(50f, 60f);

	public static readonly Vector2 OBSTACLE_WIDTH_HEIGHT_LONG = new Vector2(50f, 120f);

	public static readonly Vector2 OBSTACLE_WIDTH_HEIGHT_ORC = new Vector2(130f, 100f);

	public const float MULTI_OBSTACLE_SPACE = 55f;

	public const float WINDOW_LEFT_X = -690f;

	public const float WINDOW_RIGHT_X = 690f;

	public const float WINDOW_TOP_Y = 156f;

	public const float WINDOW_BOTTOM_Y = -248f;

	public const float GRAVITY_VEL_Y_PER_SECOND = -1300f;

	public const float JUMP_VEL_Y_PER_SECOND = 800f;

	public const float VEL_X_PER_SECOND = -150f;

	public const float OBSTACLE_VEL_X_PER_SECOND = -350f;

	public const int ENTITY_ID_SONIA = 1;

	public const int ENTITY_TERRAIN = 100;

	public const int ENTITY_OBSTACLE = 10000;

	public const int TRIPLE_OBSTACLE_REQUIRE_SCROLL_X = -15000;
}
