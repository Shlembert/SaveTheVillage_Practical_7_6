using DG.Tweening;
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
    [SerializeField] private TMP_Text readinessFarmer;
    [SerializeField] private TMP_Text readinessWarrior;
    [SerializeField] private TMP_Text readinessEnemy;
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
    [Space]
    [SerializeField] private Image farmerFilled;
    [SerializeField] private Image warriorFilled;
    [SerializeField] private Image enemyFilled;

    public void DisplayTopCount(int value, TypeUnit typeUnit)
    {
        TMP_Text currentTMP = null;

        switch (typeUnit)
        {
            case TypeUnit.Farmer: currentTMP = farmersCount;
                break;
            case TypeUnit.Warrior: currentTMP = warriorsCount;
                break;
            case TypeUnit.Food: currentTMP = foodCount;
                break;
            default:
                break;
        }

        currentTMP.text = value.ToString();
        AnimChangeCount(currentTMP.transform);
    }

    private void AnimChangeCount(Transform textTransform)
    {
        textTransform.DOKill();
        textTransform.DOScale(textTransform.localScale * 2, 0.3f).SetEase(Ease.InBack).From();
    }
}
