using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

public class Replayer : MonoBehaviour
{
    public List<Character> _characters;

    [SerializeField] private int _skipFrameCount = 15;
    private List<ReplayData> _replayDatas;
    public ReplayData CurrentReplayData { get; private set; }
    private int _currentFrameIndex = 0;
    public bool IsPlaying { get; private set; }

    public UnityEvent OnStartReplay;
    public UnityEvent OnPauseReplay;
    public UnityEvent OnResumeReplay;
    public UnityEvent<int> OnCurrentFrameIndexChanged;

    public int CurrentFrameIndex => _currentFrameIndex;
    private void Start()
    {
        _replayDatas = FileManager.GetReplayDatas();
        Recorder.Instance.OnRecordStoped.AddListener(OnRecordStoped);
    }
    void Update()
    {
        if (IsPlaying)
        {
            PlayFrame();
        }
    }

    public void StartReplay(int index)
    {
        CurrentReplayData = _replayDatas[index];
        _currentFrameIndex = 0;
        OnCurrentFrameIndexChanged.Invoke(_currentFrameIndex);
        IsPlaying = true;
        OnStartReplay.Invoke();
    }

    private void PlayFrame()
    {
        if (_currentFrameIndex < CurrentReplayData.Frames.Count)
        {
            PlayFrame(_currentFrameIndex);
            _currentFrameIndex++;
        }
        else
        {
            _currentFrameIndex = 0;
        }
        OnCurrentFrameIndexChanged.Invoke(_currentFrameIndex);
    }
    private void PlayFrame(int frameIndex)
    {
        FrameData frameData = CurrentReplayData.Frames[frameIndex];
        for (int i = 0; i < _characters.Count; i++)
        {
            CharacterData characterData = frameData.CharactersData[i];
            Character character = _characters[i];
            character.SetCharacterData(characterData);
        }
    }
    public void PauseReplay()
    {
        IsPlaying = false;
        OnPauseReplay.Invoke();
    }

    public void ResumeReplay()
    {
        IsPlaying = true;
        OnResumeReplay.Invoke();
    }

    public void FastForward()
    {
        if (_currentFrameIndex + _skipFrameCount < CurrentReplayData.Frames.Count)
        {
            _currentFrameIndex += _skipFrameCount;
        }
        else if (_currentFrameIndex == CurrentReplayData.Frames.Count - 1)
        {
            _currentFrameIndex = _skipFrameCount;
        }
        else
        {
            _currentFrameIndex = CurrentReplayData.Frames.Count - 1;
        }

        OnCurrentFrameIndexChanged.Invoke(_currentFrameIndex);
        PlayFrame(_currentFrameIndex);
    }

    public void Rewind()
    {
        if (_currentFrameIndex - _skipFrameCount > 0)
        {
            _currentFrameIndex -= _skipFrameCount;
        }
        else if (_currentFrameIndex == 0)
        {
            _currentFrameIndex = CurrentReplayData.Frames.Count - _skipFrameCount;
        }
        else
        {
            _currentFrameIndex = 0;
        }

        PlayFrame(_currentFrameIndex);
    }
    public void SetReplayerCurrentFrameIndex(int framerate)
    {
        _currentFrameIndex = framerate;
        OnCurrentFrameIndexChanged.Invoke(_currentFrameIndex);
        PlayFrame(_currentFrameIndex);
    }

    private void OnRecordStoped(ReplayData replayData)
    {
        _replayDatas.Add(replayData);
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
