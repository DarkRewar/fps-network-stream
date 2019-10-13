using System;
using System.Collections;
using FPSNetwork;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerNetwork : NetworkBehaviour
{
    public Camera PlayerCam;

    public Transform Weapon;

    public GameObject PlayerGraphics;

    [SyncVar(hook = "OnLifeChanged")]
    public int Lifepoints = 3;

    public float Speed = 0.1f;

    public Slider Lifebar;

    [SyncVar]
    public PlayerInformations Informations;

    [Header("Feedbacks")]
    public GameObject HitPrefab;

    private CharacterController _controller;

    private Vector3 _movement;

    private bool _isPaused = false;

    private Vector3 _weaponPosition;

    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<CharacterController>();

        GameManager.AddPlayer(this);

        _weaponPosition = Weapon.localPosition;

        _animator = PlayerGraphics.GetComponent<Animator>();

        PlayerCam.gameObject.SetActive(isLocalPlayer);

        foreach(Transform t in PlayerGraphics.transform)
        {
            t.gameObject?.SetActive(!isLocalPlayer);
        }

        if (!isLocalPlayer)
        {
            foreach(AudioListener audio in GetComponentsInChildren<AudioListener>())
            {
                audio.enabled = false;
            }
        }
    }

    private void OnEnable()
    {
        if (isLocalPlayer)
        {
            Camera.main.enabled = false;
        }
    }

    private void OnDisable()
    {
        if (isLocalPlayer)
        {
            Camera.main.enabled = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer && !_isPaused)
        {
            _movement = new Vector3(
                Input.GetAxis("Horizontal"),
                -0.1f,
                Input.GetAxis("Vertical")                
            );

            _animator.SetFloat("Horizontal", _movement.x);
            _animator.SetFloat("Vertical", _movement.z);

            transform.Rotate(transform.up, Input.GetAxis("Mouse X"));

            var rot = PlayerCam.transform.localEulerAngles;
            rot.x += Input.GetAxis("Mouse Y");
            //rot.x = Mathf.Clamp(rot.x, -90, 90);
            PlayerCam.transform.localEulerAngles = rot;

            if (Input.GetButtonDown("Fire1"))
            {
                CmdShoot(PlayerCam.transform.position, PlayerCam.transform.forward);
                FeedbackShoot();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer && !_isPaused)
        {
            _controller.Move(transform.TransformDirection(_movement) * Speed);
        }
    }

    private void LateUpdate()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _isPaused = !_isPaused;
            }

            Cursor.lockState = _isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = _isPaused;
        }

        Lifebar.value = Lifepoints;
    }

    /// <summary>
    /// ICI, C'EST COTE CLIENT !
    /// </summary>
    /// <param name="newLife"></param>
    private void OnLifeChanged(int newLife)
    {
        if(newLife <= 0)
        {
            _isPaused = true;
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            _isPaused = false;
        }
    }

    /// <summary>
    /// EXECUTE COTE SERVEUR MAIS APPELE PAR LE CLIENT
    /// </summary>
    [Command]
    public void CmdShoot(Vector3 origin, Vector3 direction)
    {
        Debug.Log("CmdShoot");

        RpcShoot();

        Ray ray = new Ray(origin, direction);
        Debug.DrawRay(origin, direction * 100, Color.magenta, 30);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000))
        {
            RpcShootHit(hit.point, hit.normal);

            if (hit.collider.gameObject.CompareTag("Player"))
            {
                PlayerNetwork player = hit.collider.GetComponentInParent<PlayerNetwork>();
                --player.Lifepoints;

                if (player.Lifepoints <= 0)
                {
                    ++Informations.Kills;
                    ++player.Informations.Deaths;
                    GameManager.Instance.KillPlayer(player);
                }
            }
            else
            {
                Debug.Log(hit.collider.gameObject);
                Rigidbody rigidbody = hit.collider.GetComponent<Rigidbody>();

                Debug.Log(rigidbody);
                if (rigidbody != null) {
                    rigidbody.AddForce(
                        (hit.collider.transform.position - hit.point) * 1000,
                        ForceMode.Impulse
                    );
                }
            }
        }
    }

    [ClientRpc]
    public void RpcShoot()
    {
        // SFX
        if (!isLocalPlayer)
        {
            FeedbackShoot();
        }
    }

    [ClientRpc]
    public void RpcShootHit(Vector3 point, Vector3 normal)
    {
        GameObject go = Instantiate(HitPrefab, point, Quaternion.LookRotation(point, normal));
        Destroy(go, 1);
    }

    private void FeedbackShoot()
    {
        StartCoroutine(DoRecoil());
    }

    private IEnumerator DoRecoil()
    {
        float duration = 0.5f;
        float time = 0;
        Vector3 firstPos = Weapon.localPosition - Vector3.forward * 0.03f;

        while (time < duration)
        {
            Weapon.localPosition = Vector3.Lerp(firstPos, _weaponPosition, time / duration);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Weapon.localPosition = _weaponPosition;
    }
}
