using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EditGrounder : MonoBehaviour
{
    public bool Ground = true;
    public LayerMask mask;
    public float yOffset;

    private void Update()
    {
        if (Ground)
        {
            Ground = false;
            GroundObject();
        }
    }

    private void GroundObject()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, Mathf.Infinity, mask);
        foreach (var hit in hits)
        {
            print(hit.transform.name);
        }
        if (hits.Length > 0)
        {
            transform.position = hits[0].point + new Vector3(0, yOffset, 0);
        }
    }
}
