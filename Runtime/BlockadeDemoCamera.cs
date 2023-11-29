using UnityEngine;
using UnityEngine.EventSystems;

namespace BlockadeLabsSDK
{
    public class BlockadeDemoCamera : MonoBehaviour
    {
        [SerializeField]
        private Transform _skyboxSphere;
        public Transform SkyboxSphere
        {
            get { return _skyboxSphere; }
            set { _skyboxSphere = value; }
        }

        [SerializeField]
        private bool _autoPan = true;
        public bool AutoPan
        {
            get { return _autoPan; }
            set { _autoPan = value; }
        }

        [SerializeField]
        private float _autoPanSpeed = 5.0f;
        public float AutoPanSpeed
        {
            get { return _autoPanSpeed; }
            set { _autoPanSpeed = value; }
        }

        [SerializeField]
        private float _autoPanDelay = 5.0f;
        public float AutoPanDelay
        {
            get { return _autoPanDelay; }
            set { _autoPanDelay = value; }
        }

        [SerializeField]
        private float _manualPanSpeed = 40.0f;
        public float ManualPanSpeed
        {
            get { return _manualPanSpeed; }
            set { _manualPanSpeed = value; }
        }

        [SerializeField]
        private float _zoomSpeed = 40.0f;
        public float ZoomSpeed
        {
            get { return _zoomSpeed; }
            set { _zoomSpeed = value; }
        }

        [SerializeField]
        private float _smooth = 5.0f;
        public float Smooth
        {
            get { return _smooth; }
            set { _smooth = value; }
        }

        private enum State
        {
            StayingStill,
            WaitingToAutoPan,
            AutoPanning,
            ManuallyMoving
        }


        private Vector3 _mousePosition;
        private float _yaw;
        private float _pitch;
        private float _zoom;
        private State _state = State.StayingStill;
        private float _autoPanTimeStart;

        void Update()
        {
            switch (_state)
            {
                case State.StayingStill:
                    UpdateStayingStill();
                    break;
                case State.WaitingToAutoPan:
                    UpdateWaitingToAutoPan();
                    break;
                case State.AutoPanning:
                    UpdateAutoPanning();
                    break;
                case State.ManuallyMoving:
                    UpdateManuallyMoving();
                    break;
            }

            if (!EventSystem.current.IsPointerOverGameObject() && Input.mouseScrollDelta.y != 0)
            {
                _zoom += Input.mouseScrollDelta.y * _zoomSpeed * Time.deltaTime;
                _zoom = Mathf.Clamp(_zoom, 0, 2);
            }

            var currentPos = transform.position;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(_pitch, _yaw, 0), Time.deltaTime * _smooth);
            transform.position = Vector3.Lerp(currentPos, _skyboxSphere.position + transform.rotation * Vector3.forward * _zoom, Time.deltaTime * _smooth);
        }

        private void UpdateStayingStill()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _state = State.ManuallyMoving;
                _mousePosition = Input.mousePosition;
            }
            else if (_autoPan)
            {
                _state = State.WaitingToAutoPan;
                _autoPanTimeStart = Time.time;
            }
        }

        private void UpdateWaitingToAutoPan()
        {
            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            {
                _state = State.ManuallyMoving;
                _mousePosition = Input.mousePosition;
            }
            else if (Time.time - _autoPanTimeStart > _autoPanDelay)
            {
                _state = State.AutoPanning;
            }
        }

        private void UpdateAutoPanning()
        {
            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            {
                _state = State.ManuallyMoving;
                _mousePosition = Input.mousePosition;
            }
            else
            {
                _yaw += _autoPanSpeed * Time.deltaTime;
                _pitch = 0;
            }
        }

        private void UpdateManuallyMoving()
        {
            if (Input.GetMouseButton(0))
            {
                var mouseDelta = Input.mousePosition - _mousePosition;
                _mousePosition = Input.mousePosition;
                _yaw += mouseDelta.x * _manualPanSpeed * Time.deltaTime;
                _pitch -= mouseDelta.y * _manualPanSpeed * Time.deltaTime;
            }
            else
            {
                _state = State.StayingStill;
            }
        }
    }
}