using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public int _i;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetData(int i)
    {
        _i = i;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
