using PixelPerfectURP;
using UnityEngine;

namespace HT
{
    public class FaMa : MonoBehaviour
    {
        public float leftMaxPos; // local z 最小值
        public float rightMaxPos; // local z 最大值
        [SerializeField] private float currentPos;
        public float currentRatio { get; private set; }

        private Rigidbody rb;
        private HingeJoint hinge;

        // 用于存储 HingeJoint 的部分配置
        private HingeJointData savedHingeData;
        private bool isDragging = false;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            hinge = GetComponent<HingeJoint>();
        }
        

        /// <summary>
        /// 由外部 Update 调用的 Tick 方法，根据输入条件进行鼠标点击、拖拽和释放的判断
        /// </summary>
        public void Tick()
        {
            Camera cam = PixelCameraManager.Instance.viewCamera.GetComponent<Camera>();

            // 鼠标按下（模拟 OnMouseDown）
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    // 判断射线碰撞到的对象是否为当前对象
                    if (hit.collider.gameObject == gameObject)
                    {
                        CustomOnMouseDown();
                    }
                }
            }

            // 鼠标拖拽中（模拟 OnMouseDrag）
            if (isDragging && Input.GetMouseButton(0))
            {
                CustomOnMouseDrag();
            }

            // 鼠标释放（模拟 OnMouseUp）
            if (isDragging && Input.GetMouseButtonUp(0))
            {
                CustomOnMouseUp();
            }
        }

        /// <summary>
        /// 处理鼠标按下时的逻辑
        /// </summary>
        private void CustomOnMouseDown()
        {
            isDragging = true;
            // 如果存在 HingeJoint，则保存其部分配置后移除它
            if (hinge != null)
            {
                savedHingeData = new HingeJointData
                {
                    connectedBody = hinge.connectedBody,
                    anchor = hinge.anchor,
                    axis = hinge.axis
                };
                Destroy(hinge);
                hinge = null;
            }
            if (rb != null)
                rb.isKinematic = true;
        }

        /// <summary>
        /// 处理鼠标拖拽时的逻辑（仅修改 localPosition 的 z 分量）
        /// </summary>
        private void CustomOnMouseDrag()
        {
            Camera cam = PixelCameraManager.Instance.viewCamera.GetComponent<Camera>();
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.green);

            // 注意：此处平面法向采用的是父物体的 right（即允许变化的方向），若没有父物体则使用世界坐标的 Vector3.forward
            Vector3 planeNormal = transform.parent ? transform.parent.right : Vector3.forward;
            Plane plane = new Plane(planeNormal, transform.position);

            if (plane.Raycast(ray, out float distance))
            {
                // 得到世界坐标下的拖拽点
                Vector3 worldPos = ray.GetPoint(distance);

                // 如果有父物体，则将 worldPos 转换为父物体局部坐标，否则直接使用 worldPos
                Vector3 localPos = transform.parent
                    ? transform.parent.InverseTransformPoint(worldPos)
                    : worldPos;

                if (Input.GetMouseButtonDown(1))
                {
                    Debug.Log(GetRatio());
                }

                // 仅修改 localPosition 的 z 分量，其他分量保持不变
                Vector3 currentLocal = transform.localPosition;
                currentLocal.z = Mathf.Clamp(localPos.z, rightMaxPos, leftMaxPos);
                currentPos = currentLocal.z;
                transform.localPosition = currentLocal;
            }
            currentRatio = GetRatio();
        }

        /// <summary>
        /// 处理鼠标释放时的逻辑
        /// </summary>
        private void CustomOnMouseUp()
        {
            isDragging = false;
            if (rb != null)
                rb.isKinematic = false;

            // 拖拽结束后，重新添加 HingeJoint 并恢复配置
            hinge = gameObject.AddComponent<HingeJoint>();
            if (savedHingeData != null)
            {
                hinge.connectedBody = savedHingeData.connectedBody;
                hinge.anchor = savedHingeData.anchor;
                hinge.axis = savedHingeData.axis;
            }
            
        }

        float GetRatio()
        {
            currentPos = transform.localPosition.z;
            float range = Mathf.Abs(rightMaxPos - leftMaxPos);
            float ratio = Mathf.Abs(currentPos - leftMaxPos) / range;
            return ratio;
        }

        // 调试用：在 Scene 视图中绘制拖拽参考平面
        void OnDrawGizmos()
        {
            // 计算平面的法向量，与 CustomOnMouseDrag 中保持一致
            Vector3 planeNormal = transform.parent ? transform.parent.right : Vector3.forward;
            Vector3 planeOrigin = transform.position;

            // 求平面上的两个正交切向量
            Vector3 tangent = Vector3.Cross(planeNormal, Vector3.up);
            if (tangent.sqrMagnitude < 0.001f)
                tangent = Vector3.Cross(planeNormal, Vector3.right);
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(planeNormal, tangent).normalized;

            // 定义绘制平面的大小
            float size = 0.5f;
            // 计算平面四个角的位置
            Vector3 corner1 = planeOrigin + (tangent + bitangent) * size;
            Vector3 corner2 = planeOrigin + (tangent - bitangent) * size;
            Vector3 corner3 = planeOrigin + (-tangent - bitangent) * size;
            Vector3 corner4 = planeOrigin + (-tangent + bitangent) * size;

            // 绘制平面边界（青色）
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(corner1, corner2);
            Gizmos.DrawLine(corner2, corner3);
            Gizmos.DrawLine(corner3, corner4);
            Gizmos.DrawLine(corner4, corner1);

            // 绘制平面法向量（红色）
            Gizmos.color = Color.red;
            Gizmos.DrawRay(planeOrigin, planeNormal * size);
        }
    }

    // 用于存储 HingeJoint 的部分参数
    public class HingeJointData
    {
        public Rigidbody connectedBody;
        public Vector3 anchor;
        public Vector3 axis;
    }
}
