using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Netcode;
using UnityEngine.Networking;

namespace SurvivorsEscape
{
    public enum CharacterStance { STANDING, CROUCHING}
    public class CharacterController : NetworkBehaviour, IHitResponder
    {
        private InputManager _inputs;
        private CameraController _cameraController;
        private Animator _animator;
        private EventHandler _eventHandler;
        private CapsuleCollider _capsuleCollider;
        private CharacterStance _stance;

        [Header("Speed [Normal Sprint]")]
        [SerializeField] private Vector3 _standingSpeed = new Vector3(6, 8, 2);
        [SerializeField] private Vector2 _crouchingSpeed = new Vector2(0, 0);

        [Header("Capsule [Radius Height YOffset]")]
        [SerializeField] private Vector3 _standingCapsule = Vector3.zero;
        [SerializeField] private Vector3 _crouchingCapsule = Vector3.zero;

        [Header("Attacking")]
        [SerializeField] private int _lifeDamage = 5;
        [SerializeField] private int _woodDamage = 12;
        [SerializeField] private int _rockDamage = 7;
        [SerializeField] private SEHitBox _hitBox;

        [Header("Sharpness")]
        [SerializeField] private float _moveSharpness = 10f;
        [SerializeField] private float _standingRotationSharpness = 10f;
        [SerializeField] private float _crouchingRotationSharpness = 10f;

        [Header("Body parts")]
        [SerializeField] private Transform _hand;
        private GameObject _handVessel;

        [Header("Shootable objects")] 
        [SerializeField] private LayerMask aimLayerMask = new LayerMask();
        [SerializeField] private Transform _bullerProjectile;
        [SerializeField] private Transform _spawnBulletPosition;

        [SerializeField] private Transform _debugTransform;

        [Header("Shooting particles")]
        [SerializeField] private Transform _vfxParticlesCollition;

        #region ANIMATOR_STATE_NAMES
        private const string _standToCrouch = "Base Layer.Base Crouching";
        private const string _crouchToStand = "Base Layer.Base Standing";
        private const string _meleeAtack    = "Base Layer.Melee attack horizontal";
        private const string _shootRifle    = "Base Layer.Shoot rifle";
        #endregion

        private bool _inAnimation;
        private Vector3 _animatorVelocity;
        private Quaternion _animatorDeltaRotation;
        private bool _hitting;
        private List<GameObject> _objectsHit = new List<GameObject>();

        private bool _strafing;
        private bool _sprinting;
        private bool _hasGun;
        private float _strafeParameter;
        private Vector3 _strafeParameterXZ;

        private LayerMask _layerMask;
        private Collider[] _obstructions = new Collider[8];

        private float _runSpeed;
        private float _sprintSpeed;
        private float _walkSpeed;
        private float _rotationSharpness;

        private float _targetSpeed;
        private Quaternion _targetRotation;

        private float _newSpeed;
        private Vector3 _newVelocity;
        private Quaternion _newRotation;

        private bool _proning;
        private bool _isAiming;
        private bool _bInvOpen = false;
        private Vector3 _mouseWorldPosition = new Vector3();

        private GameObject _handInt;
        private GameObject _handBone;

        private void Start()
        {
            _hasGun = false;

            _animator = GetComponent<Animator>();
            _cameraController = IsOwner ? GetComponent<CameraController>() : null;
            _inputs = GetComponent<InputManager>();
            _eventHandler = GetComponent<EventHandler>();
            _capsuleCollider = GetComponent<CapsuleCollider>();

            //_handVessel = GameObject.Find("mixamorig1:RightHand");
            //if (_handVessel != null)
            //{
            //    Debug.Log("GOT THE BONE");
            //    _handVessel.AddComponent<NetworkObject>();

            //    var nw = _handVessel.GetComponent<NetworkObject>();
            //    if (nw != null)
            //    {
            //       // nw.Spawn(true);
            //        Debug.Log("Spawned HAND VESSEL");
            //    }
            //}
            
            //_handBone = GameObject.Find("WeaponAttach");
            //if (_handBone != null)
            //{
            //    Debug.Log("GOT THE HAND BRI");
            //    var nw = _handBone.GetComponent<NetworkObject>();
            //    if (nw != null)
            //    {
            //        nw.Spawn(true);
            //        Debug.Log("SPAWNED THE HAND BROSK");
            //    }
            //}

            //ReparentHandClientRpc();

            _runSpeed = _standingSpeed.x;
            _sprintSpeed = _standingSpeed.y;
            _walkSpeed = _standingSpeed.z;
            _rotationSharpness = _standingRotationSharpness;
            _hitBox.HitResponder = this;

            _stance = CharacterStance.STANDING;
            SetCapsuleDimensions(_standingCapsule);

            int mask = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!(Physics.GetIgnoreLayerCollision(gameObject.layer, i)))
                {
                    mask |= 1 << i;
                }
            }
            _layerMask = mask;

            _eventHandler.Event.AddListener(OnEvent);
        }

        [ClientRpc]
        void ReparentHandClientRpc()
        {

            //_handBone.transform.SetParent(transform, false);
        }

        private void Update()
        {
            if (_proning) return;
            if (!IsOwner) return;

            Vector3 moveInputVector = new Vector3(_inputs.MoveAxisRight, 0, _inputs.MoveAxisForward);
            Vector3 cameraPlanarDirection = _cameraController._cameraPlanarDirection;

            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection);
            Vector3 moveInputVectorOrientation = cameraPlanarRotation * moveInputVector.normalized;

            if (_inputs.CursosrEnable.PressedDown())
            {
                if (_bInvOpen)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    _bInvOpen = false;
                    Debug.Log("Inv close");
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    _bInvOpen = true;
                    Debug.Log("Inv open");
                }
            }

            if (_inputs.Aim.Pressed())
            {
                if (!_isAiming)
                {
                    _cameraController.SetAimView(true);
                    _isAiming = true;
                }
            }
            else
            {
                if (_isAiming)
                {
                    _cameraController.SetAimView(false);
                    _isAiming = false;
                }
            }

            if(_strafing)
            {
                _sprinting = _inputs.Sprint.PressedDown() && (moveInputVector != Vector3.zero);
                _strafing = !_inputs.Draw.PressedDown() && !_sprinting;
            }
            else
            {
                _sprinting = _inputs.Sprint.Pressed() && (moveInputVector != Vector3.zero);
                _strafing = _inputs.Draw.PressedDown();
            }

            if (_sprinting)
                _targetSpeed = moveInputVector != Vector3.zero ? _sprintSpeed : 0;
            else if (_strafing)
                _targetSpeed = moveInputVector != Vector3.zero ? _walkSpeed : 0; 
            else
                _targetSpeed = moveInputVector != Vector3.zero ? _runSpeed : 0;

            _newSpeed = Mathf.Lerp(_newSpeed, _targetSpeed, Time.deltaTime * _moveSharpness);

            if (_inAnimation)
                _newVelocity = _animatorVelocity;
            else
                _newVelocity = moveInputVectorOrientation * _targetSpeed;
            transform.Translate(_newVelocity * Time.deltaTime, Space.World);

            if (_inAnimation)
            {
                transform.rotation *= _animatorDeltaRotation;
            }

            if(_strafing)
            {
                _targetRotation = Quaternion.LookRotation(cameraPlanarDirection);
                _newRotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _rotationSharpness);
                transform.rotation = _newRotation;
            }
            else if (_targetSpeed != 0)
            {
                _targetRotation = Quaternion.LookRotation(moveInputVectorOrientation);
                _newRotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _rotationSharpness);
                transform.rotation = _newRotation;
            }

            if (_strafing)
            {
                _strafeParameter = Mathf.Clamp01(_strafeParameter + Time.deltaTime * 4);
                _strafeParameterXZ = Vector3.Lerp(_strafeParameterXZ, moveInputVector * _newSpeed, _moveSharpness * Time.deltaTime);
            }
            else
            {
                _strafeParameter = Mathf.Clamp01(_strafeParameter - Time.deltaTime * 4);
                _strafeParameterXZ = Vector3.Lerp(_strafeParameterXZ, Vector3.forward * _newSpeed, _moveSharpness * Time.deltaTime);
            }

            if (_hasGun)
            {
                _animator.SetFloat("Strifing", -_strafeParameter);
            }
            else
            {
                _animator.SetFloat("Strifing", _strafeParameter);
            }


            _animator.SetFloat("StrifingX", Mathf.Round(_strafeParameterXZ.x * 100f) / 100f);
            _animator.SetFloat("StrifingZ", Mathf.Round(_strafeParameterXZ.z * 100f) / 100f);

            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            Transform hitTransform = null;
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimLayerMask))
            {
                _debugTransform.position = raycastHit.point;
                _mouseWorldPosition = raycastHit.point;
                hitTransform = raycastHit.transform;
            }

            if (!_hasGun)
            {
                if (!_inAnimation)
                {
                    if (_inputs.Attack.PressedDown())
                    {
                        _inAnimation = true;
                        _animator.CrossFadeInFixedTime(_meleeAtack, 0.1f, 0, 0);
                    }
                }
            }
            else
            {
                if (!_inAnimation)
                {
                    if (_inputs.Attack.PressedDown())
                    {
                        _inAnimation = true;
                        _animator.CrossFadeInFixedTime(_shootRifle, 0.1f, 0, 0);

                        Vector3 aimDir = (_mouseWorldPosition - _spawnBulletPosition.position).normalized;

                        Spawner.Instace.SpawnBulletServerRpc(_spawnBulletPosition.position.x,
                            _spawnBulletPosition.position.y,
                            _spawnBulletPosition.position.z,
                            aimDir.x,
                            aimDir.y,
                            aimDir.z);

                        //Instantiate(_bullerProjectile, _spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));

                        /*if (hitTransform != null)
                        {
                            if (hitTransform.GetComponent<BulletTarget>() != null)
                            {
                                Instantiate(_vfxParticlesCollition, transform.position, Quaternion.identity);
                            }
                        }*/

                        
                    }
                }
            }

            if(_hitting)
            {
                _hitBox.CheckHit();
            }
        }

        public void SetDMG(int ld, int wd, int rd)
        {
            _lifeDamage = ld;
            _woodDamage = wd;
            _rockDamage = rd;
        }

        private void OnAnimatorMove()
        {
            if (!IsOwner) return;

            if(_inAnimation)
            {
                _animatorVelocity = _animator.velocity;
                _animatorDeltaRotation = _animator.deltaRotation;
            }
        }

        private void LateUpdate()
        {
            if (_proning) return;
            if (!IsOwner) return;

            switch (_stance)
            {
                case CharacterStance.STANDING:
                    if (_inputs.Crouch.PressedDown()) { RequestStanceChange(CharacterStance.CROUCHING); }
                    break;

                case CharacterStance.CROUCHING:
                    if (_inputs.Crouch.PressedDown()) { RequestStanceChange(CharacterStance.STANDING); }
                    break;
            }
        }

        public bool RequestStanceChange(CharacterStance newStance)
        {
            if (_stance == newStance) return true;

            switch (_stance)
            {
                case CharacterStance.STANDING:
                    if (newStance == CharacterStance.CROUCHING)
                    {
                        if (!CharacterOverlap(_crouchingCapsule))
                        {
                            _runSpeed = _crouchingSpeed.x;
                            _sprintSpeed = _crouchingSpeed.y;
                            _rotationSharpness = _crouchingRotationSharpness;
                            _stance = newStance;

                            _animator.CrossFadeInFixedTime(_standToCrouch, 0.5f);
                            SetCapsuleDimensions(_crouchingCapsule);

                            return true;
                        }
                    }
                    break;

                case CharacterStance.CROUCHING:
                    if (newStance == CharacterStance.STANDING)
                    {
                        if (!CharacterOverlap(_standingCapsule))
                        {
                            _runSpeed = _standingSpeed.x;
                            _sprintSpeed = _standingSpeed.y;
                            _rotationSharpness = _standingRotationSharpness;
                            _stance = newStance;

                            _animator.CrossFadeInFixedTime(_crouchToStand, 0.5f);
                            SetCapsuleDimensions(_standingCapsule);

                            return true;
                        }
                    }
                    break;
            }

            return false;

        }

        private bool CharacterOverlap(Vector3 capsuleDimensions)
        {
            float radius = capsuleDimensions.x;
            float height = capsuleDimensions.y;
            Vector3 center = new Vector3(_capsuleCollider.center.x, capsuleDimensions.z, _capsuleCollider.center.z);

            Vector3 point0;
            Vector3 point1;

            if (height < radius * 2)
            {
                point0 = transform.position + center;
                point1 = transform.position + center;
            }
            else
            {
                point0 = transform.position + center + (transform.up * (height * 0.5f - radius));
                point1 = transform.position + center - (transform.up * (height * 0.5f - radius));
            }
            radius = radius - 0.1f;

            int numOverlaps = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, _obstructions, _layerMask);
            for (int i = 0; i < numOverlaps; i++)
            {
                if (_obstructions[i] == _capsuleCollider)
                    numOverlaps--;
            }

            return numOverlaps > 0;
        }

        private void SetCapsuleDimensions(Vector3 capsuleDimensions)
        {
            _capsuleCollider.center = new Vector3(_capsuleCollider.center.x, capsuleDimensions.z, _capsuleCollider.center.z);
            _capsuleCollider.radius = capsuleDimensions.x;
            _capsuleCollider.height = capsuleDimensions.y;
        }

        public void OnEvent(string eventName)
        {
            if (!IsOwner) return;

            switch(eventName)
            {
                case "HitStart":
                    _objectsHit.Clear();
                    _hitting = true;
                    break;

                case "HitEnd":
                    _hitting = false;
                    break;

                case "AnimationEnd":
                    _inAnimation = false;
                    break;
            }
        }

        public void TakeWeapon(INV_PickUp item)
        {
            Vector3 newPosition = new Vector3(0.087f, 0.082f, 0.07f);
            Vector3 newRotation = new Vector3(-162.30f, 71.77f, -29.83f);

            var rb = item.GetComponent<Rigidbody>();
            if (rb != null)
                Destroy(rb);

            item.transform.parent = _handBone.transform;
            item.gameObject.transform.position = _handVessel.transform.position;
            item.gameObject.transform.eulerAngles = _handVessel.transform.eulerAngles;
        }

        int IHitResponder.LifeDamage
        {
            get => _lifeDamage;
        }
        int IHitResponder.WoodDamage
        {
            get => _woodDamage;
        }
        int IHitResponder.RockDamage
        {
            get => _rockDamage;
        }

        bool IHitResponder.CheckHit(HitInteraction data)
        {
            if (data.HurtBox.Owner == gameObject)
                return false;

            else if(_objectsHit.Contains(data.HurtBox.Owner))
                return false;
            else
                return true;
        }

        void IHitResponder.Response(HitInteraction data)
        {
            _objectsHit.Add(data.HurtBox.Owner);
        }

        public Vector2 GetStandingSpeed() { return _standingSpeed; }
        public float GetMoveSharpness() { return _moveSharpness; }
        public float GetRotationSharpness() { return _rotationSharpness; }

        public Animator GetAnimator() { return _animator; }
        public CameraController GetCameraController() { return _cameraController; }
        public InputManager GetInputManager() { return _inputs; }
        public Transform GetPlayerTransform() { return transform; }

        public bool IsPlayerOwner() { return IsOwner; }
    }
}
