using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
    public bool isRewinding;
    public List<Vector3> positions = new List<Vector3>();
    public List<Quaternion> rotations = new List<Quaternion>();
    public Transform saveParent;
    private List<Transform> myBones = new List<Transform>();
    private Transform myRootBone;
    private Mesh myMesh;
    private Transform rootParent;
    private Material[] myMaterial;
    public bool collapsed;
    SkinnedMeshRenderer myRenderer;

    void Start()
    {
        //Invoke("FallApart", 1f);

        saveParent = transform.parent;

        myRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    public void Dismember()
    {
        //GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

        //GetComponent<Rigidbody>().isKinematic = true;

        myMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;

        myMaterial = GetComponent<SkinnedMeshRenderer>().sharedMaterials;

        GetComponent<SkinnedMeshRenderer>().enabled = false;

        GetComponent<MeshFilter>().sharedMesh = myMesh;

        GetComponent<MeshRenderer>().sharedMaterials = myMaterial;

        transform.SetParent(null);
    }

    public void FallApart()
    {
        collapsed = true;

        Invoke("GatherBones", 3f);
    }

    void Resurrect()
    {
        Destroy(GetComponent<MeshFilter>());

        Destroy(GetComponent<MeshRenderer>());

        //GetComponent<SkinnedMeshRenderer>().enabled = true;

        GetComponent<SkinnedMeshRenderer>().sharedMesh = myMesh;

        myBones.Clear();

        GetComponent<SkinnedMeshRenderer>().rootBone = myRootBone;

        foreach (Transform element in GetComponent<SkinnedMeshRenderer>().bones) myBones.Add(element);

        myRootBone = GetComponent<SkinnedMeshRenderer>().rootBone;

        GetComponent<SkinnedMeshRenderer>().bones = myRenderer.bones;

        GetComponent<SkinnedMeshRenderer>().rootBone = myRenderer.rootBone;

        transform.SetParent(saveParent);

        collapsed = false;

        StopBonesRewind();

        positions.Clear();

        rotations.Clear();

        //GetComponent<Rigidbody>().isKinematic = false;

        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }

    void GatherBones()
    {
        isRewinding = true;

        //GetComponent<Rigidbody>().isKinematic = true;

        Invoke("Resurrect", 3f);
    }

    void RewindBones()
    {
        if (positions.Count > 0 || rotations.Count > 0)
        {
            transform.position = positions[0];

            transform.rotation = rotations[0];

            positions.RemoveAt(0);

            rotations.RemoveAt(0);
        }
        else StopBonesRewind();
    }

    void StopBonesRewind()
    {
        isRewinding = false;
    }

    void FixedUpdate()
    {
        if (collapsed)
        {
            if (isRewinding) RewindBones();
            else RecordBones();
        }      
    }

    void RecordBones()
    {
        if (positions.Count > Mathf.Round(5f / Time.fixedDeltaTime) || rotations.Count > Mathf.Round(5f / Time.fixedDeltaTime))
        {
            positions.RemoveAt(positions.Count - 1);

            rotations.RemoveAt(rotations.Count - 1);
        }

        positions.Insert(0, transform.position);

        rotations.Insert(0, transform.rotation);
    }
}
