using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RotateLabelBehavior : MonoBehaviour
{
    public List<string> labels;
    private int index = 0;
    private TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.text = labels[index];
    }

    public void ChangeLabel() 
    {
        index ++;
        if (index == labels.Count) 
        {
            index = 0;
        }
        text.text = labels[index];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
