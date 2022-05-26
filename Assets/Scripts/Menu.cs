using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void Hesap(string cite)
    {
        Application.OpenURL(cite);
    }
    public void Loadscene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
