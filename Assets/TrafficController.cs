using System.Collections;
using UnityEngine;

public class TrafficController : MonoBehaviour
{
    [SerializeField] GameObject[] carPrefabs;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] float chanceOfCarToSpawnPerTick;

    // Update is called once per frame
    void Update()
    {
        float rand = Random.Range(0.0f, 100f);
        if (rand < chanceOfCarToSpawnPerTick)
        {
            SpawnCar();
        }
    }

    void SpawnCar()
    {
        var carPrefabIndex = Random.Range(0, carPrefabs.Length);
        var spawnPointIndex = Random.Range(0, spawnPoints.Length);

        var car = Instantiate(carPrefabs[carPrefabIndex], spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
        var endPoint = car.transform.forward * 100f;
        
        StartCoroutine(CarMovement(car, endPoint));
    }

    IEnumerator CarMovement(GameObject car, Vector3 endpoint)
    {
        Vector3 initialPosition = car.transform.position;
        float initialLifetime = 2;
        float carLifetime = initialLifetime;
        while (carLifetime > 0.0f)
        {
            float progress = (initialLifetime - carLifetime) / initialLifetime;
            car.transform.position = Vector3.Lerp(initialPosition, endpoint, progress);
            carLifetime -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(car);
    }
}
