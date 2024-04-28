using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [Space]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameWinPanel;
    [SerializeField] private GameObject gameOverPanel;
    [Space]
    [SerializeField] private TMP_Text farmersCount;
    [SerializeField] private TMP_Text warriorsCount;
    [SerializeField] private TMP_Text foodCount;
    [SerializeField] private TMP_Text enemyesCount;
    [SerializeField] private TMP_Text waveNumber;
    [Space]
    [SerializeField] private TMP_Text farmerCreate;
    [SerializeField] private TMP_Text farmerDead;
    [SerializeField] private TMP_Text warriorCreate;
    [SerializeField] private TMP_Text warriorDead;
    [SerializeField] private TMP_Text enemyCreate;
    [SerializeField] private TMP_Text enemyDead;
    [SerializeField] private TMP_Text assaultCount;
    [SerializeField] private TMP_Text foodStorage;
    [SerializeField] private TMP_Text foodLost;
    [SerializeField] private Image invasionFilled;

    public Image InvasionFilled { get => invasionFilled; set => invasionFilled = value; }
    public TMP_Text EnemyesCount { get => enemyesCount; set => enemyesCount = value; }
    public TMP_Text WaveNumber { get => waveNumber; set => waveNumber = value; }

    public void DisplayTopCount(int value, TypeUnit typeUnit)
    {
        TMP_Text currentTMP = null;

        switch (typeUnit)
        {
            case TypeUnit.Farmer:
                currentTMP = farmersCount;
                break;
            case TypeUnit.Warrior:
                currentTMP = warriorsCount;
                break;
            case TypeUnit.Food:
                currentTMP = foodCount;
                break;
            default:
                break;
        }

        currentTMP.text = value.ToString();
    }

    public void PauseOn()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void PauseOff()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1;
    }
}
