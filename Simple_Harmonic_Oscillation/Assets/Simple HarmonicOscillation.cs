using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHarmonicOscillation : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;

    // Parameters for oscillation
    [SerializeField] float springConstant = 1f;  // Spring constant (k)
    [SerializeField] float timeStep = 0.02f;     // Time step for integration
    [SerializeField] int maxPoints = 100;
    public enum IntegrationMethod { Euler, Verlet, RK4 }
    [SerializeField] IntegrationMethod currentMethod = IntegrationMethod.Euler;

    // Internal state variables
    Vector2 position = new Vector2(1f, 0f);
    Vector2 previousPosition;
    Vector2 velocity = Vector2.zero;
    List<Vector2> positionsArray = new List<Vector2>();
    float timeElapsed = 0f;

    void Start()
    {
        lineRenderer.positionCount = maxPoints;

        previousPosition = position;

        SwitchMethod();
    }

    void Update()
    {
        // Switch between methods when space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchMethod();
            ResetLine();
        }

        // Update the position based on selected integration method
        for (int i = 0; i < maxPoints; i++)
        {
            Vector2 newPosition = UpdatePosition();
            positionsArray.Add(newPosition);
            timeElapsed += timeStep;
        }

        UpdateLineRenderer(positionsArray);
    }

    //---------------------------------- Update Position -------------------------------------
    Vector2 UpdatePosition()
    {
        Vector2 newPosition = position;
        switch (currentMethod)
        {
            case IntegrationMethod.Euler:
                newPosition = EulerMethod(timeStep, position, velocity, springConstant);
                break;
            case IntegrationMethod.Verlet:
                newPosition = VerletMethod(timeStep, position, previousPosition, springConstant);
                break;
            case IntegrationMethod.RK4:
                newPosition = RK4Method(timeStep, position, velocity, springConstant);
                break;
        }

        // Store the previous position for Verlet method
        previousPosition = position;
        position = newPosition;
        return newPosition;
    }

    //---------------------------------- Euler Method -------------------------------------
    Vector2 EulerMethod(float dt, Vector2 position, Vector2 velocity, float springConstant)
    {
        Vector2 acceleration = -springConstant * position;
        position += velocity * dt;
        velocity += acceleration * dt;
        return position;
    }

    //---------------------------------- Verlet Method -------------------------------------
    Vector2 VerletMethod(float dt, Vector2 position, Vector2 previousPosition, float springConstant)
    {
        Vector2 acceleration = -springConstant * position;
        Vector2 newPosition = 2 * position - previousPosition + acceleration * dt * dt;
        return newPosition;
    }

    //---------------------------------- Runge-Kutta 4 (RK4) Method -------------------------------------
    Vector2 RK4Method(float dt, Vector2 position, Vector2 velocity, float springConstant)
    {
        Vector2 k1 = velocity;
        Vector2 k1a = -springConstant * position;

        Vector2 k2 = velocity + k1a * (dt / 2);
        Vector2 k2a = -springConstant * (position + k1 * (dt / 2));

        Vector2 k3 = velocity + k2a * (dt / 2);
        Vector2 k3a = -springConstant * (position + k2 * (dt / 2));

        Vector2 k4 = velocity + k3a * dt;
        Vector2 k4a = -springConstant * (position + k3 * dt);

        velocity += (k1a + 2 * k2a + 2 * k3a + k4a) * (dt / 6);
        position += (k1 + 2 * k2 + 2 * k3 + k4) * (dt / 6);
        return position;
    }

    // Update the LineRenderer
    void UpdateLineRenderer(List<Vector2> positions)
    {
        lineRenderer.positionCount = positions.Count;
        for (int i = 0; i < positions.Count; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(positions[i].x, positions[i].y, 0));
        }
    }

    // Switch between the integration methods
    void SwitchMethod()
    {
        if (currentMethod == IntegrationMethod.Euler)
        {
            currentMethod = IntegrationMethod.Verlet;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
        }
        else if (currentMethod == IntegrationMethod.Verlet)
        {
            currentMethod = IntegrationMethod.RK4;
            lineRenderer.startColor = Color.blue;
            lineRenderer.endColor = Color.blue;
        }
        else
        {
            currentMethod = IntegrationMethod.Euler;
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;
        }

    }

    // Resets the LineRenderer
    void ResetLine()
    {
        positionsArray.Clear();
        lineRenderer.positionCount = 0;
        position = new Vector2(1f, 0f);
        previousPosition = position;
        velocity = Vector2.zero;
        timeElapsed = 0f;
    }
}
