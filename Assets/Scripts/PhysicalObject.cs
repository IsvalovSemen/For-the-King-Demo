using UnityEngine;

public abstract class PhysicalObject : MonoBehaviour, IDamageable
{
    protected SoundManager SM { get; set; }
    [SerializeField] private float _weight;
    protected Rigidbody RB;
    protected int infinityFallThreshold = -1;
    [SerializeField] float _maxDurability = 100f;
    [SerializeField] protected float currentDurability;
    [SerializeField] float _discardingTime = 30f;
    [SerializeField] float _takeDmgThreshold = 1f;
    [SerializeField] public float dealDmgThreshold = 5f;
    [SerializeField] Transform _pieces;

    protected virtual void Awake()
    {
        currentDurability = _maxDurability;

        RB = GetComponent<Rigidbody>();

        RB.mass = _weight;
    }

    private void Start()
    {
        SM = GetComponent<SoundManager>();
    }

    private void Update()
    {
        if (transform.position.y < infinityFallThreshold) SnapToTheSurface();
    }

    private void SnapToTheSurface()
    {
        Ray ray = new Ray(transform.position, Vector3.up);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            //Moves to the point of impact + a little higher
            Vector3 targetPosition = hit.point + (Vector3.up * 1f);

            transform.position = targetPosition;

            //Resets the speed and angular velocity
            RB.velocity = Vector3.zero;

            RB.angularVelocity = Vector3.zero;

            Debug.Log("Object moved to top surface: " + hit.collider.name);
        }
        else
        {
            Debug.LogWarning("No surface found above( Object remains falling down... Trying to destroy object.");

            Destroy(this);
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (GetComponent<Rigidbody>().velocity.magnitude >= _takeDmgThreshold)
        {
            if (coll.gameObject.layer == 3) GetHit(-(int) (RB.velocity.magnitude * _weight), DamageType.Blunt, null);
        }
        
        if (GetComponent<Rigidbody>().velocity.magnitude >= dealDmgThreshold)
        {
            if (coll.gameObject.layer == 3 || coll.gameObject.layer == 6 && coll.transform.root != transform.root)
            {
                if (coll.transform.root.GetComponent<IDamageable>() != null)
                {
                    coll.transform.root.GetComponent<IDamageable>().GetHit(-(int) (_weight + RB.velocity.magnitude), DamageType.Blunt, coll.transform);

                    UIManager.instance.PrintMessage(transform.root.name + " hits the " + coll.transform.root.name + ", speed: " + RB.velocity.magnitude);
                }
            }
        }
    }

    public void GetHit(int amount, DamageType type, Transform part)
    {
        SM.PlaySound("GetHit");

        //RB.AddForce(RB.velocity * 10f, ForceMode.Impulse);

        if (type != DamageType.Blunt) amount /= 2;

        if (_maxDurability > 0) ChangeDurability(amount);
    }

    private void ChangeDurability(int value)
    {
        currentDurability += value;

        if (currentDurability <= 0) Break();
    }

    private void Break()
    {
        currentDurability = 0;

        _pieces.gameObject.SetActive(true);
        /*
        for (int i = 0; i < _pieces.childCount; i++)
        {
            _pieces.GetChild(i).SetParent(null);
        }
        */
        SM.PlaySound("Broke");

        Invoke("DiscardParts", _discardingTime);

        foreach (Collider coll in transform.GetChild(0).GetComponents<Collider>()) coll.enabled = false;

        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false; 

        RB.isKinematic = true;

        RB.useGravity = false;

        UIManager.instance.PrintMessage(transform.name + " broke");

        GetComponent<MeshRenderer>().enabled = false;
    }

    private void DiscardParts()
    {
        Destroy(this.gameObject);
    }
}