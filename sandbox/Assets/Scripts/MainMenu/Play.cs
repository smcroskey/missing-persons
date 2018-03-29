using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Play : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(()=> LoadGame());
    }

    // Update is called once per frame
    void LoadGame()
    {
        SceneManager.LoadScene("Persistent-SC");
    }

    void Awake()
    {
        PlayerPrefs.SetString("CurrentSave", "TestDatabase.db");
    }
}
