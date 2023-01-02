using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class TectonicsToy : MonoBehaviour
{
    public GameObject Chunk;
    [FormerlySerializedAs("LeftMaterial")]
    public Material TopMaterial;
    [FormerlySerializedAs("RightMaterial")]
    public Material BottomMaterial;
    public int NumChunks;
    public float ChunkSize;
    public float PlateDistance;

    [Header("Params")]
    public float InflationRate = 0.5f;
    public float MinSubductionPressure = 0.1f;
    public bool EnableInflation = true;
    public bool EnableSubduction = true;
    private readonly List<GameObject> BottomPlate = new();

    private readonly List<GameObject> TopPlate = new();

    private void Start()
    {
        for (var i = NumChunks; i > 0; i--)
        {
            TopPlate.Add(CreateChunk(new Vector3(-i * ChunkSize, PlateDistance, 0), new Vector3(ChunkSize, ChunkSize / 2, ChunkSize), TopMaterial));
            BottomPlate.Add(CreateChunk(new Vector3(-i * ChunkSize, -PlateDistance - ChunkSize, 0), new Vector3(ChunkSize, 0, ChunkSize), BottomMaterial));
        }

        for (var i = 0; i <= NumChunks; i++)
        {
            TopPlate.Add(CreateChunk(new Vector3(i * ChunkSize, PlateDistance, 0), new Vector3(ChunkSize, 0, ChunkSize), TopMaterial));
            BottomPlate.Add(CreateChunk(new Vector3(i * ChunkSize, -PlateDistance - ChunkSize, 0), new Vector3(ChunkSize, ChunkSize / 2, ChunkSize), BottomMaterial));
        }

        var controls = new Controls();
        controls.Exhibit.Enable();

        controls.Exhibit.Forward.performed += _ => MoveRight();
        controls.Exhibit.Back.performed += _ => MoveLeft();
    }

    public void MoveRight()
    {
        var endChunk = BottomPlate.Last();
        BottomPlate.Remove(endChunk);
        BottomPlate.Insert(0, endChunk);
        endChunk.transform.localScale = new Vector3(ChunkSize, 0, ChunkSize);
        endChunk.transform.localPosition = new Vector3(-ChunkSize * NumChunks, -PlateDistance, ChunkSize);

        foreach (var chunk in BottomPlate) UpdatePosition(chunk);
        Simulate();
    }

    public void MoveLeft()
    {
        var endChunk = BottomPlate.First();
        BottomPlate.Remove(endChunk);
        BottomPlate.Add(endChunk);
        endChunk.transform.localScale = new Vector3(ChunkSize, 0, ChunkSize);
        endChunk.transform.localPosition = new Vector3(ChunkSize * NumChunks, -PlateDistance, ChunkSize);

        foreach (var chunk in BottomPlate) UpdatePosition(chunk);
        Simulate();
    }

    public void Simulate()
    {
        for (var i = NumChunks * 2; i > 0; i--)
        {
            var topChunk = TopPlate[i].transform;
            var bottomChunk = BottomPlate[i].transform;
            var top = topChunk.localScale.y;
            var bottom = bottomChunk.localScale.y;
            if (i < NumChunks)
            {
                if (EnableInflation)
                    StartCoroutine(AnimationUtils.AnimateVector3(0.5f,
                        topChunk.localScale,
                        new Vector3(ChunkSize, Inflation(top, bottom), ChunkSize),
                        x => topChunk.localScale = x,
                        null,
                        EaseType.InOut));
                if (EnableSubduction)
                    StartCoroutine(AnimationUtils.AnimateVector3(0.5f,
                        bottomChunk.localScale,
                        new Vector3(ChunkSize, Subduction(top, bottom), ChunkSize),
                        x => bottomChunk.localScale = x,
                        null,
                        EaseType.InOut));
            }
            else
            {
                if (EnableInflation)
                    StartCoroutine(AnimationUtils.AnimateVector3(0.5f,
                        bottomChunk.localScale,
                        new Vector3(ChunkSize, Inflation(bottom, top), ChunkSize),
                        x => bottomChunk.localScale = x,
                        null,
                        EaseType.InOut));
                if (EnableSubduction)
                    StartCoroutine(AnimationUtils.AnimateVector3(0.5f,
                        topChunk.localScale,
                        new Vector3(ChunkSize, Subduction(bottom, top), ChunkSize),
                        x => topChunk.localScale = x,
                        null,
                        EaseType.InOut));
            }
        }

        float Inflation(float top, float bottom)
        {
            var targetThickness = math.clamp(top + bottom, ChunkSize * 0.2f, ChunkSize);
            return math.lerp(top, targetThickness, math.lerp(InflationRate, 0, math.saturate(top / ChunkSize)));
        }

        float Subduction(float top, float bottom)
        {
            var pressure = math.clamp((top - bottom) / (top + bottom), MinSubductionPressure, 1);
            var subduction = top * pressure;
            return math.max(bottom - subduction, 0.1f);
        }
    }


    private void UpdatePosition(GameObject chunk)
    {
        var position = new Vector3(-NumChunks * ChunkSize + BottomPlate.IndexOf(chunk) * ChunkSize, -PlateDistance- ChunkSize, 0);
        StartCoroutine(AnimationUtils.AnimateVector3(0.5f, chunk.transform.localPosition, position, x => chunk.transform.localPosition = x, null, EaseType.InOut));
    }

    private GameObject CreateChunk(Vector3 position, Vector3 scale, Material material)
    {
        var chunk = Instantiate(Chunk);
        chunk.transform.parent = transform;
        chunk.GetComponentInChildren<MeshRenderer>().material = material;
        chunk.transform.localPosition = position;
        chunk.transform.localScale = scale;
        return chunk;
    }
}