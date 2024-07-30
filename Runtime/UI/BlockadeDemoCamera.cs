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
        private RectTransform _rectTransform;
        public RectTransform RectTransform
        {
            get { return _rectTransform; }
            set { _rectTransform = value; }
        }

        [SerializeField]
        private bool _autoPan = true;
        public bool AutoPan
        {
            get { return _autoPan; }
            set { _autoPan = value; }
        }

        [SerializeField]
        private float _autoPanSpeed = 10.0f;
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
        private float _manualPanSpeed = 80.0f;
        public float ManualPanSpeed
        {
            get { return _manualPanSpeed; }
            set { _manualPanSpeed = value; }
        }

        [SerializeField]
        private float _zoomDefault = 0.0f;
        public float ZoomDefault
        {
            get { return _zoomDefault; }
            set { _zoomDefault = value; }
        }

        [SerializeField]
        private float _sphereOrbitZoomDefault = -1f;
        public float SphereOrbitZoomDefault
        {
            get { return _sphereOrbitZoomDefault; }
            set { _sphereOrbitZoomDefault = value; }
        }

        [SerializeField]
        private float _meshCreatorZoomDefault = -0.5f;
        public float MeshCreatorZoomDefault
        {
            get { return _meshCreatorZoomDefault; }
            set { _meshCreatorZoomDefault = value; }
        }

        [SerializeField]
        private float _zoomSpeed = 120.0f;
        public float ZoomSpeed
        {
            get { return _zoomSpeed; }
            set { _zoomSpeed = value; }
        }

        [SerializeField]
        private float _zoomMax = 2.0f;
        public float ZoomMax
        {
            get { return _zoomMax; }
            set { _zoomMax = value; }
        }

        [SerializeField]
        private float _zoomMin = -2.0f;
        public float ZoomMin
        {
            get { return _zoomMin; }
            set { _zoomMin = value; }
        }

        [SerializeField]
        private float _sphereOrbitZoomMax = -2.0f;
        public float SphereOrbitZoomMax
        {
            get { return _sphereOrbitZoomMax; }
            set { _sphereOrbitZoomMax = value; }
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
            WaitingToAutoPan,
            AutoPanning,
            ManuallyMoving
        }

        public enum Mode
        {
            SkyboxDefault,
            CenterOrbit,
            MeshCreator
        }

        private Vector2 _initialRotation;
        private Vector3 _mousePosition;
        private float _yaw;
        private float _pitch;
        private float _zoom;
        private State _state = State.WaitingToAutoPan;
        private float _autoPanTimeStart;
        private float _previousZoom;
        private Mode _mode;

        public void SetMode(Mode mode)
        {
            _mode = mode;
            switch (mode)
            {
                case Mode.SkyboxDefault:
                    _zoom = _zoomDefault;
                    break;
                case Mode.CenterOrbit:
                    _zoom = _sphereOrbitZoomDefault;
                    break;
                case Mode.MeshCreator:
                    _zoom = _meshCreatorZoomDefault;
                    break;
            }
        }

        private void Awake()
        {
            _pitch = transform.rotation.eulerAngles.x;
            _yaw = transform.rotation.eulerAngles.y;
            _initialRotation = new Vector2(_pitch, _yaw);
        }

        private void Update()
        {
            switch (_state)
            {
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

            bool mouseOverGameView;

            if (_rectTransform)
            {
                mouseOverGameView = RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition);
            }
            else
            {
                mouseOverGameView = Input.mousePosition.x >= 0 &&
                                    Input.mousePosition.x < Screen.width &&
                                    Input.mousePosition.y >= 0 &&
                                    Input.mousePosition.y < Screen.height;
            }

            if (!MouseIsOverUI() && Input.mouseScrollDelta.y != 0 && mouseOverGameView)
            {
                _zoom += Input.mouseScrollDelta.y * _zoomSpeed * 0.001f;
                _zoom = Mathf.Clamp(_zoom, _zoomMin, _mode == Mode.CenterOrbit ? _sphereOrbitZoomMax : _zoomMax);
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(_pitch, _yaw, 0), _smooth * Time.deltaTime);
            var newZoom = Mathf.Lerp(_previousZoom, _zoom, _smooth * Time.deltaTime);
            transform.position = _skyboxSphere.position + transform.forward * newZoom;
            _previousZoom = newZoom;
        }

        public void ResetRotation()
        {
            _yaw = _initialRotation.y;
            _pitch = _initialRotation.x;
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0);
        }

        public void ResetView()
        {
            ResetRotation();
            _zoom = _zoomDefault;
            transform.position = _skyboxSphere.position + transform.forward * _zoom;
        }

        private void UpdateWaitingToAutoPan()
        {
            if (!MouseIsOverUI() && Input.GetMouseButtonDown(0))
            {
                _state = State.ManuallyMoving;
                _mousePosition = Input.mousePosition;
            }
            else if (!_autoPan)
            {
                // Ensure auto-pan starts right away if it's enabled.
                _autoPanTimeStart = 0;
            }
            else if (Time.time - _autoPanTimeStart > _autoPanDelay)
            {
                _state = State.AutoPanning;
            }
        }

        private void UpdateAutoPanning()
        {
            if (!_autoPan || (!MouseIsOverUI() && Input.GetMouseButtonDown(0)))
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

        private bool MouseIsOverUI()
        {
            if (_rectTransform)
            {
                return !RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition);
            }

            return EventSystem.current?.IsPointerOverGameObject() ?? false;
        }

        private void UpdateManuallyMoving()
        {
            if (Input.GetMouseButton(0))
            {
                var mouseDelta = Input.mousePosition - _mousePosition;
                _mousePosition = Input.mousePosition;

                if (_mode == Mode.CenterOrbit)
                {
                    mouseDelta = -mouseDelta;
                }

                _yaw -= mouseDelta.x * _manualPanSpeed * 0.001f;
                _pitch += mouseDelta.y * _manualPanSpeed * 0.001f;
            }
            else
            {
                _state = State.WaitingToAutoPan;
                _autoPanTimeStart = Time.time;
            }
        }
    }
}