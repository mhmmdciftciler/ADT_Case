
using System.Collections.Generic;
using UnityEngine;


public class Character : MonoBehaviour
{
    public bool isCharacterDataSave;
    [SerializeField] private Transform _root;
    [SerializeField] private bool _recordRoot;
#if UNITY_EDITOR
    [SerializeField] private string _boneTag = "Recordable";
#endif
    [SerializeField] private GameObject _indicator;//sphere
    [SerializeField] private List<Transform> _bones;//Tekrar tekrar her frame bone aramamak için.
    [SerializeField] private Animator Animator;
    private void Start()
    {
        Recorder.Instance.OnRecordStarted.AddListener(StartRecord);
        Recorder.Instance.OnRecordStoped.AddListener(StopRecord);
    }
    public CharacterData GetCharacterData()
    {
        CharacterData characterData = new CharacterData();
        characterData.TransformData = new List<TransformData>();
        characterData.isCharacterDataSave = isCharacterDataSave;
        
        for (int i = 0; i < _bones.Count; i++)
        {
            if (isCharacterDataSave)
            {
                TransformData boneData = new TransformData();
                boneData.Position = _bones[i].localPosition;
                boneData.Rotation = _bones[i].localRotation;
                characterData.TransformData.Add(boneData);
                _indicator.GetComponent<Renderer>().material.color = Color.red;
            }
            else
            {
                _indicator.GetComponent<Renderer>().material.color = Color.white;
            }
        }
        return characterData;
    }
    public void SetCharacterData(CharacterData characterData)
    {
        if (characterData.isCharacterDataSave)
        {
            for (int i = 0; i < _bones.Count; i++)
            {
                Transform bone = _bones[i];
                bone.localPosition = characterData.TransformData[i].Position;
                bone.localRotation = characterData.TransformData[i].Rotation;
            }
            _indicator.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            _indicator.GetComponent<Renderer>().material.color = Color.white;
        }



    }
    private void StartRecord()
    {
        Animator.enabled = true;
    }
    private void StopRecord(ReplayData replayData)
    {
        Animator.enabled = false;
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

