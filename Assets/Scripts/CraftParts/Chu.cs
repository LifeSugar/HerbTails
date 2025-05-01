using UnityEngine;

namespace HT
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Chu : MonoBehaviour
    {

        /// <summary>已经捣击的次数（只读）</summary>
        [field: SerializeField]public int PoundCount { get; private set; } = 0;
        
        public void RegisterPound()
        {
            PoundCount++;
        }

        public void ResetPound()
        {
            PoundCount = 0;
        }
        
        [Header("钵的几何信息")]
        public Transform bowlCenter;
        public float innerRadius = 0.35f;
        public float minY = 0.5f;
        public float maxY = 1.4f;

        private Camera cam;
        private Rigidbody rb;

        private bool isHeld;
        private float grabDist;
        private Vector3 grabOffset;
        private Vector3 targetPos;

        void Awake()
        {
            cam = Camera.main;
            rb = GetComponent<Rigidbody>();

            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        void Update()
        {
            if (!isHeld && Input.GetMouseButtonDown(0) && CursorSlot.instance.isEmpty)
                TryGrab();
            if (isHeld && Input.GetMouseButtonUp(0)) Release();

            if (isHeld)
            {
                Vector3 pos = MouseWorldPoint() + grabOffset;
                pos = ClampInsideBowl(pos);
                targetPos = pos;
            }
        }

        void FixedUpdate()
        {
            if (isHeld) rb.MovePosition(targetPos);
        }

        /* ---------- 抓取 / 放下 ---------- */
        void TryGrab()
        {
            Ray ray = Utility.GetRayFromRealCamScreenPos(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f) &&
                hit.collider == GetComponent<Collider>())
            {
                isHeld = true;
                grabDist = hit.distance;
                grabOffset = transform.position - hit.point;
                rb.useGravity = false;
            }
        }

        void Release()
        {
            isHeld = false;
            rb.useGravity = true;
        }

        /* ---------- 计数触发点 ---------- */
        /// <summary>
        /// 当杵与钵（或其他物体）发生碰撞时自动递增捣击次数。
        /// </summary>
        void OnCollisionEnter(Collision col)
        {

            RegisterPound();
        }

        /* ---------- 约束 ---------- */
        Vector3 ClampInsideBowl(Vector3 pos)
        {
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            Vector3 rel = pos - bowlCenter.position;
            Vector2 hz = new Vector2(rel.x, rel.z);
            float dist = hz.magnitude;

            if (dist > innerRadius)
            {
                hz = hz / dist * innerRadius;
                pos.x = bowlCenter.position.x + hz.x;
                pos.z = bowlCenter.position.z + hz.y;
            }
            return pos;
        }

        /* ---------- 工具 ---------- */
        Vector3 MouseWorldPoint()
        {
            Ray ray = Utility.GetRayFromRealCamScreenPos(Input.mousePosition);
            return ray.origin + ray.direction * grabDist;
        }
    }
}
