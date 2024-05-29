
using System.Collections.Generic;
using UnityEngine;


public class CharacterRecorder : MonoBehaviour
{
    [SerializeField] private Transform _root;
    [SerializeField] private bool _recordRoot;
#if UNITY_EDITOR
    [SerializeField] private string _boneTag;
#endif
    [SerializeField] private GameObject _indicator;//sphere
    [SerializeField] private List<Transform> _bones;//Tekrar tekrar her frame bone aramamak için.
    public Animator Animator;

    public void SerializeCharacterData(CharacterData characterData)
    {
        for (int i = 0; i < _bones.Count; i++)
        {
            if (characterData.Save)
            {
                BoneData boneData = new BoneData();
                boneData.Position = _bones[i].localPosition;
                boneData.Rotation = _bones[i].localRotation;
                characterData.BoneDataList.Add(boneData);
                _indicator.GetComponent<Renderer>().material.color = Color.red;
            }
            else
            {
                _indicator.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }
    public void PlayCharacterFrame(CharacterData characterData)
    {
        if (characterData.Save)
        {
            for (int i = 0; i < _bones.Count; i++)
            {
                Transform bone = _bones[i];
                bone.localPosition = characterData.BoneDataList[i].Position;
                bone.localRotation = characterData.BoneDataList[i].Rotation;
                Debug.Log(characterData.BoneDataList[i].Rotation);
            }
            _indicator.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            _indicator.GetComponent<Renderer>().material.color = Color.white;
        }



    }
#if UNITY_EDITOR
    [ContextMenu("Find Transform With Tag")]
    private void SetCharacterBones()
    {
        foreach (Transform bone in _root.GetComponentsInChildren<Transform>())
        {
            if (!_recordRoot && bone == _root)
            {
                continue;
            }
            if (bone.tag == _boneTag)
            {
                _bones.Add(bone);
            }

        }
    }
#endif
}

