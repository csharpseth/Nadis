using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UIElements;
using System.Security.Cryptography;

public class AsteroidController : MonoBehaviour
{
    public Vector2 region;
    public float pointSeparation = 3f;
    public float pointDensity = 0.1f;
    public int maxLocations = 100;
    public LayerMask groundMask;

    public float minSpeed = 20f, maxSpeed = 30f;
    public float minDistance = 300f, maxDistance = 500f;
    public GameObject asteroidPrefab;
    public float delay = 1f;

    Vector3[] possibleLocations;

    public Queue<Asteroid> asteroids;

    private void Awake()
    {
        asteroids = new Queue<Asteroid>();
        possibleLocations = Util.GetNormalizedGridPoints(region.x, region.y, pointSeparation, pointDensity, maxLocations, groundMask);

    }

    float time = 5f;
    private void Update()
    {
        time += Time.deltaTime;
        if(time >= delay)
        {
            CreateAsteroid();
            time = 0f;
        }


        if(asteroids.Count > 0)
        {
            for (int i = 0; i < asteroids.Count; i++)
            {
                Asteroid asteroid = asteroids.Dequeue();
                asteroid.Do(Time.deltaTime);
                if(asteroid.hit)
                {

                }else
                {
                    asteroids.Enqueue(asteroid);
                }
            }
        }
    }

    private Vector3 GetPointNormal(Vector3 point)
    {
        Vector3 normal = Vector3.up;
        RaycastHit[] hits = new RaycastHit[1];
        if(Physics.RaycastNonAlloc(point + normal, Vector3.down, hits) > 0)
        {
            normal = hits[0].normal;
        }

        return normal;
    }
    private Vector3 GetRandomDestination()
    {
        return possibleLocations[UnityEngine.Random.Range(0, possibleLocations.Length)];
    }

    private void CreateAsteroid()
    {
        Vector3 destination = GetRandomDestination();
        Vector3 origin = destination + (GetPointNormal(destination) * Util.RollValue(minDistance, maxDistance));

        Transform body = Instantiate(asteroidPrefab, origin, Quaternion.identity).transform;
        Asteroid asteroid = new Asteroid(body, destination, Util.RollValue(minSpeed, maxSpeed));
        asteroids.Enqueue(asteroid);
    }
}

public struct Asteroid
{
    private Vector3 destination;
    private float speed;
    public Transform body;
    private Vector3 dir;
    public bool hit;

    public Asteroid(Transform body, Vector3 destination, float speed)
    {
        this.body = body;
        this.destination = destination;
        this.speed = speed;
        dir = (destination - body.position).normalized;
        hit = false;
    }

    public void Do(float deltaTime)
    {
        Vector3 pos = body.position;
        pos += dir * speed * deltaTime;
        body.position = pos;
        body.LookAt(destination);

        float distance = (destination - body.position).sqrMagnitude;
        if(distance <= (0.25f))
        {
            hit = true;
        }

    }
}
