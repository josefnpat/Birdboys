using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Birdboys
{
    public class PlayerBird : NetworkBehaviour
    {
        [SyncVar]
        public bool respawnReady = false;
        [SyncVar]
        public bool respawnWait = false;
        [SyncVar]
        public bool respawnTrigger = false;
        [SyncVar]
        public int health = 3;
        private float deathAnimationDt = 0f;
        private float deathAnimationTime = 3f;
        private int healthMax = 3;
        private string healthChar = "♥";

        [SyncVar]
        public string playerName;
        public TextAsset namesFile;

        [SyncVar]
        public int kills = 0;
        [SyncVar]
        public int deaths = 0;
        [SyncVar]
        public int assists = 0;

        private int revolverBulletsCurrent= 6;
        private int revolverBulletsMax = 6;

        private bool revolverFireReady = true;
        private float revolverFireTime = 0.16666666666f;//1 / 6;
        private float revolverFireDt = 0.16666666666f;//1 / 6;

        private float revolverMuzzleFlashTime = 0.16666666666f;//1 / 6;
        private float revolverMuzzleFlashDt = 0f;

        private float revolverReloadTime = 3f; // dont change without remaking reload sfx
        private float revolverReloadDt = 0f;

        private float clampZ = 85f;

        public GameObject birdCamera;
        public GameObject head;
        public GameObject[] playerModelParts;
        public GameObject revolver;
        public GameObject revolverInfo;
        public Transform revolverBulletSpawn;
        public GameObject revolverBulletPrefab;
        public GameObject revolverMuzzleFlash;
        private float moveSpeed = 10;//5
        private float rotateSpeed = 3;//5
        public Rigidbody rigidbody;
        [SyncVar]
        public Quaternion headRotation;
        public Quaternion headRotationLocal;
        public TextMesh nameText;

        public GameObject playerCanvas;
        public GameObject deathPanel;
        public Text healthText;
        public Text kdaText;

        void Start()
        {
            if(isLocalPlayer)
            {
                foreach (GameObject playerModelPart in playerModelParts)
                {
                    playerModelPart.SetActive(false);
                }
                string[] names = namesFile.text.Split(
                    new[] { Environment.NewLine },
                    StringSplitOptions.None
                );
                int index = UnityEngine.Random.Range(0, names.Length);
                CmdNick(names[index]);

                revolverInfo.SetActive(true);
                GameObject.FindWithTag("MainCamera").GetComponent<AudioListener>().enabled = false;
                birdCamera.SetActive(true);
                GetComponent<AudioListener>().enabled = true;

                playerCanvas.SetActive(true);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // this is called on the server
        [Command]
        void CmdNick(string name)
        {
            playerName = name;
        }

        void Update()
        {

            if(isLocalPlayer && respawnTrigger)
            {
                respawnTrigger = false;
                revolverFireReady = true;
                revolverBulletsCurrent = revolverBulletsMax;
            }

            if (isLocalPlayer)
            {
                UpdateRevolverInfo();
                if (transform.position.y < -100)
                {
                    CmdSuicide();
                }
            }

            if (health <= 0)
            {
                deathPanel.SetActive(true);
                deathAnimationDt += Time.deltaTime;
                if (deathAnimationDt >= deathAnimationTime)
                {
                    deathAnimationDt = deathAnimationTime;
                    // seems sometimes the CmdRespawn command doesn't get sent...
                    if (isLocalPlayer)// && !respawnWait)
                    {
                        respawnWait = true;
                        CmdRespawn();
                    }
                }
            }
            else
            {
                deathPanel.SetActive(false);
                deathAnimationDt = 0f;
            }

            if (!isLocalPlayer)
            {
                float scale = 1 - deathAnimationDt / deathAnimationTime;
                foreach (GameObject playerModelPart in playerModelParts)
                {
                    playerModelPart.transform.localScale = new Vector3(scale, scale, scale);
                }
                
            }


            if (isLocalPlayer && health > 0)
            {
                Vector3 rawEuler = head.transform.rotation.eulerAngles + new Vector3(
                    0f,
                    0f,
                    rotateSpeed * Input.GetAxis("Mouse Y")
                );
                // no you're a hack.
                float newZ = rawEuler.z;
                if (newZ < 0)
                {
                    newZ += 360f;
                }
                if (newZ < clampZ | newZ > 360 - clampZ)
                {
                    Quaternion headRotation = Quaternion.Euler(
                        new Vector3(rawEuler.x, rawEuler.y, newZ)
                    );
                    headRotationLocal = headRotation;
                    CmdSetHead(headRotation);
                }
            }

            if (isLocalPlayer)
            {
                head.transform.rotation = headRotationLocal;
            }
            else
            {
                head.transform.rotation = headRotation;
            }

            if(isLocalPlayer)
            {
                UpdatePlayerCanvas();
            }


            if (isLocalPlayer && health > 0)
            {

                // reload system
                bool isReloading = revolverBulletsCurrent == 0 && revolverFireDt > revolverFireTime;
                if (isReloading)
                {
                    revolverReloadDt += Time.deltaTime;
                    if (revolverReloadDt >= revolverReloadTime)
                    {
                        revolverReloadDt = 0f;
                        revolverBulletsCurrent = revolverBulletsMax;
                    }
                }
                if(revolver.activeSelf && isReloading)
                {
                    //Debug.Log("Lazy Reload Trigger");
                    revolverBulletSpawn.GetComponent<AudioSource>().Play();
                }
                revolver.SetActive(!isReloading);

                // todo: make this a server object
                revolverFireDt += Time.deltaTime;
                revolverMuzzleFlash.SetActive(revolverMuzzleFlashDt < revolverMuzzleFlashTime);
                revolverMuzzleFlashDt += Time.deltaTime;

                if (Input.GetMouseButtonUp(0))
                {
                    revolverFireReady = true;
                }

                if (Input.GetMouseButton(0) && revolverFireReady)
                {
                    if (revolverFireDt >= revolverFireTime && revolverBulletsCurrent > 0)
                    {
                        revolverFireReady = false;
                        revolverFireDt = 0;
                        revolverBulletsCurrent--;
                        revolverMuzzleFlashDt = 0;
                        CmdFire();
                    }
                }

                if(Input.GetButton("Reload"))
                {
                    revolverBulletsCurrent = 0;
                }

                if(Input.GetButton("Jump"))
                {

                }

            }

        }

        // syncvars only!~
        public void RespawnSyncVar()
        {
            respawnReady = false;
            respawnWait = false;
            health = healthMax;
        }


        [Command]
        void CmdSuicide()
        {
            health = 0;
        }

        [Command]
        void CmdRespawn()
        {
            Debug.Log("Setting respawn ready on server.");
            respawnReady = true;
            //RpcOnReset();
        }
        /*
        // this is called on the tank that fired for all observers
        [ClientRpc]
        void RpcOnReset()
        {
        }
        */


        // this is called on the server
        [Command]
        void CmdFire()
        {
            GameObject bullet = Instantiate(
                revolverBulletPrefab,
                revolverBulletSpawn.position,
                head.transform.rotation);
            bullet.GetComponent<BulletBird>().source = gameObject;
            NetworkServer.Spawn(bullet);
            RpcOnFire();
        }

        // this is called on the tank that fired for all observers
        [ClientRpc]
        void RpcOnFire()
        {
            //Debug.Log("Someone shot!");
            //animator.SetTrigger("Shoot");
        }

        void UpdateRevolverInfo()
        {
            string s = "";
            if (revolverBulletsCurrent > 0)
            {
                s = "";
                for (int i = 0; i < revolverBulletsCurrent; i++)
                {
                    s += "|";
                }
            }
            else
            {
                int loadingBullets = Mathf.FloorToInt(revolverReloadDt / revolverReloadTime * revolverBulletsMax);
                s = "";
                for (int i = 0; i < loadingBullets; i++)
                {
                    s += ".";
                }
            }
            revolverInfo.GetComponent<TextMesh>().text = s;
        }

        void UpdatePlayerCanvas()
        {
            string s = "";
            for(int i = 0; i < health; i++)
            {
                s += healthChar;
            }
            healthText.text = s;
            kdaText.text = kills + "/" + deaths + "/" + Mathf.Floor(assists/revolverBulletsMax);
        }

        void UpdatePlayerVisuals()
        {
            nameText.text = playerName;
        }

        // need to use FixedUpdate for rigidbody
        void FixedUpdate()
        {

            UpdatePlayerVisuals();

            if (isLocalPlayer && health > 0)
            {

                rigidbody.rotation = Quaternion.Euler(
                    rigidbody.rotation.eulerAngles + new Vector3(
                        0f,
                        rotateSpeed * Input.GetAxis("Mouse X"),
                        0f
                    )
                );

                rigidbody.MovePosition(transform.position +
                    (
                        transform.right * Input.GetAxisRaw("Vertical") +
                        transform.forward * -Input.GetAxisRaw("Horizontal")
                    ) * moveSpeed * Time.fixedDeltaTime
                );

            }
        }

        // this is called on the server
        [Command]
        void CmdSetHead(Quaternion rot)
        {
            headRotation = rot;
        }
    }
}
