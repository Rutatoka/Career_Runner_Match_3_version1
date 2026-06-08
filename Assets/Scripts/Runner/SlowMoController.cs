using UnityEngine;
using System.Collections;

public class SlowMoController : MonoBehaviour
{
    public static SlowMoController Instance;

    public float slowScale = 0.4f;
    public float choiceTime = 5f;
    private int leftChoiceLane;
    private int rightChoiceLane;

    private bool choosing = false;
    private bool laneChanged = false;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerChoice(int leftLane, int rightLane)
    {
        if (choosing) return;
        if (RunnerController.Instance.State != RunnerState.Running)
            return;

        leftChoiceLane = leftLane;
        rightChoiceLane = rightLane;

        laneChanged = false;

        if (RunnerController.Instance?.player != null)
            RunnerController.Instance.player.OnLaneChanged += OnLaneChanged;

        StartCoroutine(ChoiceRoutine());
    }

    private void OnLaneChanged(int lane)
    {
        // выбор сделан ТОЛЬКО если игрок ушёл в одну из профессий
        if (lane == leftChoiceLane || lane == rightChoiceLane)
            laneChanged = true;
    }

    private IEnumerator ChoiceRoutine()
    {
        choosing = true;

        float originalScale = Time.timeScale;

        // включаем слоу‑мо сразу
        Time.timeScale = slowScale;
        yield return new WaitForEndOfFrame(); // ← гарантирует, что событие не проскочит

        float timer = choiceTime;

        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;

            if (laneChanged)
                break;

            yield return null;
        }

        Time.timeScale = originalScale;
        choosing = false;

        if (RunnerController.Instance?.player != null)
            RunnerController.Instance.player.OnLaneChanged -= OnLaneChanged;
    }


    public void TriggerPortalSlowMo()
    {

        StartCoroutine(PortalSlowRoutine());

    }

    private IEnumerator PortalSlowRoutine()
    {
        if (choosing) yield break; // не перебивать развилку

        float originalScale = Time.timeScale;

        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = originalScale;
    }

    public void ForceEndSlowMo()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        choosing = false;

        if (RunnerController.Instance?.player != null)
            RunnerController.Instance.player.OnLaneChanged -= OnLaneChanged;
    }
}
