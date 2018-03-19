using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character2D
{
    public class Enemy : Attackable
    {

        [System.Serializable]
        public class BeaconControl
        {
            public GameObject currTarget;
            public GameObject[] beacons;
            public int beaconNum;
        }
        private float maxSpeed;
        private EnemyMovement enemyMovement;
        public AIJumpTrigger topJump;
        public AIJumpTrigger botJump;
        [Header("Beacon System")]
        public BeaconControl beacCon;
        public bool chasingPlayer;

        [Range(0.0f, 100.0f)]
        [Tooltip("How often should the AI stop and look around at each beacon, 0% - 100% (0% never, 100% max).")]
        public float stoppingPercentage; //How often should the AI stop and look around

        [Range(5.0f, 15.0f)]
        [Tooltip("How long(seconds) the AI will follow the player outside of their boxcast")]
        public float giveUpTime;
        private float chaseTime;

        [Header("AI Scanning Masks")]
        [Space(5)]
        [Tooltip("Select the Players layer.")]
        public LayerMask playerMask;

        [Tooltip("Select the default layer or the layer the world is going to be in.")]
        public LayerMask defaultMask;

        [Tooltip("Select the Layer that has all the objects that the player can hide behind.")]
        public LayerMask hideablesMask;

        private LayerMask layers;

        private Vector2Int boxCastDimensions;
        private int boxCastDirection;
        private int boxCastDistance;

        void OnBecameVisible()
        {
            enabled = true;
        }

        void OnBecameInvisible()
        {
            enabled = false;
        }

        //used for initialization
        protected new void Start()
        {
            base.Start();

            boxCastDimensions = new Vector2Int(1, 1);
            boxCastDistance = 5;

            layers = playerMask | defaultMask | hideablesMask;
            enemyMovement = GetComponent<EnemyMovement>();
            canFlinch = false;
            canKnockBack = true;
            canTakeDamage = true;
            chasingPlayer = false;
            beacCon.beaconNum = 0;
            beacCon.currTarget = beacCon.beacons[beacCon.beaconNum];
            chaseTime = giveUpTime;

            enabled = false;
        }

        protected void FixedUpdate()
        {
            RaycastHit2D ray = Physics2D.BoxCast(this.transform.position, boxCastDimensions, 0.0f, transform.right, boxCastDistance, layers.value);

            DrawBox(transform.position, new Vector2(boxCastDimensions.x * boxCastDistance, boxCastDimensions.y));

            if (ray && ray.collider.gameObject.GetComponent<Player>() != null && !chasingPlayer)
            {
                chasingPlayer = true;
                beacCon.currTarget = ray.collider.gameObject;
            }
            else if (chaseTime <= 0)
            {
                chasingPlayer = false;
                if (!enemyMovement.isScanning)
                {
                    StartCoroutine(enemyMovement.StopAndScan());
                    SwitchBeacon();
                }
                chaseTime = giveUpTime;
            }
            else if (chasingPlayer)
            {
                chaseTime -= Time.deltaTime;
            }

            if (chasingPlayer)
            {
                CheckDirection();
            }
        }

        //TODO: remove debug function.
        private void DrawBox(Vector2 position, Vector2 size)
        {
            boxCastDirection = enemyMovement.isFacingRight ? 1 : -1;

            Vector2 origin = position;
            Vector2 upperBound = position + new Vector2(0.0f, size.y);
            Vector2 outerUpperBound = position + new Vector2(size.x * boxCastDirection, size.y);
            Vector2 outerLowerBound = position + new Vector2(size.x * boxCastDirection, 0.0f);
            Debug.DrawLine(origin, outerLowerBound, Color.red, Time.fixedDeltaTime);
            Debug.DrawLine(origin, upperBound, Color.red, Time.fixedDeltaTime);
            Debug.DrawLine(upperBound, outerUpperBound, Color.red, Time.fixedDeltaTime);
            Debug.DrawLine(outerLowerBound, outerUpperBound, Color.red, Time.fixedDeltaTime);
        }

        protected override void InitializeDeath()
        {
            //take away enemy input
            //enemy no longer targets player
            //enemy no longer attackable
            isDying = true;
            anim.SetBool("isDying", isDying); //death animation
        }

        public override void FinalizeDeath()
        {
            //drop loot
            Debug.Log("Enemy died: " + gameObject.name); //TODO: remove debug
            Destroy(gameObject);
        }

        //TODO: change to using fixed update to constantly monitor towards target (for edge cases)
        //TODO: move to a behavior AI class that will take care of figuring out where the enemy needs to go
        public void SwitchBeacon()
        {
            beacCon.beaconNum++;
            beacCon.currTarget = beacCon.beacons[beacCon.beaconNum % beacCon.beacons.Length];

            CheckDirection();
            ShouldScan();
        }

        private void CheckDirection()
        {
            if (beacCon.currTarget.transform.position.x - this.gameObject.transform.position.x < 0 && enemyMovement.isFacingRight)
            {
                enemyMovement.ChangeDirection();
            }
            else if (beacCon.currTarget.transform.position.x - this.gameObject.transform.position.x > 0 && !enemyMovement.isFacingRight)
            {
                enemyMovement.ChangeDirection();
            }
        }

        private void ShouldScan()
        {
            if (UnityEngine.Random.Range(1.0f, 100.0f) <= stoppingPercentage && enemyMovement.isScanning == false)
            {
                enemyMovement.isScanning = true;
                StartCoroutine(enemyMovement.StopAndScan());
            }
        }
    }
}
