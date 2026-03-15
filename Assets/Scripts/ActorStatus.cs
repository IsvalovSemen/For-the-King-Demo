using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class ActorStatus : MonoBehaviour
{
    [SerializeField] private Creature _owner;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TMP_Text _healthRatioText;
    [SerializeField] private float _dmgHideDelay = 2f;
    [SerializeField] private float _totalDmg;
    [SerializeField] private TMP_Text _dmgCountText;
    public Image alertIndicator;
    [SerializeField] private float _minOffset = 2.5f;
    [SerializeField] private float _maxOffset = 3f;
    private Coroutine _resetCoroutine;

    public void Init(Creature actor)
    {
        _owner = actor;
        _nameText.text = actor.name;
        _dmgCountText.text = string.Empty;
        _canvas.enabled = false;

        actor.OnHealthChange += UpdateHealthBar;
        actor.OnRecieveDamage += UpdateDamageCounter;
    }

    private void LateUpdate()
    {
        float distanceToPlayer = Vector3.Distance(_owner.transform.position, Player.instance.transform.position);

        Vector3 screenPos = CameraControl.instance.mainCam.WorldToScreenPoint(_owner.transform.position); //Projects actor's position in world coordinates onto it's position on screen.

        //float angleToPlayer = Vector3.Angle(CameraControl.instance.transform.forward, (_owner.transform.position - CameraControl.instance.transform.position)); //Angle between target and this actor, checks if Player faces this actor.

        float dotProduct = Vector3.Dot(CameraControl.instance.transform.forward, (_owner.transform.position - CameraControl.instance.transform.position).normalized); //Check if the object looking in roughly the same direction as the camera: if below 0, Player is behind the actor.

        bool actorIsVisible = screenPos.x > 0f && screenPos.x < Screen.width && screenPos.y > 0f && screenPos.y < Screen.height;

        if (!Physics.Linecast(CameraControl.instance.transform.position, _owner.transform.position) && dotProduct > 0)
        {
            transform.position = CameraControl.instance.mainCam.WorldToScreenPoint(new Vector3(_owner.transform.position.x, Mathf.Clamp(distanceToPlayer, _minOffset, _maxOffset), _owner.transform.position.z));

            transform.localScale = new Vector2(Mathf.Clamp(1 / distanceToPlayer, 1, 100), Mathf.Clamp(1 / distanceToPlayer, 1, 100));

            //transform.position = transform.position + new Vector3(0, _HPBarOffset, 0);

            //transform.LookAt(GameMaster.instance.player.transform);

            _canvas.enabled = true;
        }
        else _canvas.enabled = false;
    }

    private void UpdateHealthBar(float curHP, float maxHP)
    {
        _healthSlider.maxValue = maxHP;
        _healthSlider.value = curHP;
        _healthRatioText.text = $"{curHP}/{maxHP}";
    }

    private void UpdateDamageCounter(float dmg)
    {
        _totalDmg += dmg;

        _dmgCountText.text = _totalDmg.ToString();

        if (_resetCoroutine != null) StopCoroutine(_resetCoroutine);

        _resetCoroutine = StartCoroutine(ResetDamageCounter());
    }

    public IEnumerator ResetDamageCounter()
    {
        yield return new WaitForSeconds(_dmgHideDelay);

        _totalDmg = 0;

        _dmgCountText.text = string.Empty;
    }
}
