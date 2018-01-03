﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Character2D
{
    public class CharacterInteraction : MonoBehaviour
    {
        public InteractionTrigger interactionTrigger;
        public CharacterBehavior characterBehavior;

        public GameObject interactBar; //reference to the interact popup bar that asks for input to interact
        public GameObject interactButton; //reference to the interact button prefab
        public GameObject interactList; //reference to the interact list which contains interact button prefabs
        public GameObject interactContainer; //reference to the interact container which contains the interact list

        public TMP_Text interactBarItem; //reference to the item to interact with
        public TMP_Text interactBarType; //reference to the type of interaction
        public TMP_Text interactBarKey; //reference to the key pressed for the interaction

        public ScrollRect scrollableList; //the default y position of the interact list (to scroll back to the top)

        public bool interactionInput; //whether the character is trying to interact or not

        void Start()
        {
            interactionInput = false;

            interactBar.SetActive(false);
            interactContainer.SetActive(false);
            //TODO: set default interactBarKey
        }

        // Update is called once per frame
        void Update()
        {
            //if the character inputs for an interaction
            if (interactionInput)
            {
                InteractPress();
                interactionInput = false;
            }
        }

        //displays the interaction popup depending on the current item count
        public void DisplayText()
        {
            //if there are no current interactables, don't display the popup
            if (interactionTrigger.currentObjects.Count == 0)
            {
                interactBar.SetActive(false);
            }
            //if there is one interactable, display its unique popup
            else if (interactionTrigger.currentObjects.Count == 1)
            {
                interactBar.SetActive(true);
                interactBarType.text = interactionTrigger.currentObjects[0].GetComponent<InteractableObject>().interactType;
                interactBarItem.text = interactionTrigger.currentObjects[0].GetComponent<InteractableObject>().interactItem;
            }
            //if there exists more than one interactable, display a generic popup
            else
            {
                interactBar.SetActive(true);
                interactBarType.text = "choose interaction";
                interactBarItem.text = "";
            }
        }

        //handles a press of the interact button given the amount of current interactables
        public void InteractPress()
        {
            //if there are no interactables, do nothing
            if (interactionTrigger.currentObjects.Count == 0)
            {
                return;
            }
            //if there is one interactable, interact with it
            else if (interactionTrigger.currentObjects.Count == 1)
            {
                Interact(0);
            }
            //if there exists more than one interactable, show the interact container
            else
            {
                ShowList();
            }
        }

        //shows the list of interactables that the player can select to interact with
        public void ShowList()
        {

            //iterate through the list of interactables spawning buttons on screen in a list
            for (int i = 0; i < interactionTrigger.currentObjects.Count; i++)
            {
                //instantiate a prefab for the interact button
                GameObject newButton = Instantiate(interactButton) as GameObject;
                InteractableObject controller = newButton.GetComponent<InteractableObject>();

                //set the text for the interactable onscreen 
                controller.interactableText.text = interactionTrigger.currentObjects[i].GetComponent<InteractableObject>().interactText;

                //put the interactable in the list
                newButton.transform.SetParent(interactList.transform);

                //for some reason Unity does not use full scale for the instantiated object by default
                newButton.transform.localScale = Vector3.one;
            }
            interactBar.SetActive(false);
            interactContainer.SetActive(true);

            //move the scrollbar back to the top of the list
            scrollableList.verticalNormalizedPosition = 1.0f;

            //pause game time (in part to prevent user input)
            //TODO: decide if we want to keep time paused here or just disable user action/motion input
            Time.timeScale = 0;
            Cursor.visible = true;
        }

        //performs the interaction with the selected item
        public void Interact(int index)
        {
            switch(interactionTrigger.currentObjects[index].GetComponent<InteractableObject>().interactType)
            {
                case "pick up":
                    characterBehavior.AddItem(interactionTrigger.currentObjects[index].GetComponent<InteractableObject>().interactItem);
                    break;
                case "talk":
                    //open talk dialogue
                    break;
                case "open":
                    //toggle item open/closed based on current state
                    break;
                default:
                    Debug.Log("Error: interact type is unknown. Please add its behavior to CharacterInteraction.");
                    break;
            }
            //interact with the selected item
            Debug.Log(interactionTrigger.currentObjects[index].GetComponent<InteractableObject>().interactText);

            //destroy the item, removing it from the game and list
            Destroy(interactionTrigger.currentObjects[index]);
            CloseContainer();
        }

        //cleans up the screen after an interactable is chosen
        public void CloseContainer()
        {
            //unpause time and hide the container
            //TODO: if changed above, change here too
            interactContainer.SetActive(false);
            Time.timeScale = 1;

            //display the interact popup
            DisplayText();

            //delete ui elements from the list for the next iteration
            var children = new List<GameObject>();
            foreach (Transform child in interactList.transform)
            {
                children.Add(child.gameObject);
            }
            children.ForEach(child => Destroy(child));
        }

        //changes the interact key for display
        //TODO: implement
        public void interactKeyChange(string newKey)
        {
            interactBarKey.text = newKey;
        }
    }
}