using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl: MonoBehaviour
{
    public static CameraControl instance;
    public Camera mainCam;
    private float _distanceToObject;
    private Ray _ray;
    public RaycastHit hit;
    [SerializeField] protected float maxHorizontalAngle = 60f;
    [SerializeField] protected float maxVerticalAngle = 60f;
    protected float XRotation;
    protected float YRotation;
    private IInteractable _prevSelected;

    #region Singleton
    private void Awake()
    {
        mainCam = GetComponent<Camera>();

        if (instance != null) Debug.LogWarning("More than one Main camera.");

        instance = this;
    }
    #endregion

    public virtual void Update()
    {
        if (!UIManager.instance.IsAnyMenuOpen)
        {
            if (Input.GetAxis("Vertical") > 0f) // If Player is moving forward.
            {
                Vector3 forward = transform.forward;

                forward.y = 0f;

                if (forward.sqrMagnitude > 0.01f) Player.instance.transform.rotation = Quaternion.LookRotation(forward); // Rotate character model to the direction the camera is facing.

                XRotation = 0f;
            }
        }
    }

    void LateUpdate()
    {
        if (!UIManager.instance.IsAnyMenuOpen)
        {
            if (!Input.GetMouseButton(0) & !Input.GetMouseButton(1))
            {
                XRotation += Input.GetAxis("Mouse X") * GameMaster.instance.mouseSensitivity * Time.deltaTime; ;
                YRotation -= Input.GetAxis("Mouse Y") * GameMaster.instance.mouseSensitivity * Time.deltaTime; ;

                YRotation = Mathf.Clamp(YRotation, -maxVerticalAngle, maxVerticalAngle);

                if (XRotation > maxHorizontalAngle)
                {
                    float extra = XRotation - maxHorizontalAngle;
                    XRotation = maxHorizontalAngle;

                    Player.instance.transform.Rotate(Vector3.up * extra);
                }
                else if (XRotation < -maxHorizontalAngle)
                {
                    float extra = XRotation + maxHorizontalAngle;
                    XRotation = -maxHorizontalAngle;

                    Player.instance.transform.Rotate(Vector3.up * extra);
                }

                XRotation = Mathf.Clamp(XRotation, -maxHorizontalAngle, maxHorizontalAngle);

                Quaternion parentRotation = Player.instance.transform.rotation;
                Quaternion localRotation = Quaternion.Euler(YRotation, XRotation, 0f);
                transform.rotation = parentRotation * localRotation;

                if (Physics.Raycast(_ray, out hit))
                {
                    _distanceToObject = Vector3.Distance(hit.transform.position, transform.position);

                    if (hit.transform.tag == "Items" && _distanceToObject < GameMaster.instance.interactionDistance)
                    {
                        UIManager.instance.SetCursor(true);
                    }
                    else if (hit.transform.tag != "Items" || _distanceToObject > GameMaster.instance.interactionDistance)
                    {
                        UIManager.instance.SetCursor(false);
                    }
                }
            }
        }
    }

    public IInteractable InteractionCheck()
    {
        IInteractable result = null;

        if (!UIManager.instance.IsAnyMenuOpen)
        {
            _ray = new Ray(transform.position, transform.forward * GameMaster.instance.interactionDistance);

            //Debug.DrawRay(transform.position, transform.forward * GM.GetComponent<DungeonMaster>().interactionDistance, Color.red);

            if (Physics.Raycast(_ray, out hit, 100f, ~0, QueryTriggerInteraction.Collide)) // "QueryTriggerInteraction.Collide" is necessary for raycast to work with trigger colliders.
            {
                _distanceToObject = Vector3.Distance(hit.transform.position, transform.position); // Calculate distance to the target object.

                if (_distanceToObject < GameMaster.instance.interactionDistance) // If distance is appropriate.
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Environment") || hit.transform.gameObject.layer == LayerMask.NameToLayer("Entitites"))
                    {
                        if (hit.collider.isTrigger)
                        {
                            IInteractable currentSelected = hit.transform.GetComponentInChildren<IInteractable>();

                            if (currentSelected != null)
                            {
                                if (_prevSelected != null)
                                {
                                    _prevSelected.OnDeselect();
                                }

                                currentSelected.OnSelect();

                                _prevSelected = currentSelected;

                                result = currentSelected;
                            }
                        }
                    }
                }
            }
        }

        if (result == null)
        {
            if (_prevSelected != null)
            {
                _prevSelected.OnDeselect();

                _prevSelected = null;
            }
        }

        return result;
    }
}
