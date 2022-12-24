using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlateTectonicsAudio : MonoBehaviour
{
    [Range(0, 1)]
    public float AudioLerpSpeed = 1;
    public float MoveThreshhold = 2f;
    public float PitchVariation = 0.5f;
    public AudioSource MovePlateSound;
    public AudioSource BreakPlateSound;
    public AudioSource MergePlateSound;

    private PlateTectonicsData _data;

    public bool IsActive { get; private set; }

    private void Start() => Planet.Data.Subscribe(x => _data = x.PlateTectonics);

    private void Update()
    {
        if (!IsActive) return;
        var velocity = _data.Plates.Sum(x => Quaternion.Angle(x.Velocity, quaternion.identity));
        MovePlateSound.volume = GetVolume(MovePlateSound.volume, velocity, MoveThreshhold);
    }

    public void Enable()
    {
        MovePlateSound.Play();
        IsActive = true;
    }

    public void Disable()
    {
        MovePlateSound.Stop();
        IsActive = false;
    }

    public void BreakPlate()
    {
        BreakPlateSound.pitch = Random.Range(1 - PitchVariation, 1 + PitchVariation);
        BreakPlateSound.Play();
    }

    public void MergePlate()
    {
        MergePlateSound.pitch = Random.Range(1 - PitchVariation, 1 + PitchVariation);
        MergePlateSound.Play();
    }

    private float GetVolume(float volume, float velocity, float threshold)
    {
        var target = math.saturate(velocity / threshold);
        return math.lerp(math.max(volume, target), target, Time.deltaTime * AudioLerpSpeed);
    }
}