using System;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerNetwork : NetworkBehaviour
{
    public Camera PlayerCam;

    [SyncVar(hook = "OnLifeChanged")]
    public int Lifepoints = 3;

    public float Speed = 0.1f;

    public Slider Lifebar;

    private CharacterController _controller;

    private Vector3 _movement;

    private bool _isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<CharacterController>();

        GameManager.AddPlayer(this);

        if (!isLocalPlayer)
        {
            PlayerCam.enabled = false;
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

            transform.Rotate(transform.up, Input.GetAxis("Mouse X"));

            var rot = PlayerCam.transform.localEulerAngles;
            rot.x += Input.GetAxis("Mouse Y");
            //rot.x = Mathf.Clamp(rot.x, -90, 90);
            PlayerCam.transform.localEulerAngles = rot;

            if (Input.GetButtonDown("Fire1"))
            {
                CmdShoot();
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
    public void CmdShoot()
    {
        Debug.Log("CmdShoot");
        Ray ray = new Ray(PlayerCam.transform.position, PlayerCam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                PlayerNetwork player = hit.collider.GetComponentInParent<PlayerNetwork>();
                --player.Lifepoints;

                if (player.Lifepoints <= 0)
                {
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
}
