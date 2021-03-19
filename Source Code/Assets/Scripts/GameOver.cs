using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{

    public Text distanceText = null;
    public Text successesText = null;

    private void Awake()
    {
        distanceText.text = "Distance to target: " + GameController.instance.playerDistance.ToString();
        successesText.text = "Encounters finished: " + GameController.instance.playerEncounterSuccesses.ToString();
    }


    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
