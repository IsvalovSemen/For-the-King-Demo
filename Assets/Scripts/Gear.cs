using System.Collections.Generic;
using UnityEngine;

public class Gear : Item, IEquipment
{
    private SkinnedMeshRenderer _mySkinnedMesh;
    private List<Transform> _bones = new List<Transform>();
    private Transform _rootBone;
    private Mesh _myMesh;
    private Material[] _myMaterial;

    public override void Awake()
    {
        base.Awake();

        //ConvertToRegularMesh();

        _mySkinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();

        _myMesh = _mySkinnedMesh.sharedMesh;
    }

    public void Equip()
    {
        Destroy(GetComponentInChildren<Rigidbody>());

        foreach (var collider in (GetComponentsInChildren<Collider>()))
        {
            Destroy(collider);
        }

        _mySkinnedMesh.sharedMesh = _myMesh;

        var targetSkinnedMesh = transform.parent.GetComponent<SkinnedMeshRenderer>();

        _bones.Clear();

        _mySkinnedMesh.rootBone = targetSkinnedMesh.rootBone;

        foreach (Transform element in _mySkinnedMesh.bones) _bones.Add(element);

        _mySkinnedMesh.bones = targetSkinnedMesh.bones;

        _mySkinnedMesh.rootBone = targetSkinnedMesh.rootBone;
    }

    public void Unequip()
    {
        Destroy(this.gameObject);
    }
}
