using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ButtonBestMoveBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler

{
    private GameObject childText = null;
    void Start()
    {
        TextMeshProUGUI text = GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            childText = text.gameObject;
            childText.SetActive(false);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        childText.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        childText.SetActive(false);
    }
}
