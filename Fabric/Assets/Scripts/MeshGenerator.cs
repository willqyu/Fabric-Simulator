using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public Mesh mesh;

    [Range(5, 100)] public int width;
    [Range(5, 100)] public int height;

    public float r; //rest distance
    public float k; //spring constant
    public float m; //mass
    public float d; //damping
    public float s; //spring strength

    public bool gravity;
    public float g;

    public bool constraints;
    public int constraintAccuracy;

    Particle sphere;
    public bool useSphere = true;

    public bool cornerPins = false;
    public bool edgePins = false;

    public bool paused = false;
    public bool editorGraphics = false;

    Particle[] particles;
    Vector3[] vertices;
    int[] triangles;

    Vector3[] cubes;
    Vector3[] cubeForces;

    Spring[] structurals;
    Spring[] shears;
    Spring[] bends;

    public bool wind;
    public float windSpeed;
    public float oscillationSpeed;
    public float theta;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        safeHooke();
        toggleCornerPins();
        createMesh();
    }

    void Update()
    {
        if (!paused)
        {
            defineVertices();
            updateSprings();
            updateParticles();

            updateMesh();
        }


    }

    public void createMesh()
    {
        createParticles();
        defineTriangles();
        createSprings();
        if (cornerPins)
        {
            particles[0].pin();
            particles[height - 1].pin();
            particles[height * width - height].pin();
            particles[height * width - 1].pin();
        }

    }

    void createParticles()
    {

        particles = new Particle[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x * height + y;
                particles[index] = new Particle(m, g, d, m, new Vector3(x * r, 0, y * r));
                if (x == 0 && edgePins)
                {
                    particles[index].pin();
                }
            }
        }
        sphere = new Particle(0, 0, 0, width * r / 8, new Vector3(width * r / 2, -width * r / 4, height * r / 2));

    }

    void defineTriangles()
    {
            
        triangles = new int[6 * (width - 1) * (height - 1)]; //same num of triangles as num of shear springs
                                                                 // but x3 for 3 points per triangle
        int i = 0;
        for (int x = 0; x < width - 1; x ++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                int index = x * height + y;
                triangles[i] = index;
                triangles[i + 1] = index + 1;
                triangles[i + 2] = index + height;
                triangles[i + 3] = index + height + 1;
                triangles[i + 4] = index + height;
                triangles[i + 5] = index + 1;
                i += 6;
                
            }
        }
    }

    void defineVertices()
    {
        vertices = new Vector3[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x * height + y;
                vertices[index] = particles[index].pos;
            }
        }
    }

    void createSprings()
    {
        //initialise arrays w/ formulae for number of springs
        
        structurals = new Spring[2 * width * height - width - height];
        shears = new Spring[2*(width-1)*(height-1)];
        bends = new Spring[2 * (width * height - height - width)];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x * height + y;
                if (x < width - 1)
                {
                    int n = findNextIndex<Spring>(structurals, index);
                    structurals[n] = new Spring(k, r, particles[index]);
                    structurals[n].attach(particles[index + height]);
                    
                }
                if (y < height - 1)
                {
                    int n = findNextIndex<Spring>(structurals, index);
                    structurals[n] = new Spring(k, r, particles[index]);
                    structurals[n].attach(particles[index + 1]);
                }
                if (x < width - 1 && y < height-1)
                {
                    int n = findNextIndex<Spring>(shears, index);
                    shears[n] = new Spring(k, r * Mathf.Sqrt(2), particles[index]);
                    shears[n].attach(particles[index + height + 1]);
                }
                if (x < width - 1 && y > 0)
                {
                    int n = findNextIndex<Spring>(shears, index);
                    shears[n] = new Spring(k, r * Mathf.Sqrt(2), particles[index]);
                    shears[n].attach(particles[index + height - 1]);
                }
                if (x < width - 2)
                {
                    int n = findNextIndex<Spring>(bends, index);
                    bends[n] = new Spring(k, r * 2, particles[index]);
                    bends[n].attach(particles[index + height * 2]);
                }
                if (y < height - 2)
                {
                    int n = findNextIndex<Spring>(bends, index);
                    bends[n] = new Spring(k, r * 2, particles[index]);
                    bends[n].attach(particles[index + 2]);
                }
            }
        }
    }

    void createCubes()
    {
        cubes = new Vector3[(width - 1) * (height - 1) * (width - 1)]; 
        for (int x = 0; x < width; x ++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    cubes[x * height + y * width + z] = new Vector3(x * r, -y * r, z * r);
                    cubeForces[x * height + y * width + z] = new Vector3(Mathf.Sin(x), Mathf.Sin(y), Mathf.Sin(z));
                    cubeForces[x * height + y * width + z] = cubeForces[x * height + y * width + z].normalized;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (editorGraphics) {
            Gizmos.color = Color.red;
            foreach (Spring s in bends)
            {
                Gizmos.DrawLine(s.origin.pos, s.target.pos);
            }
            Gizmos.color = Color.green;
            foreach (Spring s in shears)
            {
                Gizmos.DrawLine(s.origin.pos, s.target.pos);
            }
        }
    }

    void updateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    int findNextIndex<T>(T[] array, int index)
    {
        int offset = 0;

        while (array[index + offset] != null)
        {
            offset++;
            if (index + offset > array.Length - 1)
            {
                return array.Length;
            }
        }
        return index + offset;
    }

    void updateSprings()
    {
        for (int i = 0; i < constraintAccuracy; i++)
        {
            if (constraints)
            {
                foreach (Spring s in structurals)
                {
                    s.applyConstraints();
                }
                foreach (Spring s in shears)
                {
                    s.applyConstraints();
                }
                foreach (Spring s in bends)
                {
                    s.applyConstraints();
                }
            }
            else
            {
                foreach (Spring s in structurals)
                {
                    s.applyHooke();
                }
                foreach (Spring s in shears)
                {
                    s.applyHooke();
                }
                foreach (Spring s in bends)
                {
                    s.applyHooke();
                }
            }
        }
    }

    void updateParticles()
    {
        foreach (Particle p in particles)
        {
            if (gravity)
            {
                p.applyForce(p.gravForce());
            }
            if (wind)
            {
                Vector3 wind = new Vector3(
                    Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * (p.pos.x + theta))),
                    0,
                    0.5f * Mathf.Sin(Mathf.Deg2Rad * theta)
                );
                p.applyForce(wind * windSpeed);

                theta += oscillationSpeed;
                Debug.DrawLine(p.pos, p.pos + wind * windSpeed, Color.red);
                if (theta > 360) theta -= 360;
                

            }
            if (useSphere)
            {
                p.checkCollision(sphere);
            }
            
            p.integratePosition(0.01f);
        }
    }

    //UI functions
    public void safeHooke()
    {
        r = 5;
        k = 100;
        m = 1;
        d = 1;
        g = 9.8f;
        constraints = false;
        constraintAccuracy = 1;
        createMesh();
    }

    public void safeConstraints()
    {
        r = 5;
        k = 1;
        m = 1;
        d = 20;
        g = 9.8f;
        constraints = true;
        constraintAccuracy = 3;
        createMesh();
    }

    public void toggleSphere()
    {
        useSphere = !useSphere;
    }

    public void toggleWind()
    {
        wind = !wind;
    }

    public void toggleCornerPins()
    {
        cornerPins = !cornerPins;
    }
    public void toggleEdgePins()
    {
        edgePins = !edgePins;
    }

    public void togglePause()
    {
        paused = !paused;
    }
}
