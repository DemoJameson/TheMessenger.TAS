using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheMessenger.TAS.Components.Trainers.Hitboxes;

public class HitboxesRenderer : MonoBehaviour {
    // ReSharper disable once StructCanBeMadeReadOnly
    private struct HitboxType : IComparable<HitboxType> {
        public static readonly HitboxType Player = new(Color.green, 0);
        public static readonly HitboxType Enemy = new(Color.red, 1);
        public static readonly HitboxType Attack = new(Color.yellow, 2);
        public static readonly HitboxType OneWayJumpThrough = new(Color.cyan, 3);
        public static readonly HitboxType Trigger = new(new Color(0.5f, 0.5f, 1f), 4);
        public static readonly HitboxType Breakable = new(new Color(0.5f, 0.75f, 1f), 5);
        public static readonly HitboxType Hittable = new(Color.yellow, 6);
        public static readonly HitboxType Collectable = new(new Color(0.25f, 0.75f, 0.5f), 7);
        public static readonly HitboxType NotClimbable = new(new Color(0.8f, 0.5f, 1f), 8);
        public static readonly HitboxType Quicksand = new(new Color(1f, 0.75f, 0.25f), 9);
        public static readonly HitboxType Terrain = new(new Color(1f, 0.5f, 0.25f), 10);
        public static readonly HitboxType Other = new(Color.white, 11);

        public readonly Color Color;
        public readonly int Depth;

        private HitboxType(Color color, int depth) {
            Color = color;
            Depth = depth;
        }

        public int CompareTo(HitboxType other) {
            return other.Depth.CompareTo(Depth);
        }
    }

    private readonly SortedDictionary<HitboxType, HashSet<Collider2D>> colliders = new() {
        {HitboxType.Player, new HashSet<Collider2D>()},
        {HitboxType.Enemy, new HashSet<Collider2D>()},
        {HitboxType.Attack, new HashSet<Collider2D>()},
        {HitboxType.OneWayJumpThrough, new HashSet<Collider2D>()},
        {HitboxType.Trigger, new HashSet<Collider2D>()},
        {HitboxType.Breakable, new HashSet<Collider2D>()},
        {HitboxType.Hittable, new HashSet<Collider2D>()},
        {HitboxType.Collectable, new HashSet<Collider2D>()},
        {HitboxType.NotClimbable, new HashSet<Collider2D>()},
        {HitboxType.Quicksand, new HashSet<Collider2D>()},
        {HitboxType.Terrain, new HashSet<Collider2D>()},
        {HitboxType.Other, new HashSet<Collider2D>()},
    };

    private static float LineWidth => Mathf.Max(1, Screen.width / 960f);

    private void Start() {
        foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects()) {
            AddHitboxes(rootGameObject);
        }
    }

    public void AddHitboxes(GameObject go) {
        Collider2D[] components = go.name == "Player"
            ? go.GetComponents<Collider2D>()
            : go.GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D col in components) {
            AddHitbox(col);
        }
    }

    private void AddHitbox(Collider2D collider2D) {
        if (collider2D == null) {
            return;
        }

        if (collider2D is BoxCollider2D or PolygonCollider2D or EdgeCollider2D or CircleCollider2D) {
            GameObject go = collider2D.gameObject;

            int layer = go.layer;
            if (layer == LayerConstants.PLAYER) {
                colliders[HitboxType.Player].Add(collider2D);
            } else if (go.GetComponent<PlayerAttackHitZone>() || go.GetComponent<Shuriken>() || go.GetComponent<WindmillShuriken>() ||
                       go.GetComponent<GraplouTarget>()) {
                colliders[HitboxType.Attack].Add(collider2D);
            } else if (go.GetComponent<BreakableCollision>()) {
                colliders[HitboxType.Breakable].Add(collider2D);
            } else if (go.GetComponent<Hittable>()) {
                colliders[HitboxType.Hittable].Add(collider2D);
            } else if (go.GetComponent<Collectible>()) {
                colliders[HitboxType.Collectable].Add(collider2D);
            } else if (layer is Layers.LAVA_8 or Layers.LAVA_16 || go.GetComponent<HurtZone>()) {
                colliders[HitboxType.Enemy].Add(collider2D);
            } else if (layer is Layers.QUICKSAND_8 or Layers.QUICKSAND_16) {
                colliders[HitboxType.Quicksand].Add(collider2D);
            } else if (layer is Layers.GROUND_8 or Layers.GROUND_16 or Layers.MOVING_COLLISION_8 or Layers.MOVING_COLLISION_16) {
                if (go.name.Contains("OneWay")) {
                    colliders[HitboxType.OneWayJumpThrough].Add(collider2D);
                } else if (go.tag.Contains("NotClimbable")) {
                    colliders[HitboxType.NotClimbable].Add(collider2D);
                } else {
                    colliders[HitboxType.Terrain].Add(collider2D);
                }
            } else if (layer is Layers.WATER_8 or Layers.WATER_16 || go.GetComponent<AirGeyser>() || go.GetComponent<OnTriggerExitHandler>() || go.GetComponent<Cutscene>()) {
                colliders[HitboxType.Trigger].Add(collider2D);
            } else if (layer == LayerConstants.SPAWN_TRIGGER || go.GetComponent<ObjectSpawner>() || go.GetComponentInParent<ObjectSpawner>()) {
                // colliders[HitboxType.Other].Add(collider2D);
            } else {
                colliders[HitboxType.Other].Add(collider2D);
            }
        }
    }

    private void OnGUI() {
        if (HitboxesController.State == HitboxState.Hide || Event.current?.type != EventType.Repaint || Camera.main is not { } camera) {
            return;
        }

        int origDepth = GUI.depth;
        float lineWidth = LineWidth;
        foreach (var pair in colliders) {
            GUI.depth = pair.Key.Depth;
            foreach (Collider2D collider2D in pair.Value) {
                if (HitboxesController.State == HitboxState.ShowMost && pair.Key.Equals(HitboxType.Other)) {
                    continue;
                }

                DrawHitbox(camera, collider2D, pair.Key, lineWidth);
            }

            if (pair.Key.Equals(HitboxType.Attack)) {
                DrawRopeDart(camera, lineWidth);
            }
        }

        GUI.depth = origDepth;
    }

    private void DrawRopeDart(Camera camera, float lineWidth) {
        if (Manager<PlayerManager>.instance is {player.graplou: {thrown: true} graplou} && graplou.GetComponentInChildren<SpriteRenderer>() is {} renderer) {
            Vector2 offset = Vector2.right * 2.6f;
            Vector2 halfSize = renderer.bounds.size / 2f;
            Vector2 topLeft = new Vector2(-halfSize.x, halfSize.y) + offset;
            Vector2 topRight = halfSize + offset;
            Vector2 bottomRight = new Vector2(halfSize.x, -halfSize.y) + offset;
            Vector2 bottomLeft = -halfSize + offset;
            List<Vector2> points = new() {
                topLeft, topRight, bottomRight, bottomLeft, topLeft
            };

            for (int i = 0; i < points.Count - 1; i++) {
                Vector2 pointA = points[i];
                Vector2 pointB = points[i + 1];
                Vector2 result = camera.WorldToScreenPoint((Vector2) renderer.transform.TransformPoint(pointA));
                pointA = new Vector2((int) Math.Round(result.x), (int) Math.Round(Screen.height - result.y));
                result = camera.WorldToScreenPoint((Vector2) renderer.transform.TransformPoint(pointB));
                pointB = new Vector2((int) Math.Round(result.x), (int) Math.Round(Screen.height - result.y));
                Drawing.DrawLine(pointA, pointB, HitboxType.Attack.Color, lineWidth, false);
            }
        }
    }

    private void DrawHitbox(Camera camera, Collider2D collider2D, HitboxType hitboxType, float lineWidth) {
        if (!collider2D || !collider2D.isActiveAndEnabled) {
            return;
        }

        if (!InCamera(camera, collider2D)) {
            return;
        }

        if (!InCurrentEbits(collider2D)) {
            return;
        }


        if (collider2D is BoxCollider2D or EdgeCollider2D or PolygonCollider2D) {
            switch (collider2D) {
                case BoxCollider2D boxCollider2D:
                    Vector2 halfSize = boxCollider2D.size / 2f;
                    Vector2 topLeft = new(-halfSize.x, halfSize.y);
                    Vector2 topRight = halfSize;
                    Vector2 bottomRight = new(halfSize.x, -halfSize.y);
                    Vector2 bottomLeft = -halfSize;
                    List<Vector2> boxPoints = new() {
                        topLeft, topRight, bottomRight, bottomLeft, topLeft
                    };
                    DrawPointSequence(boxPoints, camera, collider2D, hitboxType, lineWidth);
                    break;
                case EdgeCollider2D edgeCollider2D:
                    DrawPointSequence(new List<Vector2>(edgeCollider2D.points), camera, collider2D, hitboxType, lineWidth);
                    break;
                case PolygonCollider2D polygonCollider2D:
                    for (int i = 0; i < polygonCollider2D.pathCount; i++) {
                        List<Vector2> polygonPoints = new(polygonCollider2D.GetPath(i));
                        if (polygonPoints.Count > 0) {
                            polygonPoints.Add(polygonPoints[0]);
                        }

                        DrawPointSequence(polygonPoints, camera, collider2D, hitboxType, lineWidth);
                    }

                    break;
            }
        } else if (collider2D is CircleCollider2D circleCollider2D) {
            Vector2 center = LocalToScreenPoint(camera, collider2D, Vector2.zero);
            Vector2 right = LocalToScreenPoint(camera, collider2D, Vector2.right * circleCollider2D.radius);
            int radius = (int) Math.Round(Vector2.Distance(center, right));

            Drawing.DrawCircle(center, radius, hitboxType.Color, lineWidth, true, radius / 10);
        }
    }

    private void DrawPointSequence(List<Vector2> points, Camera camera, Collider2D collider2D, HitboxType hitboxType, float lineWidth) {
        Vector2? pointB = null;
        for (int i = 0; i < points.Count - 1; i++) {
            Vector2 pointA = pointB ?? LocalToScreenPoint(camera, collider2D, points[i]);
            pointB = LocalToScreenPoint(camera, collider2D, points[i + 1]);

            Drawing.DrawLine(pointA, pointB.Value, hitboxType.Color, lineWidth, false);
        }
    }

    private static Vector2 LocalToScreenPoint(Camera camera, Collider2D collider2D, Vector2 point) {
        Vector2 result = camera.WorldToScreenPoint((Vector2) collider2D.transform.TransformPoint(point + collider2D.offset));
        return new Vector2((int) Math.Round(result.x), (int) Math.Round(Screen.height - result.y));
    }

    private static bool InCurrentEbits(Collider2D collider2D) {
        GameObject go = collider2D.gameObject;
        GameObject parent = go.transform.parent?.gameObject;
        string name = go.name;
        string parentName = parent?.name ?? "";
        int layer = go.layer;
        if (layer == 10 && parent) {
            layer = parent.layer;
        }

        EBits currentDimension = Manager<DimensionManager>.instance.currentDimension;

        if (layer is Layers.GROUND_16 or Layers.WATER_16 or Layers.QUICKSAND_16 or Layers.COLLISION_16 or Layers.LAVA_16 ||
            name.Contains("_16") && !name.Contains("_8") || parentName.Contains("_16") && !parentName.Contains("_8")) {
            return currentDimension == EBits.BITS_16;
        } else if (layer is Layers.GROUND_8 or Layers.WATER_8 or Layers.QUICKSAND_8 or Layers.COLLISION_8 or Layers.LAVA_8 ||
                   name.Contains("_8") && !name.Contains("_16") || parentName.Contains("_8") && !parentName.Contains("_16")) {
            return currentDimension == EBits.BITS_8;
        } else {
            return true;
        }
    }

    private static bool InCamera(Camera camera, Collider2D collider2D) {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, collider2D.bounds);
    }
}

public class Layers {
    public const int DEFAULT = 0;
    public const int TRANSPARENT_FX = 1;
    public const int IGNORE_RAYCAST = 2;
    public const int WATER_8 = 4;
    public const int UI = 5;
    public const int PLAYER = 8;
    public const int GROUND_8 = 9;
    public const int HITTABLE = 11;
    public const int GROUND_16 = 12;
    public const int PLAYER_ATTACK_TRIGGER = 13;
    public const int SPAWN_TRIGGER = 15;
    public const int SCROLL_BLOCK = 16;
    public const int MOVING_COLLISION_8 = 23;
    public const int MOVING_COLLISION_16 = 24;
    public const int COLLISION_8 = 21;
    public const int COLLISION_16 = 22;
    public const int MULTI_DIMENSION_COLLISION = 10;
    public const int WATER_16 = 14;
    public const int SOFT_GROUND_DETECTOR = 17;
    public const int QUICKSAND_8 = 18;
    public const int QUICKSAND_16 = 19;
    public const int LAVA_8 = 20;
    public const int LAVA_16 = 25;
    public const int DIMENSION_ZONE_MASK = 28;
    public const int DIMENSION_PORTAL_MASK = 29;
    public const int DIMENZION_ZONE = 30;
    public const int DIMENSION_DETECTION_TRIGGER = 31;
}