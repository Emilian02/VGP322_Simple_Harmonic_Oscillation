using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHO : MonoBehaviour
{
    public enum IntegrationMethod
    {
        Euler,
        Verlet,
        RK4
    }

    [SerializeField] float amplitude;        // A
    [SerializeField] float angularFrequency; // B
    [SerializeField] float phaseShift;      // D
    [SerializeField] float verticalShift;   // C
    [SerializeField] float horizontalShift;
    [SerializeField] float timeStep;
    [SerializeField] int numPoints;      // Number of points to plot
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] IntegrationMethod method;

    Vector2[] points;

    //Verlet Method
    Vector2 previousPosition;
    Vector2 currentPosition;
    //RK4 Method
    Vector2 velocity;

    void Start()
    {
        points = new Vector2[numPoints];

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = numPoints;
        }

        InitializePositions();
    }

    void Update()
    {
        if (points.Length != numPoints)
        {
            points = new Vector2[numPoints];
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = numPoints;
            }
        }

        UpdateMethod();

        if (lineRenderer != null)
        {
            UpdateLineRenderer();
        }
    }

    void UpdateMethod()
    {
        switch (method)
        {
            case IntegrationMethod.Euler:
                EulerMethod();
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
                break;

            case IntegrationMethod.Verlet:
                InitializePositions();
                VerletMethod();
                lineRenderer.startColor = Color.blue;
                lineRenderer.endColor = Color.blue;
                break;

            case IntegrationMethod.RK4:
                InitializePositions();
                RK4Method();
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
                break;
        }
    }

    void EulerMethod()
    {
        float time = 0f;

        for (int i = 0; i < numPoints; i++)
        {
            float y = ComputeSHOValue(time);
            points[i] = new Vector2(time + horizontalShift, y);
            time += timeStep;
        }
    }

    void VerletMethod()
    {
        float time = 0f;

        for (int i = 0; i < numPoints; i++)
        {
            // Compute acceleration for the current position (only applied to the y-axis)
            float accelerationY = ComputeAcceleration(currentPosition.y);

            // Verlet integration formula for updating the position
            Vector2 newPosition;
            newPosition.x = currentPosition.x + timeStep;
            newPosition.y = 2 * currentPosition.y - previousPosition.y + accelerationY * timeStep * timeStep;

            // Store the new position
            points[i] = new Vector2(newPosition.x + horizontalShift, newPosition.y);

            // Update positions for the next iteration
            previousPosition = currentPosition;
            currentPosition = newPosition;

            time += timeStep;
        }
    }
    void RK4Method()
    {
        float time = 0f;

        for (int i = 0; i < numPoints; i++)
        {
            // Compute the current acceleration based on the Y position
            float accelerationY = ComputeAcceleration(currentPosition.y);
            Vector2 acceleration = new Vector2(0f, accelerationY);

            // Compute k1
            Vector2 k1 = timeStep * velocity; // Initial step
            Vector2 k1Acc = timeStep * acceleration; // Acceleration step for k1

            // Compute k2
            Vector2 midPosition = currentPosition + 0.5f * k1; // Intermediate position
            Vector2 midVelocity = velocity + 0.5f * k1Acc; // Intermediate velocity
            float midAccelerationY = ComputeAcceleration(midPosition.y);
            Vector2 k2 = timeStep * midVelocity; // Second step
            Vector2 k2Acc = timeStep * new Vector2(0f, midAccelerationY); // Acceleration step for k2

            // Compute k3
            Vector2 nextPosition = currentPosition + 0.5f * k2; // Intermediate position using k2
            Vector2 nextVelocity = velocity + 0.5f * k2Acc; // Intermediate velocity using k2
            float nextAccelerationY = ComputeAcceleration(nextPosition.y);
            Vector2 k3 = timeStep * nextVelocity; // Third step
            Vector2 k3Acc = timeStep * new Vector2(0f, nextAccelerationY); // Acceleration step for k3

            // Compute k4
            Vector2 finalPosition = currentPosition + k3; // Final position
            Vector2 finalVelocity = velocity + k3Acc; // Final velocity
            float finalAccelerationY = ComputeAcceleration(finalPosition.y);
            Vector2 k4 = timeStep * finalVelocity; // Fourth step
            Vector2 k4Acc = timeStep * new Vector2(0f, finalAccelerationY); // Acceleration step for k4

            // Update position and velocity using RK4 formula
            Vector2 newPosition;
            newPosition.x = currentPosition.x + timeStep; // Linear update for X
            newPosition.y = currentPosition.y + (1.0f / 6.0f) * (k1.y + 2 * k2.y + 2 * k3.y + k4.y);

            // Update velocity using RK4
            velocity = velocity + (1.0f / 6.0f) * (k1Acc + 2 * k2Acc + 2 * k3Acc + k4Acc);

            // Store the new position
            points[i] = new Vector2(newPosition.x + horizontalShift, newPosition.y);

            // Update positions for the next iteration
            currentPosition = newPosition;

            time += timeStep; // Increment time
        }
    }

    void InitializePositions()
    {
        // Set the current position at t = 0
        currentPosition = new Vector2(0.0f + horizontalShift, ComputeSHOValue(0.0f));
        // Set the previous position at t = -timeStep (to calculate the initial velocity)
        previousPosition = new Vector2(-timeStep + horizontalShift, ComputeSHOValue(-timeStep));
        velocity = Vector2.zero;
    }

    float ComputeSHOValue(float time)
    {
        // formula y = A cos(B (x - D)) + C
        return amplitude * Mathf.Cos(angularFrequency * (time - phaseShift)) + verticalShift;
    }

    float ComputeAcceleration(float y)
    {
        // a(t) = -B^2 * y(t)
        return -Mathf.Pow(angularFrequency, 2) * y;
    }

    void UpdateLineRenderer()
    {
        for (int i = 0; i < points.Length; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(points[i].x, points[i].y, 0));
        }
    }
}
