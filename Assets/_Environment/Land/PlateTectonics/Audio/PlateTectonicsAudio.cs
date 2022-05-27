using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class PlateTectonicsAudio : MonoBehaviour
{
    [Range(0, 1)]
    public float AudioLerpSpeed = 1;
    [Range(0, 2)]
    public float RumbleThreshhold = 0.5f;
    public AudioSource RumbleSound;
    [Range(0, 2)]
    public float BoulderThreshhold = 1;
    public AudioSource BoulderSound;

    private PlateTectonicsData _data;
    public bool IsInitialized => _data != null;
    public bool IsActive { get; private set; }

    public void Initialize(PlateTectonicsData data)
    {
        _data = data;
    }
    public void Enable()
    {
        if (!IsInitialized) return;
        RumbleSound.Play();
        BoulderSound.Play();
        IsActive = true;
    }
    public void Disable()
    {
        RumbleSound.Stop();
        BoulderSound.Stop();
        IsActive = false;
    }

    public void Update()
    {
        if (!IsActive) return;
        var velocity = _data.Plates.Sum(x => Quaternion.Angle(x.Velocity, quaternion.identity));
        RumbleSound.volume = GetVolume(RumbleSound.volume, velocity, RumbleThreshhold);
        BoulderSound.volume = GetVolume(BoulderSound.volume, velocity, BoulderThreshhold);
    }
    private float GetVolume(float volume, float velocity, float threshold)
    {
        var target = math.saturate(velocity / threshold);
        return math.lerp(math.max(volume, target), target, Time.deltaTime * AudioLerpSpeed);
    }
}
