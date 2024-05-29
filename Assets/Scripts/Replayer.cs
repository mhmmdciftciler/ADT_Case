using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using System;

public class Replayer : MonoBehaviour
{
    public static Replayer Instance;
    public List<CharacterRecorder> _characters;
    [SerializeField] private TMP_Dropdown _replayDropdown;
    [SerializeField] private TextMeshProUGUI _replayClipName;
    [SerializeField] private TextMeshProUGUI _currentFrameText;
    [SerializeField] private TextMeshProUGUI _totalFrameText;
    [SerializeField] private Slider _playerSlider;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _previousButton;
    [SerializeField] private int _skipFrameCount = 15;
    private List<string> _replayFiles;
    private ReplayData _currentReplay;
    private int _currentFrameIndex = 0;
    private bool _isPlaying = false;
    private bool _sliderSelected;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        Recorder.Instance.OnRecording.AddListener(OnRecording);
        _pauseButton.onClick.AddListener(PauseReplay);
        _playButton.onClick.AddListener(StartReplay);
        _resumeButton.onClick.AddListener(ResumeReplay);
        _nextButton.onClick.AddListener(FastForward);
        _previousButton.onClick.AddListener(Rewind);
        _replayDropdown.onValueChanged.AddListener(StartReplay);
        _playerSlider.onValueChanged.AddListener(SetCurrentFramIndex);
        AddEventTrigger(_playerSlider.gameObject, EventTriggerType.Deselect, OnSliderDeselect);
        AddEventTrigger(_playerSlider.gameObject, EventTriggerType.Select, OnSliderSelect);
        LoadReplayFiles();
    }

    void Update()
    {
        if (_isPlaying)
        {
            _currentFrameText.text = _currentFrameIndex.ToString();
            if (!_sliderSelected)
            {
                _playerSlider.SetValueWithoutNotify(_currentFrameIndex);
                PlayFrame();
            }
            else
            {
                PlayFrame(_currentFrameIndex);
            }
        }
    }
    private void AddEventTrigger(GameObject target,
        EventTriggerType eventType,
        UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = target.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    private void OnRecording(bool recording)
    {
        _playButton.interactable = !recording;
        _pauseButton.interactable = !recording;
        _playerSlider.interactable = !recording;
        _replayDropdown.interactable = !recording;
        _nextButton.interactable = !recording;
        _previousButton.interactable = !recording;
        if (recording && _isPlaying)
        {
            PauseReplay();
        }
        Debug.Log("OnRecording : " + recording);
    }
    private void OnSliderSelect(BaseEventData baseEventData) //button event
    {
        _sliderSelected = true;
        Debug.Log("OnSliderSelect");
    }
    private void OnSliderDeselect(BaseEventData baseEventData) //button event
    {
        _sliderSelected = false;
        Debug.Log("OnSliderDeselect");
    }
    public void LoadReplayFiles()
    {
        _replayFiles = new List<string>(Directory.GetFiles(Application.persistentDataPath, "*.json"));
        _replayDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (string file in _replayFiles)
        {
            options.Add(Path.GetFileNameWithoutExtension(file));
        }
        _replayDropdown.AddOptions(options);
    }
    private void StartReplay(int index)
    {
        _pauseButton.gameObject.SetActive(false);
        _resumeButton.gameObject.SetActive(false);
        _playButton.gameObject.SetActive(true);
    }
    private void StartReplay()//button event
    {
        if (_replayFiles.Count == 0) return;

        string filePath = _replayFiles[_replayDropdown.value];
        string json = File.ReadAllText(filePath);

        _currentReplay = JsonUtility.FromJson<ReplayData>(json);
        _currentFrameIndex = 0;
        _replayClipName.text = filePath;
        _totalFrameText.text = _currentReplay.Frames.Count.ToString();
        _playerSlider.maxValue = _currentReplay.Frames.Count - 1;
        _playerSlider.SetValueWithoutNotify(0);
        _currentFrameText.text = 0.ToString();
        _isPlaying = true;
        _pauseButton.gameObject.SetActive(true);
        _playButton.gameObject.SetActive(false);
    }

    private void PlayFrame()
    {
        if (_currentFrameIndex < _currentReplay.Frames.Count)
        {
            PlayFrame(_currentFrameIndex);
            _currentFrameIndex++;
        }
        else
        {
            _currentFrameIndex = 0;
        }
    }
    private void PlayFrame(int frameIndex)
    {
        FrameData frameData = _currentReplay.Frames[frameIndex];
        Debug.Log("FrameIndex : " + frameIndex);
        for (int i = 0; i < _characters.Count; i++)
        {
            CharacterData characterData = frameData.CharactersData[i];
            CharacterRecorder character = _characters[i];
            character.PlayCharacterFrame(characterData);

        }
    }
    private void PauseReplay()//button event
    {

        _isPlaying = false;
        _playButton.gameObject.SetActive(false);
        _resumeButton.gameObject.SetActive(true);
        _pauseButton.gameObject.SetActive(false);
    }

    private void ResumeReplay()//button event
    {
        _isPlaying = true;
        _playButton.gameObject.SetActive(false);
        _pauseButton.gameObject.SetActive(true);
        _resumeButton.gameObject.SetActive(false);
    }

    private void FastForward()//button event
    {
        if (_currentFrameIndex + _skipFrameCount < _currentReplay.Frames.Count)
        {
            _currentFrameIndex += _skipFrameCount;
        }
        else if(_currentFrameIndex == _currentReplay.Frames.Count - 1)
        {
            _currentFrameIndex = _skipFrameCount;
        }
        else
        {
            _currentFrameIndex = _currentReplay.Frames.Count - 1;
        }
        PauseReplay();
        _currentFrameText.text = _currentFrameIndex.ToString();
        _playerSlider.SetValueWithoutNotify(_currentFrameIndex);

        PlayFrame(_currentFrameIndex);
    }

    private void Rewind()//button event
    {
        if (_currentFrameIndex - _skipFrameCount > 0)
        {
            _currentFrameIndex -= _skipFrameCount;
        }
        else if(_currentFrameIndex == 0)
        {
            _currentFrameIndex = _currentReplay.Frames.Count -_skipFrameCount;
        }
        else
        {
            _currentFrameIndex = 0;
        }
        _currentFrameText.text = _currentFrameIndex.ToString();
        _playerSlider.SetValueWithoutNotify(_currentFrameIndex);

        PlayFrame(_currentFrameIndex);
    }
    private void SetCurrentFramIndex(float framerate)//sliderEvent
    {
        _currentFrameIndex = (int)framerate;
        _currentFrameText.text = _currentFrameIndex.ToString();
        PlayFrame(_currentFrameIndex);
    }
    [ContextMenu("Delet All Json Files")]
    private void DeleteReplayFiles()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath, "replay_*.json");

        foreach (string file in files)
        {
            try
            {
                // Dosyayý sil
                File.Delete(file);
                Debug.Log("Deleted replay file: " + file);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to delete replay file: " + file + ". Exception: " + e.Message);
            }
        }
    }
}
