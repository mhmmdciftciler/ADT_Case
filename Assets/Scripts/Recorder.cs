using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class Recorder : MonoBehaviour
{
    public static Recorder Instance;
    [SerializeField] private List<CharacterRecorder> _characters;
    [SerializeField] private int _targetFrameRate = 30;
    [SerializeField] private List<CharacterData> _characterDatas;
    [SerializeField] private Button _record;
    [SerializeField] private Button _recordStop;
    [SerializeField] private TextMeshProUGUI _totalFrameCount;
    private List<FrameData> _frames;
    private bool _isRecording = false;
    public UnityEvent<bool> OnRecording;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }
    private void Start()
    {
        _record.onClick.AddListener(StartRecording);
        _recordStop.onClick.AddListener(StopRecording);
        Application.targetFrameRate = _targetFrameRate;
        _frames = new List<FrameData>();
    }

    private void LateUpdate()
    {
        if (_isRecording)
        {
            RecordFrame();
        }
    }

    private void StartRecording()
    {
        _frames.Clear();
        foreach (CharacterRecorder character in _characters)
        {
            character.Animator.enabled = true;
        }
        _isRecording = true;
        OnRecording.Invoke(true);
        _recordStop.gameObject.SetActive(true);
        _record.gameObject.SetActive(false);
    }

    private void StopRecording()
    {
        _isRecording = false;
        SaveRecording();
        OnRecording.Invoke(false);
        _record.gameObject.SetActive(true);
        _recordStop.gameObject.SetActive(false);
        foreach (CharacterRecorder character in _characters)
        {
            character.Animator.enabled = false;
        }
    }

    private void RecordFrame()
    {
        List<CharacterData> characterDatas = new List<CharacterData>();
        for (int i = 0; i < _characters.Count; i++)
        {
            CharacterData characterData = new CharacterData();
            characterData.Save = _characterDatas[i].Save;
            characterData.BoneDataList = new List<BoneData>();
            _characters[i].SerializeCharacterData(characterData);
            characterDatas.Add(characterData);
        }
        FrameData frameData = new FrameData();
        frameData.CharactersData = characterDatas;
        _frames.Add(frameData);
        _totalFrameCount.text = _frames.Count.ToString();
    }

    private void SaveRecording()
    {
        ReplayData replayData = new ReplayData();
        replayData.Frames = _frames;
        string json = JsonUtility.ToJson(replayData, true);
        File.WriteAllText(Application.persistentDataPath + "/replay_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json", json);
        Replayer.Instance.LoadReplayFiles();
    }

}

[System.Serializable]
public struct BoneData
{
    public Vector3 Position;
    public Quaternion Rotation;
}

[System.Serializable]
public struct CharacterData
{
    public List<BoneData> BoneDataList;
    public bool Save;
}

[System.Serializable]
public struct FrameData
{
    public List<CharacterData> CharactersData;
}

[System.Serializable]
public struct ReplayData
{
    public List<FrameData> Frames;
}
