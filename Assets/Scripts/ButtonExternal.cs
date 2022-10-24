using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonExternal : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static bool OnButton;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        OnButton = true;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        OnButton = false;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        AudioManager.Instance.PlayAudio("button");
    }
}
