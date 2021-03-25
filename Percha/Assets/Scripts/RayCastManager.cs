using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastManager : MonoBehaviour
{
    public GameObject index_ignore;

    private Transform _selection;
    public static Vector3 lineDistance;
    private bool okey;
    private Renderer selectionRend;

    // Start is called before the first frame update
    void Start()
    {
        lineDistance  = new Vector3(0,0.5f,0);
        okey = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_selection != null)
        {
            var selectionRenderer = _selection.GetComponent<Renderer>();
            selectionRenderer.material.color = Color.gray;
            lineDistance  = new Vector3(0,0.5f,0);
            if (AddInitialScript.gestID != 2)
            {
                 StartCoroutine(Delay());
            }
            _selection = null;
        }
        
        var ray = new Ray(index_ignore.transform.position, index_ignore.transform.right);
        Debug.DrawRay(index_ignore.transform.position, index_ignore.transform.right);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var selected = hit.transform;
            if (selected.tag == "Selectable" & AddInitialScript.gestID == 2)
            {
                var selectionRenderer = selected.GetComponent<Renderer>();
                lineDistance = selected.position - index_ignore.transform.position;
                if (selectionRenderer != null)
                {
                    selectionRenderer.material.color = Color.yellow;
                }

                _selection = selected;
            }
        }

        if (okey = true & AddInitialScript.gestID == 1)
        {
            selectionRend.material.color = Color.blue;
        }
    }

    IEnumerator Delay()
    {
        selectionRend = _selection.GetComponent<Renderer>();
        selectionRend.material.color = Color.yellow;
        okey = true;
        yield return new WaitForSeconds(3);
        selectionRend.material.color = Color.gray;
        okey = false;
    }
}
