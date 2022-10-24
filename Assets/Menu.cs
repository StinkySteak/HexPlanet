using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public MenuUI MenuUI;
    public void StartGame()
    {
        MenuUI.PanelClicked();
    }
}
