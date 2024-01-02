using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneButton : MonoBehaviour
{
    public void GoToTrainingField()
    {
        SceneManager.LoadScene(1);
    }

    public void GoToDuel()
    {
        SceneManager.LoadScene(0);
    }
}
