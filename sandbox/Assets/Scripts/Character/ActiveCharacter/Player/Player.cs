using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Character2D
{
    public class Player : Attackable
    {
        public static Player instance;
        public static Action<PlayerInfoTuple> OnPlayerInfoChanged;

        public int gold;
        public string currentQuest;
        public string equippedArmor;
        public string equippedWeapon;

        public Animator weaponAnim;

        public CinemachineVirtualCamera virtualCamera;
        private PlayerInput playerInput;
        private TravelMenu travelMenu;

        public Vector2 spawnPoint; //the spawnpoint upon death (one of the fast travel points)
        public SpawnManager spawnManager;

        public TMP_Text healthText;
        public TMP_Text goldText;
        public TMP_Text locationText;
        public TMP_Text questText;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        //used for initialization
        protected new void Start()
        {
            base.Start();

            playerInput = GetComponent<PlayerInput>();
            travelMenu = GetComponent<TravelMenu>();

            healthText.text = currentVitality + "/" + maxVitality;

            //TODO: remove temp AssetBundle loading
            string path = Application.streamingAssetsPath + "/AssetBundles/default";
            AssetBundle.LoadFromFile(path);

            //set from CharacterData
            spawnPoint = transform.position; //todo: set from file
            canFlinch = false;
            canKnockBack = true;
            canTakeDamage = true;

            SetPlayerInfo();
        }

        public void AffectGold(int delta)
        {
            gold += delta;
            InvokePlayerInfoChange();
        }

        private void SetPlayerInfo()
        {
            PlayerInfoTuple playerInfo = GameData.data.saveData.ReadPlayerInfo();
            maxVitality = playerInfo.maxHealth;
            currentVitality = playerInfo.currentHealth;
            currentQuest = playerInfo.currentQuest;
            gold = playerInfo.gold;
            spawnManager = SpawnManager.SetSpawnManager(playerInfo.checkpointName);
            spawnPoint = spawnManager.gameObject.transform.position;
            SetArmor(playerInfo.equippedArmor, isLoad : true);
            SetWeapon(playerInfo.equippedWeapon, isLoad : true);
            TakeDamage(gameObject, 0);

            transform.position = spawnPoint;

            goldText.text = gold.ToString();
            questText.text = currentQuest.ToString();
            locationText.text = spawnManager.persistentLevel.levelInfo.displayName;
        }

        public void SetArmor(string name, bool isLoad = false)
        {
            equippedArmor = name;
            if (!isLoad)
            {
                InvokePlayerInfoChange();
            }
        }

        public void SetWeapon(string name, bool isLoad = false)
        {
            equippedWeapon = name;
            if (!isLoad)
            {
                InvokePlayerInfoChange();
            }
        }

        public void InvokePlayerInfoChange()
        {
            PlayerInfoTuple playerInfo = new PlayerInfoTuple();
            playerInfo.maxHealth = maxVitality;
            playerInfo.currentHealth = currentVitality;
            playerInfo.currentQuest = currentQuest;
            playerInfo.gold = gold;
            playerInfo.checkpointName = spawnManager.managerName;
            playerInfo.equippedArmor = equippedArmor;
            playerInfo.equippedWeapon = equippedWeapon;
            OnPlayerInfoChanged(playerInfo);
        }

        public override void TakeDamage(GameObject attacker, int damage)
        {
            base.TakeDamage(attacker, damage);
            if (currentVitality < 0)
            {
                currentVitality = 0;
            }
            healthText.text = currentVitality + "/" + maxVitality;
            InvokePlayerInfoChange();
        }

        public void Kill()
        {
            currentVitality = 0;
            TakeDamage(gameObject, 0);
        }

        protected override void InitializeDeath()
        {
            if (GetComponent<BackpackMenu>().isOpen)
            {
                GetComponent<BackpackMenu>().CloseBackpackMenu();
            }
            if (GetComponent<Dialogue2D.DialogueManager>().isOpen)
            {
                GetComponent<Dialogue2D.DialogueManager>().EndDialogue();
            }
            if (GetComponent<PlayerInteraction>().isOpen)
            {
                GetComponent<PlayerInteraction>().CloseContainer();
            }
            //take away player input
            ToggleCamera(false);
            playerInput.DisableInput();

            isDying = true;
            anim.SetBool("isDying", isDying);
            weaponAnim.SetBool("isDying", isDying);
            //enemies no longer target player
            StartCoroutine(TravelMenuDelay());
        }

        private IEnumerator TravelMenuDelay()
        {
            yield return new WaitForSeconds(2.0f);
            travelMenu.Open("You died");
        }

        public override void FinalizeDeath()
        {
            //enemies target player
            //give player back input
            //death penalty: 25% of gold?
            playerInput.InvokeSleep();
        }

        public void Respawn()
        {
            isDying = false;
            anim.SetBool("isDying", isDying);
            weaponAnim.SetBool("isDying", isDying);
            currentVitality = maxVitality;
            healthText.text = currentVitality + "/" + maxVitality;
            locationText.text = spawnManager.persistentLevel.levelInfo.displayName;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            transform.position = spawnPoint;
            if (spawnManager != null)
            {
                spawnManager.RefreshLevels();
            }
            StartCoroutine(CameraToggleDelay());
            InvokePlayerInfoChange();
        }

        public void ToggleCamera(bool isActive)
        {
            CameraShift.instance.ToggleDamping(isActive);
            virtualCamera.enabled = isActive;
        }

        public void SetSpawn(Vector2 loc, SpawnManager mgr)
        {
            if (mgr != spawnManager)
            {
                spawnPoint = loc;
                spawnManager = mgr;
                InvokePlayerInfoChange();
            }
        }

        private IEnumerator CameraToggleDelay()
        {
            yield return new WaitForSeconds(0.2f);
            ToggleCamera(true);
        }
    }
}
