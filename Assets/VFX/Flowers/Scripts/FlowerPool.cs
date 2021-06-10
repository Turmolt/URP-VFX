using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPool : MonoBehaviour
{
    public static FlowerPool Instance;

    public FlowerManager[] Flowers;

    private Queue<FlowerManager> flowerPool;

    private int poolIncrements = 800;

    void Start()
    {
        Instance = this;
        FillPool();
    }

    void FillPool()
    {
        flowerPool = new Queue<FlowerManager>();
        for (int i = 0; i < poolIncrements; i++)
        {
            var flower = Instantiate(Flowers[Random.Range(0, Flowers.Length)]) as FlowerManager;
            flower.transform.localScale = (0.015f + Random.Range(-0.0025f, 0.0025f))* Vector3.one;
            flower.Initialize();
            flower.gameObject.SetActive(false);
            flowerPool.Enqueue(flower);
        }
    }

    public FlowerManager PopFlower()
    {
        return flowerPool.Dequeue();
    }

    public void PushFlower(FlowerManager flower)
    {
        flower.gameObject.SetActive(false);
        flower.transform.parent = null;
        flower.ResetFlower();
        flowerPool.Enqueue(flower);
    }

}
