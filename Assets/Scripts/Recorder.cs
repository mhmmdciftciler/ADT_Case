using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class Recorder : MonoBehaviour
{
    public static Recorder Instance { get; private set; }
    [SerializeField] private List<Character> _characters;
    [SerializeField] private int _targetFrameRate = 30;
    private List<FrameData> _frames;
    private bool _isRecording = false;
    public UnityEvent<ReplayData> OnRecordStoped;
    public UnityEvent OnRecordStarted;
    public UnityEvent<int> OnRecordFrame;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
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

    public void StartRecord()
    {
        _frames.Clear();
        _isRecording = true;
        OnRecordStarted.Invoke();

    }

    public void StopRecord()
    {
        _isRecording = false;
        SaveRecording();
    }

    private void RecordFrame()
    {
        List<CharacterData> characterDatas = new List<CharacterData>();
        for (int i = 0; i < _characters.Count; i++)
        {
            CharacterData characterData = _characters[i].GetCharacterData();
            characterDatas.Add(characterData);
        }
        FrameData frameData = new FrameData();
        frameData.CharactersData = characterDatas;
        _frames.Add(frameData);
        OnRecordFrame.Invoke(_frames.Count);
    }

    private void SaveRecording()
    {
        ReplayData replayData = new ReplayData();
        replayData.Frames = _frames;
        FileManager.SaveReplayData(replayData);
        OnRecordStoped.Invoke(replayData);
    }

}





