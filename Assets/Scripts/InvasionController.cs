using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class WaveSetting
{
    public float Cooldown;
    public int Count;
}

public class InvasionController : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private UIController uIController;
    [SerializeField] private EnemyFactory enemyFactory;
    [SerializeField] private List<WaveSetting> waveSettings;
    [Space]
    [SerializeField] private Transform indicator;
    [SerializeField] private float duration, moveRight, moveLeft;

    [SerializeField] private Image _filled;

    private CancellationTokenSource _cancellationTokenSource;

    private int _currentIndexWave;
    private bool _isReady, _shouldStopWave, _lastWave;

    public int CurrentIndexWave { get => _currentIndexWave; set => _currentIndexWave = value; }

    public async void StartInvasion()
    {
        _isReady = true;
        _lastWave = false;

        CurrentIndexWave = 0;
        _filled = uIController.InvasionFilled;

        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            await UniTask.Delay(3000);
            ShowWave();
        }
        catch (OperationCanceledException)
        {
            // ��������� ������ ��������
        }
    }

    private async UniTask Invasion(CancellationToken cancellationToken)
    {
        while (gameController.IsGame && _isReady)
        {
            await StartTimer(waveSettings[CurrentIndexWave].Cooldown, cancellationToken);

            // ���� ���� ��������� ������� ����������, ������� �� �����
            if (_shouldStopWave)
            {
                break;
            }

            await UniTask.Yield(); // ������� �������� ����� ����������
        }
    }

    public async UniTask StartTimer(float duration, CancellationToken cancellationToken)
    {
        float currentTime = 0f;

        while (currentTime < duration)
        {
            // ��������� readiness � filled � ���������
            UpdateReadiness();
            _filled.fillAmount = currentTime / duration;

            // ���� ���� ����
            await UniTask.Yield(cancellationToken);

            // ���� ���� ��������� ������� ����������, ������� �� �����
            if (_shouldStopWave)
            {
                break;
            }

            // ��������� �����
            currentTime += Time.deltaTime;
        }

        // ����� �������
        StopWave();
        SoundController.soundController.PlayEnemySpawn();
        enemyFactory.SetCountSpawnUnit(waveSettings[CurrentIndexWave].Count);

        if (CurrentIndexWave < waveSettings.Count -1)
        {
            CurrentIndexWave++;
           // Debug.Log(_currentIndexWave);
        }
        else
        {
            gameController.LastWave = true;
            _lastWave = true;
            Debug.Log("Last Wave!");
        }
    }

    private void UpdateReadiness()
    {
        float percentReady = _isReady ? 100f : _filled.fillAmount * 100f;
    }

    public void ShowWave()
    {
        // ������� ������������� ���������� ������, ���� �� ��� �������
        StopTimer();

        if (_lastWave) return;

        uIController.EnemyesCount.text = waveSettings[CurrentIndexWave].Count.ToString();
        uIController.WaveNumber.text = (CurrentIndexWave + 1).ToString();

        indicator.DOMoveX(moveLeft, duration, false).SetEase(Ease.OutBack, 0.8f).OnComplete(() =>
         {
             // ����� ��������� ������ ��� ����� �����
             _cancellationTokenSource = new CancellationTokenSource();
             _isReady = true; // ������������� ���� ����������
             _shouldStopWave = false; // ���������� ���� ��������� �������

             // ��������� ����� �������

             _ = Invasion(_cancellationTokenSource.Token);
         });
    }

    public void StopWave()
    {
        // ������������� ���� ��������� �������
        _filled.fillAmount = 0f;
        _shouldStopWave = true;
        StopTimer();
        indicator.DOMoveX(moveRight, duration, false).SetEase(Ease.InBack, 0.8f);
    }

    private void StopTimer()
    {
        _shouldStopWave = true;
        // �������� �����, ���� �� �������
        if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
    }

    private void OnDisable()
    {
        StopTimer();
    }
}
