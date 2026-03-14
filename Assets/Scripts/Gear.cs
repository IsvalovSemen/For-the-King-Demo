using System.Collections.Generic;
using UnityEngine;

public class Gear : Item, IEquipment
{
    private Transform _bodyMesh;
    private List<Transform> _bones = new List<Transform>();
    private Transform _rootBone;
    private Mesh _mesh;
    private Material[] _myMaterial;
    private Transform _rootParent;

    public override void Awake()
    {
        base.Awake();

        ConvertToRegularMesh();
    }

    private void ConvertToRegularMesh()
    {
        RigidBody.isKinematic = false;

        RigidBody.useGravity = true;

        RigidBody.freezeRotation = false;

        _mesh = transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().sharedMesh;

        _myMaterial = transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().sharedMaterials;

        //Destroy(transform.GetComponent<SkinnedMeshRenderer>());

        transform.GetChild(0).transform.GetComponent<SkinnedMeshRenderer>().enabled = false;

        transform.GetChild(0).gameObject.AddComponent<MeshFilter>();

        transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = _mesh;

        transform.GetChild(0).gameObject.AddComponent<MeshRenderer>();

        transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterials = _myMaterial;
        /*
        for (int i = 0; i < myBones.Count; i++)
        {
            GetComponent<SkinnedMeshRenderer>().bones[i] = myBones[i];
        }

        GetComponent<SkinnedMeshRenderer>().rootBone = myRootBone;*/

        _bones.Clear();
    }

    public void ConvertToSkinnedMesh()
    {
        Destroy(transform.GetChild(0).GetComponent<MeshFilter>());

        Destroy(transform.GetChild(0).GetComponent<MeshRenderer>());

        transform.GetChild(0).transform.gameObject.AddComponent<SkinnedMeshRenderer>();

        transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().sharedMesh = _mesh;

        _rootParent = transform.root.transform;

        for (int i = 0; i < _rootParent.childCount; i++)
        {
            if (_rootParent.GetChild(i).gameObject.GetComponentInChildren<SkinnedMeshRenderer>() != null)
            {
                _bodyMesh = _rootParent.GetChild(i);

                break;
            }
        }

        _bones.Clear();

        transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().rootBone = _rootBone;

        foreach (Transform element in transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().bones) _bones.Add(element);

        _rootBone = transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().rootBone;

        transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().bones = _bodyMesh.GetComponent<SkinnedMeshRenderer>().bones;

        transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().rootBone = _bodyMesh.GetComponent<SkinnedMeshRenderer>().rootBone;
    }

    private void Equip()
    {
        RigidBody.useGravity = false;

        transform.gameObject.SetActive(true);

        ConvertToSkinnedMesh();

        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;


        transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = true;

        //GetComponent<CapsuleCollider>().center = gameObject.GetComponent<SkinnedMeshRenderer>().bounds.center;

        RigidBody.isKinematic = true;

        //transform.position = GetComponent<CapsuleCollider>().bounds.center;

        //rbody.useGravity = false;

        //rbody.constraints = RigidbodyConstraints.FreezePosition;

        //rbody.freezeRotation = true;

        //rbody.constraints = RigidbodyConstraints.FreezeRotation;

        //GetComponent<Collider>().enabled = false;

        //GetComponent<CapsuleCollider>().center = gameObject.transform.position;

        //GetComponent<CapsuleCollider>().transform.rotation = gameObject.GetComponent<SkinnedMeshRenderer>().transform.rotation;

        //transform.position = GetComponent<SkinnedMeshRenderer>().transform.position;

        //GetComponent<CapsuleCollider>().center = gameObject.transform.position;

        //GetComponent<CapsuleCollider>().center = new Vector3(0, 0, 0);
    }

    private void Unequip()
    {
        ConvertToRegularMesh();
    }
}
