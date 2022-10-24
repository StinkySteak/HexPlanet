using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject[] Panels;
    bool StartPanel = false;
    public int Order;

    public void PanelClicked()
    {
        StartPanel = true;
        MainPanel.SetActive(true);

        if (Order + 1 > Panels.Length)
        {
            SceneManager.LoadScene("Game");
            return;
        }

        Panels[Order].SetActive(true);
        Order++;
    }
    private void Update()
    {
        if (StartPanel)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
                PanelClicked();
        }
    }
}
