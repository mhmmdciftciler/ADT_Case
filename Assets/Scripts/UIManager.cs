using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Replayer _replayer;
    [SerializeField] private Recorder _recorder;
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
    [SerializeField] private Button _recordStartButton;
    [SerializeField] private Button _recordStopButton;
    private bool _isReplayOnSliderSelected;
    private List<string> _replayFileNames;
    private void Start()
    {
        _pauseButton.onClick.AddListener(PauseReplay);
        _playButton.onClick.AddListener(StartReplay);
        _resumeButton.onClick.AddListener(ResumeReplay);
        _nextButton.onClick.AddListener(FastForwardReplayer);
        _previousButton.onClick.AddListener(RewindReplayer);
        _replayDropdown.onValueChanged.AddListener(OnSelectedDropdownItem);
        _playerSlider.onValueChanged.AddListener(SetReplayerCurrentFrameIndex);

        AddEventTrigger(_playerSlider.gameObject, EventTriggerType.Deselect, OnSliderDeselect);
        AddEventTrigger(_playerSlider.gameObject, EventTriggerType.Select, OnSliderSelect);

        _replayer.OnCurrentFrameIndexChanged.AddListener(OnChangeCurrentFrameIndex);

        _recordStartButton.onClick.AddListener(StartRecord);
        _recordStopButton.onClick.AddListener(StopRecord);
        _recorder.OnRecordFrame.AddListener(RecordFrame);
        _recorder.OnRecordStarted.AddListener(OnRecordingStart);
        _recorder.OnRecordStoped.AddListener(OnRecordingStop);
        LoadReplayFiles();
    }
    public void LoadReplayFiles()
    {
        _replayFileNames = FileManager.GetFileNames().ToList();
        _replayDropdown.AddOptions(_replayFileNames);
    }
    private void OnSliderSelect(BaseEventData baseEventData) //button event
    {
        _isReplayOnSliderSelected = _replayer.IsPlaying;
        PauseReplay();
    }
    private void OnSliderDeselect(BaseEventData baseEventData) //button event
    {
        if (_isReplayOnSliderSelected)
        {
            ResumeReplay();
        }
    }
    private void OnRecordingStart()
    {
        _playButton.interactable = false;
        _pauseButton.interactable = false;
        _playerSlider.interactable = false;
        _replayDropdown.interactable = false;
        _nextButton.interactable = false;
        _previousButton.interactable = false;
        if (_replayer.IsPlaying)
            PauseReplay();
    }
    private void OnRecordingStop(ReplayData replayData)
    {
        _playButton.interactable = true;
        _pauseButton.interactable = true;
        _playerSlider.interactable = true;
        _replayDropdown.interactable = true;
        _nextButton.interactable = true;
        _previousButton.interactable = true;
        _replayFileNames.Add("/replay_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        _replayDropdown.ClearOptions();
        _replayDropdown.AddOptions(_replayFileNames);

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
    #region ReplayerEvents
    private void OnChangeCurrentFrameIndex(int index)
    {
        _currentFrameText.text = index.ToString();
        _playerSlider.SetValueWithoutNotify(index);
    }
    private void SetReplayerCurrentFrameIndex(float index)//Slider
    {
        _currentFrameText.text = ((int)index).ToString();
        _replayer.SetReplayerCurrentFrameIndex((int)index);
    }
    private void FastForwardReplayer()
    {
        _replayer.FastForward();
        _currentFrameText.text = _replayer.CurrentFrameIndex.ToString();
        _playerSlider.SetValueWithoutNotify(_replayer.CurrentFrameIndex);
    }
    private void RewindReplayer()
    {
        _replayer.Rewind();
        _currentFrameText.text = _replayer.CurrentFrameIndex.ToString();
        _playerSlider.SetValueWithoutNotify(_replayer.CurrentFrameIndex);
    }
    private void StartReplay()
    {
        if (_replayFileNames.Count == 0) return;

        string filePath = _replayFileNames[_replayDropdown.value];
        _replayClipName.text = filePath;
        _replayer.StartReplay(_replayDropdown.value);
        _totalFrameText.text = _replayer.CurrentReplayData.Frames.Count.ToString();
        _playerSlider.maxValue = _replayer.CurrentReplayData.Frames.Count - 1;
        _playerSlider.SetValueWithoutNotify(0);
        _currentFrameText.text = "0";
        _pauseButton.gameObject.SetActive(true);
        _playButton.gameObject.SetActive(false);
    }
    private void ResumeReplay()
    {
        _replayer.ResumeReplay();
        _playButton.gameObject.SetActive(false);
        _pauseButton.gameObject.SetActive(true);
        _resumeButton.gameObject.SetActive(false);
    }
    private void PauseReplay()
    {
        _replayer.PauseReplay();
        _playButton.gameObject.SetActive(false);
        _resumeButton.gameObject.SetActive(true);
        _pauseButton.gameObject.SetActive(false);
    }
    private void OnSelectedDropdownItem(int index)
    {
        //Play butonuna bastýðý zaman çalýþacaðý için dropdown menü deðiþtiðinde butonlarý start file duruma hazýrlýyor.
        _pauseButton.gameObject.SetActive(false);
        _resumeButton.gameObject.SetActive(false);
        _playButton.gameObject.SetActive(true);
    }
    #endregion
    #region RecorderEvents
    private void StartRecord()
    {
        _recordStopButton.gameObject.SetActive(true);
        _recordStartButton.gameObject.SetActive(false);
        _recorder.StartRecord();
    }
    private void StopRecord()
    {
        _recordStopButton.gameObject.SetActive(false);
        _recordStartButton.gameObject.SetActive(true);
        _recorder.StopRecord();
    }
    private void RecordFrame(int index)
    {
        _totalFrameText.text = index.ToString();
    }
    #endregion
}
