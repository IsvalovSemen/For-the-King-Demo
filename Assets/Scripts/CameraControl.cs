using UnityEngine;
using UnityEngine.UI;

public class CameraControl: MonoBehaviour
{
    public static CameraControl instance;
    public Camera mainCam;
    protected float XRot;
    protected float YRot;
    [SerializeField] protected float viewAngle = 60f;
    private float _distanceToObject;
    private Ray _ray;
    public RaycastHit hit;
    private Transform _target;
    
    #region Singleton
    void Awake()
    {
        mainCam = GetComponent<Camera>();

        if (instance != null) Debug.LogWarning("More than one Main camera.");

        instance = this;
    }
    #endregion

    public virtual void Update()
    {
        if (!UIManager.instance.IsAnyMenuOpen()) // Freezes camera movement if any of menu is opened.
        {
            if (!Input.GetMouseButton(0) & !Input.GetMouseButton(1)) // If not choosing attack direction right now. FIXME: PROBABLY REMOVE IT LATER.
            {
                XRot -= Input.GetAxis("Mouse Y") * GameMaster.instance.mouseSensitivity * Time.deltaTime;

                //XRot = Mathf.Clamp(XRot, -viewAngle, viewAngle);

                YRot -= Input.GetAxis("Mouse X") * GameMaster.instance.mouseSensitivity * Time.deltaTime;

                //YRot = Mathf.Clamp(YRot, -viewAngle,viewAngle);

                transform.rotation = Quaternion.Euler(XRot, -YRot, 0f);
                /* 
                if (Input.GetAxis("Mouse ScrollWheel") > 0f || Input.GetAxis("Mouse ScrollWheel") < 0f) // If any menu is opened, zooms in/out view via mouse scroll
                {
                    GetComponent<Camera>().fieldOfView = Mathf.Clamp(GetComponent<Camera>().fieldOfView - (10 * Input.GetAxis("Mouse ScrollWheel")), 60, 90);
                }
                */
                _ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

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

    public InteractionType InteractionCheck()
    {
        InteractionType result = InteractionType.None;

        if (!UIManager.instance.IsAnyMenuOpen())
        {
            _ray = new Ray(transform.position, transform.forward * GameMaster.instance.interactionDistance);

            //Debug.DrawRay(transform.position, transform.forward * GM.GetComponent<DungeonMaster>().interactionDistance, Color.red);

            if (Physics.Raycast(_ray, out hit, 100f, ~0, QueryTriggerInteraction.Collide)) // "QueryTriggerInteraction.Collide" is necessary for raycast to work with trigger colliders.
            {
                _distanceToObject = Vector3.Distance(hit.transform.position, transform.position);

                if (_distanceToObject < GameMaster.instance.interactionDistance & hit.transform.GetComponent<IInteractable>() != null && (hit.transform.gameObject.layer == 3 || hit.transform.gameObject.layer == 6))
                {
                    if (hit.transform.GetComponent<Item>() != null)
                    {
                        //InventoryManager.instance.ShowItemInfo(hit.transform.GetComponent<Item>()); // Show item infobox while looking at item.

                        _target = hit.transform;
                    }
                    //else InventoryManager.instance.ClearItemInfo(); // Close item infobox if looking away.

                    result = hit.transform.GetComponent<IInteractable>().interactionType;
                }
                else
                {
                    result = InteractionType.None;

                    //InventoryManager.instance.ClearItemInfo();
                }
            }
            else result = InteractionType.None;

            if (result == InteractionType.None)
            {
                if (_target != null)
                {
                    //InventoryManager.instance.ClearItemInfo();

                    _target = null;
                }
            }
        }

        return result;
    }
}
