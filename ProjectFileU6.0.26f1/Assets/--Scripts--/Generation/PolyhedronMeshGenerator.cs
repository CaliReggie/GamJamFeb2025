#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PolyhedronMeshGenerator))]
public class TileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        
        PolyhedronMeshGenerator tileGen = (PolyhedronMeshGenerator) target;
        
        GUILayout.Label("Gen Flow In Descending Order - Reference State Above");

        GUILayout.Label("Vert Generation");
        
        if (GUILayout.Button("Generate Verts"))
        {
            tileGen.GenerateFromValues();
        }
        
        GUILayout.Label("Vert Modification");
        
        if (GUILayout.Button("Modify Noise"))
        {
            tileGen.ModifyNoise();
        }

        GUILayout.Label("Vert Cleaning");
        
        if (GUILayout.Button("Auto Determine/Clear Invalid Verts"))
        {
            tileGen.AutoDetermineAndClearVerts();
        }
        
        GUILayout.Label("Triangle Generation");

        if (GUILayout.Button("Generate Triangles"))
        {
            tileGen.GenerateTriangles();
        }
        
        GUILayout.Label("Triangle Application");
        
        if (GUILayout.Button("Apply Triangles"))
        {
            tileGen.ApplyToMesh();
        }
        
        GUILayout.Label("Cleaning");
        
        if (GUILayout.Button("Shed And Reset"))
        {
            tileGen.Clear(false);
        }
        
        if (GUILayout.Button("Destroy And Reset"))
        {
            tileGen.Clear(true);
        }
        
        GUILayout.Label("Manual Vert Cleaning (Deprecated)");
        
        if (GUILayout.Button("Determine Invalid Verts"))
        {
            tileGen.DetermineInvalidVerts();
        }
        
        if (GUILayout.Button("Clear Invalid Verts"))
        {
            tileGen.CleanInvalidVerts();
        }
    }
}

//enum for managing face direction between polyhedrons and their faces
internal enum EGlobalDirection
{
    Unset,
    Up,
    Down,
    Right,
    Left,
    Forward,
    Back
}

//used to represent a point along with various states and information of the point
internal class Vertex
{
    //whether vert is active and should be considered existing in the final mesh
    public bool Active { get; set; }
    
    public Vector3 Position { get; set; }
    
    //bitmask for neighbors
    public int NeighborMask { get; set; }
    
    //whether vert should be considered valid in forming polyhedron faces
    public bool Valid { get; set; }
    
    //Used to ensure exterior verts stay valid through multiple cleaning passes - assigned and used once
    public bool WasCleaned { get; set; }

    /// <summary>
    /// Creates a vertex with given position. Includes get / set information about: self pos, active state, validity,
    /// and "neighbor" bitmask (the 6 global directions). (Was cleaned is extra cheat)
    /// Bitmask is represented as 6 bits. In a bit represented as 0b101011, starting at the right most digit
    /// and moving left, the bits represent: Back (1), Front (1), Down (0), Up (1), Right (0), Left (1)
    /// Control is completely up to external holder
    /// </summary>
    public Vertex(Vector3 position, bool active = true, int neighborMask = 0)
    {
        Position = position;
        
        Active = active;
        
        NeighborMask = neighborMask;
        
        Valid = false;
        
        WasCleaned = false;
    }
}

//represents a single face of a set of verts (currently logic works for 4) that should be triangulated
internal class Face
{
    public List<Vertex> Vertices { get; set; }
    
    //the direction the face is facing (normal)
    public EGlobalDirection FaceDirection { get; set; }
    
    //condition to deny validity - assigned and used once
    public bool MarkedToSkip { get; set; }

    /// <summary>
    /// Create a face with the given vertices and face direction. No checks for validity / order of verts. Please ensure
    /// that 4 verts of the same axis are passed in, in clockwise order from the perspective of the face direction
    /// Triangulation will be done per face. It just matters that the order is correct and consistent.
    /// Using global Up, Down, Left, Right, Forward, Back relative to face normal, here are the "clockwise" orders:
    /// Up: BackLeft, ForwardLeft, ForwardRight, BackRight
    /// Down: ForwardLeft, BackLeft, BackRight, ForwardRight
    /// Right: UpBack, DownBack, DownForward, UpForward
    /// Left: DownBack, UpBack, UpForward, DownForward
    /// Forward: DownLeft, UpLeft, UpRight, DownRight
    /// Back: UpLeft, DownLeft, DownRight, UpRight
    /// These are how you should pass in the verts in the array, along with corresponding face direction
    /// Control is completely up to external holder
    /// </summary>
    public Face(List<Vertex> vertices = null, EGlobalDirection faceDirection = EGlobalDirection.Unset)
    {
        Vertices = vertices;
        
        FaceDirection = faceDirection;
        
        MarkedToSkip = false;
    }
    
    //shared if all verts are the same
    public bool SharesAllVerts(Face other)
    {
        if (Vertices == null || other.Vertices == null) return false;
        
        return Vertices.All(vert => other.Vertices.Contains(vert));
    }
    
    
    /// <summary>
    /// If face can be drawn, returns 6 verts in draw order (0,1,2) + (0,2,3)
    /// can be null if not valid
    /// </summary>
    /// <returns></returns>
    public List<Vertex> TryGetTriVerts()
    {
        if (!Valid) return null;
        
        List<Vertex> triVerts = new List<Vertex>();
        
        //add the first triangle
        triVerts.Add(Vertices[0]);
        
        triVerts.Add(Vertices[1]);
        
        triVerts.Add(Vertices[2]);
        
        //add the second triangle
        triVerts.Add(Vertices[0]);
        
        triVerts.Add(Vertices[2]);
        
        triVerts.Add(Vertices[3]);
        
        return triVerts;
    }

    //valid if have 4 verts and all are active and valid
    public bool Valid
    {
        get
        {
            //not valid if null or unset face direction
            if (Vertices == null || FaceDirection == EGlobalDirection.Unset) return false;
            
            //not valid if marked to skip
            if (MarkedToSkip) return false;
            
            return (Vertices.Count == 4 && Vertices.All(vert => vert.Active && vert.Valid));
        }
    }
}

//used as a container for a set of faces (currently logic works for 6) that should be rendered as a chunk of the mesh
//think cubemap faces
internal class Polyhedron
{
    public List<Face> Faces { get; set; }
    
    /// <summary>
    /// Create a polyhedron that can have a number of faces. No checks for validity / order of faces. Please ensure
    /// verts are set in triangulation order: (0,1,2) + (0,2,3)
    /// </summary>
    public Polyhedron(List<Face> faces = null)
    {
        Faces = faces;
    }
    
    public bool HasValidFaceInDir(EGlobalDirection direction)
    {
        if (Faces == null) return false;
        
        foreach (var face in Faces)
        {
            if (face.FaceDirection == direction && face.Valid) return true;
        }
        
        return false;
    }
    
    public Face GetFaceInDir(EGlobalDirection direction)
    {
        if (Faces == null) return null;
        
        foreach (var face in Faces)
        {
            if (face.FaceDirection == direction) return face;
        }
        
        return null;
    }
}

public class PolyhedronMeshGenerator : MonoBehaviour
{
    private enum EGenerationState
    {
        ReadyToGen,
        VertGeneration,
        VertModification,
        VertCleaning,
        ReadyToGenTriangles,
        TrianglesGenerated,
        Cleaning
    }
    
    [Header("Generation State - Do Not Modify")]
    
    [SerializeField] private EGenerationState generationState = EGenerationState.ReadyToGen;
    
    [Header("References - Set In Inspector")]
    
    [Tooltip("The prefab to use that will hold the generated mesh. Make sure it has a MeshFilter and MeshRenderer")]
    [SerializeField]
    private GameObject emptyMeshPrefab;
    
    [Header("Size")]
    
    [UnityEngine.Range(1,100)] [SerializeField] private int xSize = 10;
    
    [SerializeField] private float xSpacing = 1;
    
    [UnityEngine.Range(1,100)] [SerializeField] private int zSize = 10;
    
    [SerializeField] private float zSpacing = 1;
    
    [UnityEngine.Range(1,25)] [SerializeField]private int ySize = 1;
    
    [SerializeField] private float ySpacing = 1;
    
    [Header("Noise Pattern")]
    
    [UnityEngine.Range(0.1f, 100f)] [SerializeField] private float noiseScale = 1f;
    
    [UnityEngine.Range(0,1)] [SerializeField] private float noiseThreshold = 0.5f;

    [UnityEngine.Range(0,0.25f)] [SerializeField] private float thresholdGainOverY;
    
    [SerializeField] private bool invertNoise;
    
    [Header("Noise Offset")]
    
    [SerializeField] private Vector3 noiseOffset;
    
    [Header("Generation Behaviour")]
    
    [SerializeField]
    private bool createAtTransform = true;
    
    [Header("Modification Behaviour")]

    [Tooltip("If true, will deactivate verts that don't meet the threshold on noise modification")]
    [SerializeField]
    private bool clipOnModify = true;
    
    [Header("Gizmo Settings")]
    
    [SerializeField] private bool drawGizmos = true;
    
    [SerializeField] private Color inactiveColor = new Vector4(0,0,0,25);
    
    [SerializeField] private Color activeValidColor = Color.green;
    
    [SerializeField] private Color activeInvalidColor = Color.red;

    [SerializeField] private Color triStateColor = Color.magenta;
    
    //Dynamic
    
    private GameObject _currentMeshGO;
    
    private Mesh _currentMesh;
    
    private Vector3Int _currentSize;
    
    private Vertex[] _allVerts;
    
    //personal note. In 3d, it lays out a row in the x direction, steps up the y and does the same till height is
    //reached, then steps down the z repeating the process
    
    private List<int> _triangles;
    
    public void GenerateFromValues()
    {
        //only allow generation if we're ready to gen
        if (generationState != EGenerationState.ReadyToGen) return;
        
         _currentMeshGO = Instantiate(emptyMeshPrefab, transform);

         _currentMesh = new Mesh();
         
        _allVerts = new Vertex[(xSize + 1) * (ySize + 1) * (zSize + 1)];
        
        _currentSize = new Vector3Int(xSize, ySize, zSize);
        
        _triangles = new List<int>();
      
        for (int i = 0, z = 0; z <= _currentSize.z; z++)
        {
            for (int y = 0; y <= _currentSize.y; y++)
            {
                for (int x = 0; x <= _currentSize.x; x++)
                {
                    Vector3 targetPosition = new Vector3(x * xSpacing, y * ySpacing, z * zSpacing);
                    
                    if (createAtTransform) targetPosition += transform.position;
                    
                    //guarantee bottom level (first two y layers) are filled
                    if (y <= 1)
                    {
                        PlacePoint(i, targetPosition, true);
                        
                        i++;
                        
                        continue;
                    }
                    
                    float posNoiseValue = GetNoiseValue(targetPosition + noiseOffset, noiseScale);

                    if (invertNoise) posNoiseValue = 1 - posNoiseValue;

                    float scaledThreshold = noiseThreshold + (thresholdGainOverY * y);
                    
                    bool setActive = posNoiseValue >= scaledThreshold;
                    
                    PlacePoint(i, targetPosition, setActive);
                    
                    i++;
                }
            }
        }
        
        _currentMesh.Clear();
        
        _currentMesh.vertices = Array.ConvertAll(_allVerts, x => x.Position);
        
        _currentMeshGO.GetComponent<MeshFilter>().mesh = _currentMesh;
        
        //mark we're done generating verts
        generationState = EGenerationState.VertGeneration;
        
        return;
        
        void PlacePoint(int index, Vector3 pos, bool active)
        {
            _allVerts[index] = new Vertex(pos, active);
        }
    }

    //The idea here is to go through the mesh like before
    //Not changing positions, but comparing each to current noise settings
    //If the noise value is met, we activate it and reset the state of the vert info
    //If not, we deactivate it
    public void ModifyNoise()
    {
        //only mod from gen or mod state
        if (generationState != EGenerationState.VertGeneration &&
            generationState != EGenerationState.VertModification) return;
        
        for (int i = 0, z = 0; z <= _currentSize.z; z++)
        {
            for (int y = 0; y <= _currentSize.y; y++)
            {
                for (int x = 0; x <= _currentSize.x; x++)
                {
                    //skip the first two layers (bottom level)
                    if (y <= 1)
                    {
                        i++;
                        
                        continue;
                    }
                    
                    Vector3 targetPosition = _allVerts[i].Position;
                    
                    float posNoiseValue = GetNoiseValue(targetPosition + noiseOffset, noiseScale);
                    
                    if (invertNoise) posNoiseValue = 1 - posNoiseValue;
                    
                    float scaledThreshold = noiseThreshold + (thresholdGainOverY * y);
                    
                    bool setActive = posNoiseValue >= scaledThreshold;
                    
                    //skip turning off if desired
                    if (!setActive && !clipOnModify)
                    {
                        i++;
                        
                        continue;
                    }
                    
                    _allVerts[i].Active = setActive;
                    
                    _allVerts[i].WasCleaned = false;
                        
                    _allVerts[i].Valid = false;
                    
                    i++;
                }
            }
        }
        
        //mark we're done modifying verts
        generationState = EGenerationState.VertModification;
    }
    
    //Applied to a vertex pos, this will give us the potential surrounding vertex positions one unit above and around
    //Not including directly above
    private static Vector3Int[] AboveLocRing = 
    {
        new Vector3Int(-1, 1, 1),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(-1, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(-1, 1, -1),
        new Vector3Int(0, 1, -1),
        new Vector3Int(1, 1, -1)
    };
    
    public void AutoDetermineAndClearVerts()
    {
        //only calc from vert gen, vert mod, or vert cleaning state
        if (generationState != EGenerationState.VertGeneration &&
            generationState != EGenerationState.VertModification &&
            generationState != EGenerationState.VertCleaning) return;
        
        //keep finding and clearing invalid verts until there are none new invalid ones left
        while (true)
        {
            DetermineInvalidVerts();
            
            int numInvalid = GetActiveInvalidVerts(_allVerts).Length;
            
            if (numInvalid == 0)
            {
                generationState = EGenerationState.ReadyToGenTriangles;
                
                break;
            }
            
            CleanInvalidVerts();
        }
    }
    
    //the idea here is to disable all invalid verts
    //valid criteria still work in progress
    
    public void DetermineInvalidVerts()
    {
        //only calc from vert gen, vert mod, or vert cleaning state
        if (generationState != EGenerationState.VertGeneration &&
            generationState != EGenerationState.VertModification &&
            generationState != EGenerationState.VertCleaning) return;
        
        for (int i = 0, z = 0; z <= _currentSize.z; z++)
        {
            for (int y = 0; y <= _currentSize.y; y++)
            {
                for (int x = 0; x <= _currentSize.x; x++)
                { 
                    
                    bool onBoundsEdge = x == 0 || x == _currentSize.x ||
                                  y == 0 || y == _currentSize.y ||
                                  z == 0 || z == _currentSize.z;
                    
                    int neighborMask = 0;
                    
                    if (HasNeighbor(x - 1, y, z)) neighborMask |= 0b000001; // Left
                    if (HasNeighbor(x + 1, y, z)) neighborMask |= 0b000010; // Right
                    if (HasNeighbor(x, y + 1, z)) neighborMask |= 0b000100; // Up
                    if (HasNeighbor(x, y - 1, z)) neighborMask |= 0b001000; // Down
                    if (HasNeighbor(x, y, z + 1)) neighborMask |= 0b010000; // Front
                    if (HasNeighbor(x, y, z - 1)) neighborMask |= 0b100000; // Back

                    // set the side mask for the current vertex
                    _allVerts[i].NeighborMask = neighborMask;
                    
                    // set validity of the vertex
                    _allVerts[i].Valid = IsValidVert(x, y, z, onBoundsEdge);
                    
                    i++;
                }
            }
        }
        
        //mark entering vert cleaning state
        generationState = EGenerationState.VertCleaning;
        
        return;
        
        //Nested Local Functions
        
        //index from nested loops in z,y,x order
        int GetIndex(int x, int y, int z)
        {
            return x + (y * (_currentSize.x + 1)) + (z * (_currentSize.x + 1) * (_currentSize.y + 1));
        }
        
        bool WithinBounds(int x, int y, int z)
        {
            return x >= 0 && x <= _currentSize.x && y >= 0 && y <= _currentSize.y && z >= 0 && z <= _currentSize.z;
        }
        
        bool HasNeighbor(int x, int y, int z)
        {
            // ensure the neighbor is within bounds
            if (!WithinBounds(x, y, z)) return false;
            
            // check if neighbor is active
            return _allVerts[GetIndex(x, y, z)].Active;
        }
        
        bool HadNeighbor(int x, int y, int z)
        {
            // ensure the neighbor is within bounds
            if (!WithinBounds(x, y, z)) return false;
            
            // check if neighbor is active
            return _allVerts[GetIndex(x, y, z)].WasCleaned;
        }
        
        //different logic if on bounds edge
        bool IsValidVert(int x, int y, int z, bool isOnBoundsEdge)
        {
            Vertex vert = _allVerts[GetIndex(x, y, z)];
            
            int neighborMask = vert.NeighborMask;
            
            int numNeighbors = CountBitsOn(neighborMask);
            
            if (isOnBoundsEdge)
            {
                //first there must be at least 3 neighbors
                if (numNeighbors < 3) return false;
                
                //since it's an edge, not valid if there isn't an active vert in the horizontal center direction
                
                Vector3Int centerDirNeighbor = new Vector3Int();
                    
                if (x == 0)
                {
                    centerDirNeighbor.x = 1;
                }
                else if (x == _currentSize.x)
                {
                    centerDirNeighbor.x = -1;
                }
                    
                if (z == 0)
                {
                    centerDirNeighbor.z = 1;
                }
                else if (z == _currentSize.z)
                {
                    centerDirNeighbor.z = -1;
                }
                
                //quickly check and catch inactive, but previously valid verts
                if (HadNeighbor(x + centerDirNeighbor.x, y, z + centerDirNeighbor.z)) return true;
                    
                //valid if there is a vert in the center direction
                return HasNeighbor(x + centerDirNeighbor.x, y + centerDirNeighbor.y, z + centerDirNeighbor.z);
            }
            else
            {
                //first there must be at least 3 neighbors
                if (numNeighbors < 3) return false;
                
                
                if (numNeighbors == 3)
                {
                    Vector3Int checkLoc = new Vector3Int(x, y, z);
                    
                    //checking for the two neighbors in specific formations
                    switch (neighborMask)
                    {
                        case 0b011010: // Front, Right, Down
                            checkLoc += new Vector3Int(1, 0, 1);
                            break;
                        case 0b101010: // Back, Right, Down
                            checkLoc += new Vector3Int(1, 0, -1);
                            break;
                        case 0b101001: // Back, Left, Down
                            checkLoc += new Vector3Int(-1, 0, -1);
                            break;
                        case 0b011001: // Front, Left, Down
                            checkLoc += new Vector3Int(-1, 0, 1);
                            break;
                        //if neighbors on opposite sides, false right away
                        case 0b111000: // Front, Back, Down
                        case 0b001011: // Left, Right, Down

                            return false;
                    }
                    
                    //if neighbor doesn't exist there, we are not valid
                    if (!HasNeighbor(checkLoc.x, checkLoc.y, checkLoc.z)) return false;
                }
                
                //if connected to 6, a few specific cases are involved
                if (numNeighbors == 6)
                {
                    //as a non edge, if all neighbors are connected, the one case in which we should stay valid is if
                    //we are in inward corner. In this instance, it is possible to have 6 neighbors
                    //(seeming like it's inside the mesh), but it holds a lot of real verts together
                    
                    //we have to check if we are an inward corner by checking in a loop above and below us,
                    //if we detect a single empty space in the loop one level above and below, we are an inward corner
                    //and should be marked as valid
                    
                    //otherwise, it's inside the mesh and needs to go
                    
                    //just checks above ring for now (generation doesn't currently make upper corners)
                    if (IsNonEdgeLowerCorner(x ,y ,z)) return true;
                    
                    //if not, we're inside and not exposed or relied on
                    return false;
                }
                
                //IMPLEMENT:
                //Other cases?
                
                //Otherwise, if we have 3-5  and aren't an edge, we are a valid vert
                return true;
            }
            
            //Twice Nested Local functions
            
            int CountBitsOn(int value)
            {
                int count = 0;
                while (value != 0)
                {
                    count += value & 1;
                    value >>= 1;
                }
                return count;
            }
            
            bool IsNonEdgeLowerCorner(int x, int y, int z)
            {
                //determining if lower corner by checking VertSurroundingLocs above us
                //(if further functionality for gen is implemented, can use below as well)
                
                //if any of the locations are inactive, we are an inward corner
                for (int i = 0; i < AboveLocRing.Length; i++)
                {
                    if (!WithinBounds(AboveLocRing[i].x + x,
                            AboveLocRing[i].y + y,
                            AboveLocRing[i].z + y)) continue;
                    
                    if (!_allVerts[GetIndex(AboveLocRing[i].x + x,
                            AboveLocRing[i].y + y,
                            AboveLocRing[i].z + z)].Active) return true;
                }
                
                return false;
            }
        }
    }
    
    public void CleanInvalidVerts()
    {
        //only clean from vert clean state
        if (generationState != EGenerationState.VertCleaning) return;
        
        for (int i = 0; i < _allVerts.Length; i++)
        {
            if (!_allVerts[i].Valid)
            {
                _allVerts[i].Active = false;
                
                _allVerts[i].WasCleaned = true;
            }
        }
    }
    
    
    //these values never change. Apply these verts to the position of each iteration during tri gen to get
    //the correct verts to try and add to a polyhedron face
    private Dictionary<EGlobalDirection, Vector3Int[]> _faceVertOffsets =
        new Dictionary<EGlobalDirection, Vector3Int[]>
        {
            { EGlobalDirection.Up , new []{new Vector3Int(0, 1, 1), new Vector3Int(1, 1, 1), new Vector3Int(1, 1, 0), new Vector3Int(0, 1, 0)}},
            { EGlobalDirection.Down , new []{new Vector3Int(1, 0, 1), new Vector3Int(0, 0, 1), new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0)}},
            { EGlobalDirection.Right , new []{new Vector3Int(1, 0, 1), new Vector3Int(1, 0, 0), new Vector3Int(1, 1, 0), new Vector3Int(1, 1, 1)}},
            { EGlobalDirection.Left , new []{new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 1), new Vector3Int(0, 1, 1), new Vector3Int(0, 1, 0)}},
            { EGlobalDirection.Forward , new []{new Vector3Int(0 ,0 , 1), new Vector3Int(1 ,0 , 1), new Vector3Int(1 , 1, 1), new Vector3Int(0 ,1 , 1)}},
            { EGlobalDirection.Back , new []{new Vector3Int(1 ,0 , 0), new Vector3Int(0 ,0, 0), new Vector3Int(0 ,1 , 0), new Vector3Int(1,1 , 0)}}
        };
    
    //used for easy offsetting and iteration through face cases
    private Dictionary<EGlobalDirection, Vector3Int> _globalDirOffsets =
        new Dictionary<EGlobalDirection, Vector3Int>
        {
            { EGlobalDirection.Up , new Vector3Int(0, 1, 0)},
            { EGlobalDirection.Down , new Vector3Int(0, -1, 0)},
            { EGlobalDirection.Right , new Vector3Int(1, 0, 0)},
            { EGlobalDirection.Left , new Vector3Int(-1, 0, 0)},
            { EGlobalDirection.Forward , new Vector3Int(0, 0, 1)},
            { EGlobalDirection.Back , new Vector3Int(0, 0, -1)}
        };

    //idea here is to work in stages from a cleaned set of verts
    //first we make a 3d array of polyhedrons, we then populate polyhedrons with verts in sets of 4 that we want to be
    //rendered, that we represent as faces
    public void GenerateTriangles()
    {
        //only gen after vert clean
        
        if (generationState != EGenerationState.ReadyToGenTriangles) return;

        //array of polyhedrons to hold information about the whole mesh.
        //note that the array isn't all mesh, some will be invalid or incomplete.
        Polyhedron[] polyhedrons = new Polyhedron[_currentSize.x * _currentSize.y * _currentSize.z];
        
        //adding faces to skip over cleaning passes
        List<Face> facesToSkip = new List<Face>();
        
        //poly array creation and face assign loop
        for (int z = 0; z < _currentSize.z; z++)
        {
            for (int y = 0; y < _currentSize.y; y++)
            {
                for (int x = 0; x < _currentSize.x; x++)
                {
                    Vector3Int currentLoc = new Vector3Int(x, y, z);
                    
                    //initializing and storing current polyhedron
                    Polyhedron currentPoly = polyhedrons[GetPolyIndex(currentLoc)] = new Polyhedron();
                    
                    currentPoly.Faces = new List<Face>();
                    
                    //using our dictionary, we can loop through the face directions and add the faces
                    foreach (var face in _faceVertOffsets)
                    {
                        //get the vert offsets for the face
                        Vector3Int[] vertOffsets = face.Value;
                        
                        //face dir
                        EGlobalDirection faceDir = face.Key;
                        
                        Vector3Int loc = new Vector3Int(x, y, z);
                        
                        Vector3Int placeLoc1 = loc + vertOffsets[0];
                        
                        Vector3Int placeLoc2 = loc + vertOffsets[1];
                        
                        Vector3Int placeLoc3 = loc + vertOffsets[2];
                        
                        Vector3Int placeLoc4 = loc + vertOffsets[3];
                        
                        //create a new face with the verts and direction
                        currentPoly.Faces.Add(new Face(
                            new List<Vertex>()
                            {
                                _allVerts[GetVertIndex(placeLoc1)],
                                _allVerts[GetVertIndex(placeLoc2)],
                                _allVerts[GetVertIndex(placeLoc3)],
                                _allVerts[GetVertIndex(placeLoc4)]
                            },
                            faceDir));
                    }
                }
            }
        }
        
        //first clean pass - majority of cases
        for (int z = 0; z < _currentSize.z; z++)
        {
            for (int y = 0; y < _currentSize.y; y++)
            {
                for (int x = 0; x < _currentSize.x; x++)
                {
                    Vector3Int currentLoc = new Vector3Int(x, y, z);
                    
                    Polyhedron currentPoly = polyhedrons[GetPolyIndex(currentLoc)];
                    
                    //logic for a 2d size, skips interior faces
                    if (_currentSize.x == 1 || _currentSize.y == 1 || _currentSize.z == 1)
                    {
                        CleanTwoDimensionalPolyAt(currentLoc, currentPoly);
                        
                        continue;
                    }
                    
                    //different logic for a 3d grid of polyhedrons
                    if (y > 0)
                    {
                        //(switched order to top) DELETE THIS IF NON ISSUE
                        //if valid up and not down.
                        //have to worry about horizontal and up faces, down already invalid.
                        //If checking left face, check left neighbor for valid left face. If valid, skip the left face.
                        if (!currentPoly.HasValidFaceInDir(EGlobalDirection.Down) &&
                            currentPoly.HasValidFaceInDir(EGlobalDirection.Up))
                        {
                            CleanYesUpNoDownPolyAt(currentLoc, currentPoly);
                            
                            continue;
                        }
                        
                        //skip all faces if no valid up but "valid" down
                        if (currentPoly.HasValidFaceInDir(EGlobalDirection.Down) &&
                            !currentPoly.HasValidFaceInDir(EGlobalDirection.Up))
                        {
                            CleanNoUpYesDownPolyAt(currentPoly);
                            
                            continue;
                        }
                        
                        //if valid up and down, specific cases involved.
                        //first, we guarantee adding down, up is already valid.
                        //for sides, we check if the Up face of each horizontal neighbor polyhedron is valid.
                        //if that is valid, skip the lateral face in said direction.
                        if (currentPoly.HasValidFaceInDir(EGlobalDirection.Down) &&
                            currentPoly.HasValidFaceInDir(EGlobalDirection.Up))
                        {
                            CleanYesUpYesDownPolyAt(currentLoc, currentPoly);
                            
                        }
                    }
                    else
                    {
                        //for bottom layer in 3D, determine faces shown by position.
                        //depending on if an object is on an edge, multiple, or none, horizontal faces NOT shown
                        //can be determined easily
                        //slight special case for vertical faces.
                        CleanGroundPolyAt(currentLoc, currentPoly);
                    }
                }
            }
        }
        
        
        
        //next clean - Polyhedrons above y 1 with no valid up or down face
        for (int z = 0; z < _currentSize.z; z++)
        {
            for (int y = 0; y < _currentSize.y; y++)
            {
                for (int x = 0; x < _currentSize.x; x++)
                {
                    if (y > 1)
                    {
                        Vector3Int currentLoc = new Vector3Int(x, y, z);
                    
                        Polyhedron currentPoly = polyhedrons[GetPolyIndex(currentLoc)];
                        
                        
                        //because traveling up y.
                        //checks each lateral face of current poly for if it's valid.
                        //if valid, read other poly one y level above poly in same direction.
                        //if that face is in the skip list, also skip the current face
                        if (!currentPoly.HasValidFaceInDir(EGlobalDirection.Up) &&
                            !currentPoly.HasValidFaceInDir(EGlobalDirection.Down))
                        {
                            CleanAboveY1NoUpDownPolyAt(currentLoc, currentPoly);
                        }
                    }
                }
            }
        }
        
        //final clean - y values above 0 with valid up and down face
        for (int z = 0; z < _currentSize.z; z++)
        {
            for (int y = 0; y < _currentSize.y; y++)
            {
                for (int x = 0; x < _currentSize.x; x++)
                {
                    if (y > 0)
                    {
                        Vector3Int currentLoc = new Vector3Int(x, y, z);
                        
                        Polyhedron currentPoly = polyhedrons[GetPolyIndex(currentLoc)];
                        
                        //compare to each lateral neighbor. 
                        //for example, if comparing to right neighbor.
                        //if right neighbor has a valid left face AND,
                        //current poly has valid right face AND,
                        //both aren't in the skip list,
                        //skip both
                        if (currentPoly.HasValidFaceInDir(EGlobalDirection.Down) &&
                            currentPoly.HasValidFaceInDir(EGlobalDirection.Up))
                        {
                            CleanAboveY0YesUpDownPolyAt(currentLoc, currentPoly);
                        }
                    }
                }
            }
        }
        
        foreach (var face in facesToSkip)
        {
            face.MarkedToSkip = true;
        }

        CalculateTriangles();
        
        //mark we're done generating tris
        generationState = EGenerationState.TrianglesGenerated;
        
        return;
        
        //for getting index of poly in array
        int GetPolyIndex(Vector3Int index)
        {
            return index.x + (index.y * _currentSize.x) + (index.z * _currentSize.x * _currentSize.y);
        }
        
        //for getting index of verts in the array
        int GetVertIndex(Vector3Int index)
        {
            return index.x + (index.y * (_currentSize.x + 1)) + (index.z * (_currentSize.x + 1) * (_currentSize.y + 1));
        }
        
        //for checking if a location is within the bounds of the array
        bool IsInPolyBounds(Vector3Int loc)
        {
            return loc.x >= 0 && loc.x < _currentSize.x &&
                   loc.y >= 0 && loc.y < _currentSize.y && 
                   loc.z >= 0 && loc.z < _currentSize.z;
        }
        
        void CalculateTriangles()
        {
            for (int z = 0; z < _currentSize.z; z++)
            {
                for (int y = 0; y < _currentSize.y; y++)
                {
                    for (int x = 0; x < _currentSize.x; x++)
                    {
                        Vector3Int currentLoc = new Vector3Int(x, y, z);
                        
                        foreach (var face in polyhedrons[GetPolyIndex(currentLoc)].Faces)
                        {
                            //add index of verts that make up the face
                            //If face does return values, they will be in the correct order for triangulation
                            //(0,1,2) + (0,2,3)
                            
                            List<Vertex> triVerts = face.TryGetTriVerts();
                            
                             if (triVerts == null) continue;
                            
                            foreach (var tri in triVerts)
                            {
                                _triangles.Add(Array.IndexOf(_allVerts, tri));
                            }
                        }
                    }
                }
            }
        }
        
        void CleanTwoDimensionalPolyAt(Vector3Int loc, Polyhedron poly)
        {
            //if a poly neighbor exists in surrounding locs and a face is shared, mark it to skip
                        //when generating tris for faces
            foreach (var face in poly.Faces)
            {
                foreach (var offset in _globalDirOffsets)
                {
                    Vector3Int otherLoc = new Vector3Int(loc.x + offset.Value.x,
                                                        loc.y + offset.Value.y,
                                                        loc.z + offset.Value.z);
                    
                    if (IsInPolyBounds(otherLoc))
                    {
                        foreach (var otherFace in polyhedrons[GetPolyIndex(otherLoc)].Faces)
                        {
                            //check we share all same valid verts
                            if (face.SharesAllVerts(otherFace))
                            {
                                //if check loc poly has valid face in this dir too, we can skip 
                                //(it'll cover the face)
                                
                                switch (offset.Key)
                                {
                                    case EGlobalDirection.Up:
                                        if (polyhedrons[GetPolyIndex(otherLoc)].
                                            HasValidFaceInDir(EGlobalDirection.Up))
                                        {
                                            facesToSkip.Add(face);
                                        }
                                        break;
                                    case EGlobalDirection.Down:
                                        if (polyhedrons[GetPolyIndex(otherLoc)].
                                            HasValidFaceInDir(EGlobalDirection.Down))
                                        {
                                            facesToSkip.Add(face);
                                        }
                                        break;
                                    case EGlobalDirection.Right:
                                        if (polyhedrons[GetPolyIndex(otherLoc)].
                                            HasValidFaceInDir(EGlobalDirection.Right))
                                        {
                                            facesToSkip.Add(face);
                                        }
                                        break;
                                    case EGlobalDirection.Left:
                                        if (polyhedrons[GetPolyIndex(otherLoc)].
                                            HasValidFaceInDir(EGlobalDirection.Left))
                                        {
                                            facesToSkip.Add(face);
                                        }
                                        break;
                                    case EGlobalDirection.Forward:
                                        if (polyhedrons[GetPolyIndex(otherLoc)].
                                            HasValidFaceInDir(EGlobalDirection.Forward))
                                        {
                                            facesToSkip.Add(face);
                                        }
                                        break;
                                    case EGlobalDirection.Back:
                                        if (polyhedrons[GetPolyIndex(otherLoc)].
                                            HasValidFaceInDir(EGlobalDirection.Back))
                                        {
                                            facesToSkip.Add(face);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        void CleanNoUpYesDownPolyAt(Polyhedron poly)
        {
            foreach (var face in poly.Faces)
            {
                facesToSkip.Add(face);
            }
        }
        
        void CleanYesUpYesDownPolyAt(Vector3Int loc, Polyhedron currentPoly)
        {
            foreach (var face in currentPoly.Faces)
            {
                //if down, insta skip
                if (face.FaceDirection == EGlobalDirection.Down)
                {
                    facesToSkip.Add(face);
                    
                    continue;
                }

                foreach (var offset in _globalDirOffsets)
                {
                    Vector3Int otherLoc = new Vector3Int(loc.x + offset.Value.x,
                        loc.y + offset.Value.y,
                        loc.z + offset.Value.z);

                    if (IsInPolyBounds(otherLoc))
                    {
                        //only care about lateral face
                        //checking the up face of each lateral poly. If valid, skip the face
                        
                        switch (offset.Key)
                        {
                            case EGlobalDirection.Up:
                                if (polyhedrons[GetPolyIndex(otherLoc)]
                                    .HasValidFaceInDir(EGlobalDirection.Up))
                                {
                                    facesToSkip.Add(currentPoly.
                                        GetFaceInDir(EGlobalDirection.Up));
                                }
                                break;
                            case EGlobalDirection.Right:
                                if (polyhedrons[GetPolyIndex(otherLoc)]
                                    .HasValidFaceInDir(EGlobalDirection.Up))
                                {
                                    facesToSkip.Add(currentPoly.
                                        GetFaceInDir(EGlobalDirection.Right));
                                }
                                
                                break;
                            case EGlobalDirection.Left:
                                if (polyhedrons[GetPolyIndex(otherLoc)]
                                    .HasValidFaceInDir(EGlobalDirection.Up))
                                {
                                    facesToSkip.Add(currentPoly.
                                        GetFaceInDir(EGlobalDirection.Left));
                                }

                                break;
                            case EGlobalDirection.Forward:
                                if (polyhedrons[GetPolyIndex(otherLoc)]
                                    .HasValidFaceInDir(EGlobalDirection.Up))
                                {
                                    facesToSkip.Add(currentPoly.
                                        GetFaceInDir(EGlobalDirection.Forward));
                                }

                                break;
                            case EGlobalDirection.Back:
                                if (polyhedrons[GetPolyIndex(otherLoc)]
                                    .HasValidFaceInDir(EGlobalDirection.Up))
                                {
                                    facesToSkip.Add(currentPoly.
                                        GetFaceInDir(EGlobalDirection.Back));
                                }

                                break;
                        }
                    }
                }
            }
        }
        
        void CleanYesUpNoDownPolyAt(Vector3Int loc, Polyhedron poly)
        {
            foreach (var face in poly.Faces)
            {
                foreach (var offset in _globalDirOffsets)
                {
                    Vector3Int otherLoc = new Vector3Int(loc.x + offset.Value.x,
                                                        loc.y + offset.Value.y,
                                                        loc.z + offset.Value.z);
                    
                    
                    if (IsInPolyBounds(otherLoc))
                    {
                        foreach (var otherFace in polyhedrons[GetPolyIndex(otherLoc)].Faces)
                        {
                            //check we share all same valid verts
                            if (face.SharesAllVerts(otherFace))
                            {
                                //if other poly has valid face in this dir too, we can skip 
                                //(it'll cover the face)
                                
                                switch (offset.Key)
                                {
                                    case EGlobalDirection.Up:
                                        if (polyhedrons[GetPolyIndex(otherLoc)].
                                            HasValidFaceInDir(EGlobalDirection.Up))
                                        {
                                            facesToSkip.Add(face);
                                        }
                                        
                                        break;
                                    case EGlobalDirection.Right:
                                        if (polyhedrons[GetPolyIndex(otherLoc)].
                                            HasValidFaceInDir(EGlobalDirection.Right))
                                        {
                                            facesToSkip.Add(face);
                                        }
                                        break;
                                    case EGlobalDirection.Left:
                                        if (polyhedrons[GetPolyIndex(otherLoc)].
                                            HasValidFaceInDir(EGlobalDirection.Left))
                                        {
                                            facesToSkip.Add(face);
                                        }
                                        break;
                                    case EGlobalDirection.Forward:
                                        if (polyhedrons[GetPolyIndex(otherLoc)].
                                            HasValidFaceInDir(EGlobalDirection.Forward))
                                        {
                                            facesToSkip.Add(face);
                                        }
                                        break;
                                    case EGlobalDirection.Back:
                                        if (polyhedrons[GetPolyIndex(otherLoc)].
                                            HasValidFaceInDir(EGlobalDirection.Back))
                                        {
                                            facesToSkip.Add(face);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        void CleanGroundPolyAt(Vector3Int loc, Polyhedron poly)
        {
            //x management
            if (loc.x == 0)
            {
                facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Right));
            }
            else if (loc.x == _currentSize.x - 1)
            {
                facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Left));
            }
            else
            {
                facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Left));
                facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Right));
            }
            
            //z management
            if (loc.z == 0)
            {
                facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Forward));
            }
            else if (loc.z == _currentSize.z - 1)
            {
                facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Back));
            }
            else
            {
                facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Forward));
                facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Back));
            }
            
            //if upPoly above has valid up face, skip poly up face
            Vector3Int upLoc = new Vector3Int(loc.x, loc.y + 1, loc.z);
            
            Polyhedron upPoly = polyhedrons[GetPolyIndex(upLoc)];
            
            
            if (upPoly.HasValidFaceInDir(EGlobalDirection.Up))
            {
                facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Up));
            }
        }
        
        void CleanAboveY1NoUpDownPolyAt(Vector3Int pos, Polyhedron poly)
        {

            Polyhedron downPoly = polyhedrons[GetPolyIndex(new Vector3Int(pos.x, pos.y - 1, pos.z))];
            
            
            if (poly.HasValidFaceInDir(EGlobalDirection.Right))
            {
                if (facesToSkip.Contains(downPoly.GetFaceInDir(EGlobalDirection.Right)))
                {
                    facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Right));
                }
            }
            
            if (poly.HasValidFaceInDir(EGlobalDirection.Left))
            {
                if (facesToSkip.Contains(downPoly.GetFaceInDir(EGlobalDirection.Left)))
                {
                    facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Left));
                }
            }
            
            if (poly.HasValidFaceInDir(EGlobalDirection.Forward))
            {
                if (facesToSkip.Contains(downPoly.GetFaceInDir(EGlobalDirection.Forward)))
                {
                    facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Forward));
                }
            }
            
            if (poly.HasValidFaceInDir(EGlobalDirection.Back))
            {
                if (facesToSkip.Contains(downPoly.GetFaceInDir(EGlobalDirection.Back)))
                {
                    facesToSkip.Add(poly.GetFaceInDir(EGlobalDirection.Back));
                }
            }
        }
        
        void CleanAboveY0YesUpDownPolyAt(Vector3Int loc, Polyhedron poly)
        {
            //removed foreach face in poly faces (REMOVE IF NO ISSUES)
            
            foreach (var offset in _globalDirOffsets)
            {
                Vector3Int checkLoc = new Vector3Int(loc.x + offset.Value.x,
                    loc.y + offset.Value.y,
                    loc.z + offset.Value.z);
                
                if (IsInPolyBounds(checkLoc))
                {
                    Face otherOppositeFace = null;

                    Face currentFace = null;
                    
                    Polyhedron otherPoly = polyhedrons[GetPolyIndex(checkLoc)];
                    
                    switch (offset.Key)
                    {
                        case EGlobalDirection.Right:
                            
                            otherOppositeFace = otherPoly.
                                GetFaceInDir(EGlobalDirection.Left);
                            
                            currentFace = poly.GetFaceInDir(EGlobalDirection.Right);
                            
                            if (otherOppositeFace != null && otherOppositeFace.Valid)
                            {
                                if (!facesToSkip.Contains(otherOppositeFace) &&
                                    !facesToSkip.Contains(currentFace))
                                {
                                    facesToSkip.Add(otherOppositeFace);
                                    
                                    facesToSkip.Add(currentFace);
                                }
                            }

                            break;
                        
                        case EGlobalDirection.Left:
                            
                            otherOppositeFace = otherPoly.
                                GetFaceInDir(EGlobalDirection.Right);
                            
                            currentFace = poly.GetFaceInDir(EGlobalDirection.Left);
                            
                            if (otherOppositeFace != null && otherOppositeFace.Valid)
                            {
                                if (!facesToSkip.Contains(otherOppositeFace) &&
                                    !facesToSkip.Contains(currentFace))
                                {
                                    facesToSkip.Add(otherOppositeFace);
                                    
                                    facesToSkip.Add(currentFace);
                                }
                            }

                            break;
                        
                        case EGlobalDirection.Forward:
                            
                            otherOppositeFace = otherPoly.
                                GetFaceInDir(EGlobalDirection.Back);
                            
                            currentFace = poly.GetFaceInDir(EGlobalDirection.Forward);
                            
                            if (otherOppositeFace != null && otherOppositeFace.Valid)
                            {
                                if (!facesToSkip.Contains(otherOppositeFace) &&
                                    !facesToSkip.Contains(currentFace))
                                {
                                    facesToSkip.Add(otherOppositeFace);
                                    
                                    facesToSkip.Add(currentFace);
                                }
                            }

                            break;
                        
                        case EGlobalDirection.Back:
                            
                            otherOppositeFace = otherPoly.
                                GetFaceInDir(EGlobalDirection.Forward);
                            
                            currentFace = poly.GetFaceInDir(EGlobalDirection.Back);
                            
                            if (otherOppositeFace != null && otherOppositeFace.Valid)
                            {
                                if (!facesToSkip.Contains(otherOppositeFace) &&
                                    !facesToSkip.Contains(currentFace))
                                {
                                    facesToSkip.Add(otherOppositeFace);
                                    
                                    facesToSkip.Add(currentFace);
                                }
                            }

                            break;
                    }
                }
            }
        }
    }
    
    public void ApplyToMesh()
    {
        //only come from tri gen
        if (generationState != EGenerationState.TrianglesGenerated) return;
        
        if (_currentMesh == null) return;
        
        _currentMesh.Clear();
        
        //We need to send the V3 vert positions as an array for the mesh verticies, and obviously we don't want extra
        //This brings the issue that the tri pointers will point to an old array that is bigger
        //The solution is to keep a reference to re-point the triangles to the same data in the new sized array
        
        Vertex[] orderedTriMatches = new Vertex[_triangles.Count];
        
        for (int i = 0; i < _triangles.Count; i++)
        {
            orderedTriMatches[i] = _allVerts[_triangles[i]];
        }

        Vertex[] validVerts = GetActiveValidVerts(_allVerts);
        
        _currentMesh.vertices = Array.ConvertAll(validVerts, x => x.Position);
        
        int[] triIndices = new int[_triangles.Count];
        
        for (int i = 0; i < _triangles.Count; i++)
        {
            triIndices[i] = Array.IndexOf(validVerts, orderedTriMatches[i]);
        }
        
        _currentMesh.triangles = triIndices;
        
        //Random Bull***t Go!
        
        _currentMesh.RecalculateNormals();
        
        // _currentMesh.RecalculateBounds();
        //
        // _currentMesh.RecalculateTangents();
        //
        // _currentMesh.OptimizeIndexBuffers();
        //
        // _currentMesh.Optimize();
        
        //mark we're done and only cleaning actions left
        generationState = EGenerationState.Cleaning;
    }
    
    
    //since we're generating child prefabs, once we're done we don't destroy the go but cut ties to variables
    public void Clear(bool destroy)
    {
        if (destroy)
        {
            if (_currentMeshGO != null)
            {
                DestroyImmediate(_currentMeshGO);
            }
            
            if (_currentMesh != null)
            {
                DestroyImmediate(_currentMesh);
            }
            
            if (_allVerts != null)
            {
                Array.Clear(_allVerts, 0, _allVerts.Length);
            }
            
            if (_triangles != null)
            {
                _triangles.Clear();
            }
        }
        else
        {
            if (_currentMeshGO != null)
            {
                _currentMeshGO.transform.parent = null;
            }
        }
        
        _currentMeshGO = null;
            
        _currentMesh = null;

        _allVerts = null;

        _triangles = null;

        _currentSize = Vector3Int.zero;
        
        generationState = EGenerationState.ReadyToGen;
    }
    
    private float GetNoiseValue(Vector3 position, float scale)
    {
        return Mathf.PerlinNoise(position.x / scale, position.z / scale);
    }
    
    private Vertex[] GetActiveInvalidVerts(Vertex[] verts)
    {
        Vertex[] invalidVerts = Array.FindAll(verts, x => x.Active && !x.Valid);
        
        return invalidVerts;
    }
    
    private Vertex[] GetActiveValidVerts(Vertex[] verts)
    {
        Vertex[] validVerts = Array.FindAll(verts, x => x.Active && x.Valid);

        return validVerts;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        
        if (_allVerts == null)
        {
            return;
        }
        
        for (int i = 0; i < _allVerts.Length; i++)
        {
            
            if (_allVerts[i].Active)
            {
                if (generationState != EGenerationState.TrianglesGenerated)
                {
                    Gizmos.color = _allVerts[i].Valid ? activeValidColor : activeInvalidColor;
                }
                else
                {
                    Gizmos.color = _allVerts[i].Valid ? triStateColor : activeInvalidColor;
                }
                
            }
            else
            {
                Gizmos.color = inactiveColor;
            }
            
            Gizmos.DrawSphere(_allVerts[i].Position, 0.1f);
        }
    }
}

#endif
