using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class BoidManager : MonoBehaviour {

    public Text populationText;
    public Toggle mouseAttractToggle;
    public Toggle mouseRepelToggle;

    public GameObject BoidPrefab;

    public int population;

    public int populationPool = 100;

    public bool MouseAttract = false;
    public bool MouseRepel = false;
    public bool Alignment = true;
    public bool Cohesion = true;
    public bool Seperation = true;
    public bool DebugLines = false;

    public Bounds Bound;

    public List<Transform> Boids;

    public Vector3 mousePosition;

    private int oldPopulation;

    private void Start() {
        populationText.text = "Population: " + population;

        for (int i = 0; i < populationPool; i++) {
            Vector3 position = new Vector3(Random.Range(Bound.min.x, Bound.max.x), Random.Range(Bound.min.y, Bound.max.y));
            GameObject b = Instantiate(BoidPrefab, position, Quaternion.identity, transform);
            if (i >= population)
                b.SetActive(false);
            Boids.Add(b.transform);
        }
    }

    private void Update() {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        if (DebugLines) {
            Popcron.Gizmos.Cube(Bound.center, Quaternion.identity, Bound.size, Color.green);
        }

        foreach (Transform t in Boids) {
            Boid b = t.GetComponent<Boid>();

            // Get neighbors
            foreach (Transform n in Boids) {
                // Skip current boid
                if (n == t) continue;

                // Remove and skip boids outside of view
                Vector3 heading = n.position - t.position;
                float angle = Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg - 90;
                if (heading.sqrMagnitude > b.viewRadius * b.viewRadius || angle > b.viewAngle || angle < -b.viewAngle || !n.gameObject.activeSelf || !n.gameObject.activeInHierarchy) {
                    b.neighbors.Remove(n);
                    continue;
                }

                // Add if not already there
                if (!b.neighbors.Contains(n))
                    b.neighbors.Add(n);
            }
        }

        if (population != oldPopulation) {
            if (population > populationPool) {
                AddToPool();
            }

            for (int i = 0; i < populationPool; i++) {
                if (i >= population) Boids[i].gameObject.SetActive(false);
                else Boids[i].gameObject.SetActive(true);
            }
        }

        oldPopulation = population;
    }

    public void UpdatePopulation(float newPop) {
        population = (int)newPop;
        populationText.text = "Population: " + population;
    }

    public void UpdateSeperation(bool value) {
        Seperation = value;
    }

    public void UpdateCohesion(bool value) {
        Cohesion = value;
    }

    public void UpdateAlignment(bool value) {
        Alignment = value;
    }

    public void UpdateDebug(bool value) {
        DebugLines = value;
    }

    public void UpdateMouseAttract (bool value) {
        MouseAttract = value;
        if (value) {
            mouseRepelToggle.isOn = false;
            MouseRepel = false;
        }
    } 

    public void UpdateMouseRepel(bool value) {
        MouseRepel = value;
        if (value) {
            mouseAttractToggle.isOn = false;
            MouseAttract = false;
        }
    }


    private void AddToPool() {
        for (int i = 0; i < 50; i++) {
            Vector3 position = new Vector3(Random.Range(Bound.min.x, Bound.max.x), Random.Range(Bound.min.y, Bound.max.y));
            GameObject b = Instantiate(BoidPrefab, position, Quaternion.identity, transform);
            if (i >= population)
                b.SetActive(false);
            Boids.Add(b.transform);
        }

        populationPool += 50;
    }

    private void OnDrawGizmos() {
    }
}