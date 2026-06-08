using UnityEngine;
using System;

public enum RunnerState
{
    Idle,
    Running,
    Dead,
    Paused,
    InChallenge
}

public class RunnerController : MonoBehaviour
{
    public static RunnerController Instance;

    [Header("References")]
    public PlayerController player;
    public TileSpawner tileSpawner;
    public ProfessionSystem professionSystem;

    [Header("Settings")]
    public float startDelay = 0.5f;

    public RunnerState State { get; private set; } = RunnerState.Idle;

    public event Action OnRunStarted;
    public event Action OnRunEnded;
    public event Action OnPlayerDied;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        Instance = this;

        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (player != null)
        {
            startPosition = player.transform.position;
            startRotation = player.transform.rotation;
        }
    }

    private void Start()
    {
        SetIdle();
    }

    public void KillPlayer()
    {
        if (State != RunnerState.Running)
            return;

        EndRun();
    }

    public void EnterChallenge()
    {
        State = RunnerState.InChallenge;

        if (player != null)
            player.EnableInput(false);
    }

    public void StartRun()
    {
        if (State != RunnerState.Idle) return;

        State = RunnerState.Running;

        professionSystem?.ResetProgress();

        if (player != null)
            player.enabled = true;

        OnRunStarted?.Invoke();
    }

    public void EndRun()
    {
        if (State != RunnerState.Running) return;

        State = RunnerState.Dead;

        if (player != null)
            player.enabled = false;

        PreferenceAnalyzer.Instance?.RegisterRunCompleted();

        OnRunEnded?.Invoke();
        OnPlayerDied?.Invoke();
    }

    public void RestartRun()
    {
        if (player != null)
        {
            player.transform.position = startPosition;
            player.transform.rotation = startRotation;
            player.ForceSetLane(player.startLane);
            player.EnableInput(false);
        }

        tileSpawner?.ResetSpawner();
        professionSystem?.ResetProgress();
        SlowMoController.Instance?.ForceEndSlowMo();

        State = RunnerState.Idle;
    }

    private void SetIdle()
    {
        State = RunnerState.Idle;

        if (player != null)
            player.enabled = false;

        SlowMoController.Instance?.ForceEndSlowMo();
    }
}
