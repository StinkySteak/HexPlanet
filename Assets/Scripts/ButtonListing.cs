using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonListing : MonoBehaviour
{
    public Button Button;
    public TMP_Text Title;
    public TMP_Text Description;

    public Transform ResourceTextParent;

    public TMP_Text ResourceTextPrefab;

    public void AppendResource(string name, int value)
    {
        if (value == 0)
            return;

        var newText = Instantiate(ResourceTextPrefab, ResourceTextParent);

        newText.text = $"{value} {name}";
    }
}
