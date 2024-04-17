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

    private CancellationTokenSource _cancellationTokenSource;
    private int _currentIndexWave;
    private bool _isReady;
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
            uIController.WaveNumber.text = (_currentIndexWave +1).ToString();
            await StartTimer(waveSettings[_currentIndexWave].Cooldown);

            await UniTask.Yield(cancellationToken); // Добавим задержку между проверками
        }
    }

    public async UniTask StartTimer(float duration)
    {
        uIController.EnemyesCount.text = waveSettings[_currentIndexWave].Count.ToString();

        float currentTime = 0f;
       
        while (currentTime < duration)
        {
            // Обновляем readiness и filled в процентах
            UpdateReadiness();
            _filled.fillAmount = currentTime / duration;

            // Ждем один кадр
            await UniTask.Yield();

            // Обновляем время
            currentTime += Time.deltaTime;
        }

        // Время истекло
       // StopWave();
        enemyFactory.SetCountSpawnUnit(waveSettings[_currentIndexWave].Count);
        if (_currentIndexWave < waveSettings.Count-1) _currentIndexWave++;
        else _currentIndexWave = 0;
    }

    private void UpdateReadiness()
    {
        float percentReady = _isReady ? 100f : _filled.fillAmount * 100f;
        // readiness.text = $"{percentReady:0}%";
    }

    public void ShowWave()
    {
        _isReady = true;
    }

    private void StopWave() 
    { 
        _isReady = false;
    }

    private void OnDisable()
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
