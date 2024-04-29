using System.Collections;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController soundController;
    [SerializeField] private AudioSource audioSourceSFX, audioSourceMusic;
    [SerializeField] private AudioClip musicMenu, musicGame, farmerSpawn, warriorSpawn, enemySpawn, battle, error, buy, drop, click, crim, haha, escape, work, door;

    private void Start()
    {
        soundController = this;
        PlayMenu();
    }

    public void PlayMenu() => CrossFadeLooped(audioSourceMusic, musicMenu);
    public void PlayGame() => CrossFadeLooped(audioSourceMusic, musicGame);
    public void PlayFarmerSpawn() => audioSourceSFX.PlayOneShot(farmerSpawn);
    public void PlayWarriorSpawn() => audioSourceSFX.PlayOneShot(warriorSpawn);
    public void PlayEnemySpawn() => audioSourceSFX.PlayOneShot(enemySpawn);
    public void PlayError() => audioSourceSFX.PlayOneShot(error);
    public void PlayBattle() => audioSourceSFX.PlayOneShot(battle);
    public void PlayBuy() => audioSourceSFX.PlayOneShot(buy);
    public void PlayDrop() => audioSourceSFX.PlayOneShot(drop);
    public void PlayClick() => audioSourceSFX.PlayOneShot(click);
    public void PlayCrim() => audioSourceSFX.PlayOneShot(crim);
    public void PlayHaha() => audioSourceSFX.PlayOneShot(haha);
    public void PlayEscape() => audioSourceSFX.PlayOneShot(escape);
    public void PlayWork() => audioSourceSFX.PlayOneShot(work);
    public void PlayDoor() => audioSourceSFX.PlayOneShot(door);


    private void CrossFadeLooped(AudioSource audioSource, AudioClip newClip)
    {
        // Запустить плавное переключение на новый аудиоклип
        audioSource.loop = false;
        audioSource.clip = newClip;
        audioSource.Play();
        StartCoroutine(FadeIn(audioSource));
    }

    private IEnumerator FadeIn(AudioSource audioSource, float fadeDuration = 1f)
    {
        // Получить текущую громкость
        float startVolume = audioSource.volume;

        // Установить громкость на 0 и начать воспроизведение
        audioSource.volume = 0f;

        // Плавно увеличить громкость до исходной
        while (audioSource.volume < startVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        // Установить флаг зацикливания исходного аудиоклипа
        audioSource.loop = true;
    }
}
