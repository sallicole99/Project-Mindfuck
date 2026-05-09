using UnityEngine;
using System.Collections.Generic;

public class GaugeManager : MonoBehaviour
{
    private void Awake() => Instance = this;
    
    public Gauge CreateGaugeInstance(Sprite icon, float totalTime)
    {
        if (gauges.Count > maxGaugesAmount)
        {
            return null;
        }
        Gauge gauge = Instantiate(gaugePrefab, transform);
        gauge.Show(icon, totalTime);
        gauges.Add(gauge);
        return gauge;
    }

    public static GaugeManager Instance;
    [SerializeField] private Gauge gaugePrefab;
    [SerializeField] private int maxGaugesAmount = 5;
    [HideInInspector] public List<Gauge> gauges = new List<Gauge>();
}