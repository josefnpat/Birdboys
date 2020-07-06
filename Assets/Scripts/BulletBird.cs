using UnityEngine;

namespace Mirror.Birdboys
{
    [RequireComponent(typeof(AudioSource))]
    public class BulletBird : NetworkBehaviour
    {

        public float destroyAfter = 100;
        public Rigidbody rigidBody;
        public float force = 1000;

        public AudioClip gunshotClip;

        [Header("Game Stats")]
        private int damage = 1;
        public GameObject source;

        public override void OnStartServer()
        {
            Invoke(nameof(DestroySelf), destroyAfter);
        }

        // set velocity for server and client. this way we don't have to sync the
        // position, because both the server and the client simulate it.
        void Start()
        {
            rigidBody.AddForce(transform.right * force);
            rigidBody.rotation = source.GetComponent<PlayerBird>().birdCamera.transform.rotation;
            AudioSource.PlayClipAtPoint(gunshotClip, transform.position);
        }

        // destroy for everyone on the server
        [Server]
        void DestroySelf()
        {
            NetworkServer.Destroy(gameObject);
        }

        // ServerCallback because we don't want a warning if OnTriggerEnter is
        // called on the client
        [ServerCallback]
        void OnTriggerEnter(Collider co)
        {
            //Debug.Log("Hit:" + co.name);
            //Hit another player
            if (co.tag.Equals("Player") && co.gameObject != source)
            {

                PlayerBird bird = co.GetComponent<PlayerBird>();

                if(bird.health > 0)
                {
                    PlayerBird sourceBird = source.GetComponent<PlayerBird>();
                    if (damage >= bird.health)
                    {
                        sourceBird.kills++;
                        bird.deaths++;
                    }
                    else
                    {
                        sourceBird.assists++;
                    }
                    bird.health = Mathf.Max(0, bird.health - damage);
                }

            }

            NetworkServer.Destroy(gameObject);
        }
    }
}
