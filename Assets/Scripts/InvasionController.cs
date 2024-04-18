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
            // Обработка отмены операции
        }
    }

    private async UniTask Invasion(CancellationToken cancellationToken)
    {
        while (gameController.IsGame && _isReady)
        {
            await StartTimer(waveSettings[_currentIndexWave].Cooldown, cancellationToken);

            // Если флаг остановки таймера установлен, выходим из цикла
            if (_shouldStopWave)
            {
                break;
            }

            await UniTask.Yield(); // Добавим задержку между проверками
        }
    }

    public async UniTask StartTimer(float duration, CancellationToken cancellationToken)
    {
        uIController.EnemyesCount.text = waveSettings[_currentIndexWave].Count.ToString();

        float currentTime = 0f;

        while (currentTime < duration)
        {
            // Обновляем readiness и filled в процентах
            UpdateReadiness();
            _filled.fillAmount = currentTime / duration;

            // Ждем один кадр
            await UniTask.Yield(cancellationToken);

            // Если флаг остановки таймера установлен, выходим из цикла
            if (_shouldStopWave)
            {
                break;
            }

            // Обновляем время
            currentTime += Time.deltaTime;
        }

        // Время истекло
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
        // Сначала останавливаем предыдущий таймер, если он был запущен
        StopWave();

        // Снова запускаем таймер для новой волны
        _cancellationTokenSource = new CancellationTokenSource();
        _isReady = true; // Устанавливаем флаг готовности
        _shouldStopWave = false; // Сбрасываем флаг остановки таймера

        // Запускаем новую инвазию
        _ = Invasion(_cancellationTokenSource.Token);
    }

    private void StopWave()
    {
        // Устанавливаем флаг остановки таймера
        _shouldStopWave = true;

        // Отменяем токен, если он активен
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
