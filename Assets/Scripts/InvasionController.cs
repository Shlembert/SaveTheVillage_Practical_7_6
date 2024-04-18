using Cysharp.Threading.Tasks;
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
    [SerializeField] private float duration;
    [SerializeField] private float moveRight, moveLeft;

    private CancellationTokenSource _cancellationTokenSource;
    private int _currentIndexWave;
    private bool _isReady, _shouldStopWave;
    private Image _filled;

    public async void StartInvasion()
    {
        _isReady = true;
        _currentIndexWave = 0;
        _filled = uIController.InvasionFilled;

        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            await Invasion(_cancellationTokenSource.Token);
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
            await StartTimer(waveSettings[_currentIndexWave].Cooldown, cancellationToken);

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
        uIController.EnemyesCount.text = waveSettings[_currentIndexWave].Count.ToString();

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
        enemyFactory.SetCountSpawnUnit(waveSettings[_currentIndexWave].Count);
        if (_currentIndexWave < waveSettings.Count - 1) _currentIndexWave++;
        else _currentIndexWave = 0;
    }

    private void UpdateReadiness()
    {
        float percentReady = _isReady ? 100f : _filled.fillAmount * 100f;
        // readiness.text = $"{percentReady:0}%";
    }

    public void ShowWave()
    {
        // ������� ������������� ���������� ������, ���� �� ��� �������
        StopWave();

        // ����� ��������� ������ ��� ����� �����
        _cancellationTokenSource = new CancellationTokenSource();
        _isReady = true; // ������������� ���� ����������
        _shouldStopWave = false; // ���������� ���� ��������� �������

        // ��������� ����� �������
        _ = Invasion(_cancellationTokenSource.Token);
    }

    private void StopWave()
    {
        // ������������� ���� ��������� �������
        _shouldStopWave = true;

        // �������� �����, ���� �� �������
        if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
    }

    private void OnDisable()
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
