using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Particle
{
    public Vector3 pos;
    public Vector3 prevPos;

    public float m;    //mass
    public float g;    //gravity
    public float d;    //damping
    public Vector3 f;    //forces
    public float r;    //radius;

    public bool pinned = false;
    public Vector3 pinPos;
    public Particle(float M, float G, float D, float R, Vector3 position)
    {
        pos = position;
        prevPos = position;
        m = M;
        g = G;
        d = D;
        r = R;
    }

    public Vector3 gravForce()
    {
        //F=ma in downwards direction
        return (g * m) * Vector3.down;
        
    }

    public void applyForce(Vector3 F)
    {
        //Add force to the sum of forces
        f += F;
    }

    public void integratePosition(float deltaTime)
    {
        //Verlet Integration
        if (!pinned)
        {
            //work out velocity for damping via change in position 
            Vector3 v = (pos - prevPos) / deltaTime;

            //Newton 2
            Vector3 acc = f / m;

            //apply damping
            acc -= v * (d / m);

            //Verlet Equation
            Vector3 newPos = pos * 2 - prevPos + acc * deltaTime * deltaTime;

            //Set new prev pos
            prevPos = pos;

            //set pos
            pos = newPos;
        } else
        {
            pos = pinPos;
        }

        //reset forces
        f = Vector3.zero;
       
    }

    public void pin()
    {
        pinned = true;
        pinPos = pos;
    }

    public void unpin()
    {
        pinned = false;
    }

    public void checkCollision(Particle Q)
    {
        if (Q != this)
        {
            Vector3 offset = Q.pos - pos;
            if (offset.sqrMagnitude < r * r + Q.r * Q.r)
            {
                Vector3 offDir = offset.normalized;
                //find component of force in collison direction to find normal
                float product = Vector3.Dot(offDir, f);
                Vector3 normal = -offDir * (product / offDir.magnitude);
                applyForce(normal);
            }
        }
    }
}

class Spring
{ 
    public Particle origin;
    public Particle target;

    public float k;    //spring constant
    public float r;    //radius

    public Spring(float K, float R, Particle o)
    {
        k = K;
        r = R;
        origin = o;
        
    }

    public void attach(Particle t)
    {
        target = t;
    }

    public void applyHooke()
    {
        Vector3 offset = origin.pos - target.pos;
        //Find distance between particles
        float d = offset.magnitude;

        //Find extension
        float ext = d - r;
        //Hooke's Law and direction vector
        Vector3 f = ext * -k * offset.normalized;

        target.applyForce(-f); //pulling towards origin
        origin.applyForce(f); //pulling towards target
    }

    public void applyConstraints()
    {
        Vector3 offset = origin.pos - target.pos;
  
        float d = offset.magnitude; 

        //Find extension
        float ext = d - r;
        //Find percentage extension
        float perc = (ext / d);
        //Divide by 2 since distance is applied to both particles
        perc /= 2;
        //Adjust particles
        origin.pos -= offset * perc;
        target.pos += offset * perc;
    }
}
