using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnTravelUIListing : MonoBehaviour
{
    [HideInInspector] public string ResourceId;
    [HideInInspector] public int CurrentAmount;
    public TMP_Text Text;
    public Image Image;

    public void SetNewValue(int _amount)
    {
        CurrentAmount = _amount;    
    }
}
