using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public class Boid : MonoBehaviour {

    public float viewRadius;
    public float viewAngle;
    public float speed;
    public float turnSpeed;

    private SpriteRenderer sprite;
    private BoidManager boidManager;

    public List<Transform> neighbors;

    private Vector3 desiredVelocity;
    private Vector3 desiredAngle;

    private void Awake() {
        boidManager = GetComponentInParent<BoidManager>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Start() {
        neighbors = new List<Transform>();

        Color[] colors = new Color[] {
            new Color32(255, 173, 173, 255),
            new Color32(255, 214, 165, 255),
            new Color32(253, 255, 182, 255),
            new Color32(202, 255, 191, 255),
            new Color32(155, 246, 255, 255),
            new Color32(160, 196, 255, 255),
            new Color32(189, 178, 255, 255),
            new Color32(255, 198, 255, 255)
        };
        sprite.color = colors[Random.Range(0, colors.Length)];
    }

    void Update() {

        if (boidManager.DebugLines) {
            foreach (Transform b in neighbors) {
                Popcron.Gizmos.Line(transform.position, b.position, Color.green);
            }

            Popcron.Gizmos.Line(transform.position, transform.position + transform.up, Color.blue);

            if (desiredVelocity != Vector3.zero) {
                Popcron.Gizmos.Line(transform.position, transform.position + desiredVelocity.normalized, Color.red);
            }
        }

        // Check bounds
        Vector3 boundVelocity = Vector3.zero;
        if (!boidManager.Bound.Contains(transform.position)) {
            // Above Bound
            if (transform.position.y > boidManager.Bound.max.y - 1) {
                boundVelocity += new Vector3(0, -(transform.position.y + boidManager.Bound.max.y));
            }

            //Below bound
            if (transform.position.y < boidManager.Bound.min.y + 1) {
                boundVelocity += new Vector3(0, -(transform.position.y + boidManager.Bound.min.y));
            }

            // Left
            if (transform.position.x < boidManager.Bound.min.x + 1) {
                boundVelocity += new Vector3(-(transform.position.x + boidManager.Bound.min.x), 0);
            }

            // Right
            if (transform.position.x > boidManager.Bound.max.x - 1) {
                boundVelocity += new Vector3(-(transform.position.x + boidManager.Bound.max.x), 0);
            }
        }

        Vector3 centerVelocity = Vector3.zero;
        Vector3 collideVelocity = Vector3.zero;
        Vector3 matchVelocity = Vector3.zero;

        Vector3 mouseVelocity =  boidManager.mousePosition - transform.position;
        if (mouseVelocity.sqrMagnitude > viewRadius * viewRadius)
            mouseVelocity = Vector3.zero;
        mouseVelocity.Normalize();

        if (neighbors.Count > 0) {
            foreach (Transform n in neighbors) {

                // Cohesion
                if (boidManager.Cohesion)
                    centerVelocity += n.position;

                // Seperation
                if (boidManager.Seperation)
                    if (Vector3.Distance(transform.position, n.position) < 1f)
                        collideVelocity -= n.position - transform.position;

                // Alignment
                if (boidManager.Alignment)
                    matchVelocity += n.up;
            }
            if (boidManager.Cohesion) {
                centerVelocity /= neighbors.Count;
                centerVelocity -= transform.position;
            }
            if (boidManager.Alignment)
                matchVelocity /= neighbors.Count;
        }

        desiredVelocity = boundVelocity;
        if (boidManager.Alignment)
            desiredVelocity += matchVelocity;
        if (boidManager.Seperation)
            desiredVelocity += collideVelocity;
        if (boidManager.Cohesion)
            desiredVelocity += centerVelocity;
        if (boidManager.MouseAttract)
            desiredVelocity += mouseVelocity;
        if (boidManager.MouseRepel)
            desiredVelocity -= mouseVelocity;

        if (desiredVelocity == Vector3.zero) desiredVelocity = transform.up;

        desiredAngle = new Vector3(0, 0, Vector3.SignedAngle(Vector3.up, desiredVelocity, Vector3.forward));

        // Rotate the boid
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(desiredAngle), turnSpeed * Time.deltaTime);

        // Move the boid
        transform.position += transform.up * speed * Time.deltaTime;
    }
}
