import maya.cmds as cmds
import maya.OpenMaya as py
import maya.api.OpenMaya as om
import random
import itertools as itools
import numpy as np

class face:
    vertices = ()
    center = ()
    normal = np.array()
    def __init__(self, vertices, tetCenter):
        self.center = findCenter(vertices)
        self.normal = calcNormal(vertices[0], vertices[1], vertices[2])



class tetra:
    # both vertices and faces are tuples of tuples
    # they are immutable - if either changes we wil have a new tetra
    vertices = ()
    faces = ()
    center = ()
    # neighours is a dictionary that stores neighbouring tetras, where the key is the face
    neighbours = {}
    def __init__(self, vertices):
        self.vertices = vertices
        # center = sum(x)/4, sum(y)/4, sum(z)/4
        # self.center = (a/4 for a in [sum(x) for x in zip(vertices)])
        self.center = findCenter(vertices)
        # faces are all possible 3 vertex combinations of the 4 points
        faces = itools.permutations(vertices, 3)
        # need to make sure vertices are always clockwise, Left Hand Rule
        # --- CLOCKWISE CODE ---

def insertPoint(p,dt):

    """
    Input: A DT(S) T in R^3, and a new point p to insert
    Output: Tp = T ∪ {p}
    1: τ ← Walk {to obtain tetra containing p}
    2: insert p in τ with a flip14
    3: push 4 new tetrahedra on stack
    4: while stack is non-empty do
    5:  τ = {p, a, b, c} ← pop from stack
    6:  τa = {a, b, c, d} ← get adjacent tetrahedron of τ having abc as a facet
    7:  if d is inside circumsphere of τ then
    8:      Flip(τ , τa)
    9:  end if
    10:end while
    """

    tetraStack = []

    # find tetra that contains p
    containingTetra = walk(p, dt)

    # insert point into tetra
    newTetras = flip14(p, dt)

    # push the four resulting tetras onto the stack
    for tet in newTetras:
        tetraStack.append(tet)

    while len(tetraStack) != 0:
        t = tetraStack.pop()
        # get adjacent tetra of t that has abc as a facet
        v = t.vertices
        face = (v[1], v[2], v[3])
        ta = t.neighbours[face]
        circumsphere = inSphere(t.vertices[0],t.vertices[1],t.vertices[2],t.vertices[3],ta.vertices[3])
        if circumsphere > 0:
            flip(t, ta)

def contains(p, tet, visited):
    """
    starting from a dsimplex
    σ, we move to one of the neighbours of σ (σ
    has (d + 1)-neighbours; we choose one neighbour such
    that the query point p and σ are on each side of the
    (d − 1)-simplex shared by σ and its neighbour) until
    there is no such neighbour, then the simplex containing
    p is σ.
    """
    for face, neighbour in tet.neighbours:
        a = orient(face[0], face[1], face[2], tet.center)
        b = orient(face[0], face[1], face[2], p)
        if a != b and neighbour not in visited:
            visited.append(tet)
            return contains(p, neighbour)

    return tet

# Orthogonal walk
def walk(p, dt):
    tet = dt[0]
    visited = []
    return contains(p, tet, visited)

# Find the centroid point from a set of vertices
def findCenter(vertices):
    vertCount = len(vertices)
    return (a / vertCount for a in [sum(x) for x in zip(vertices)])

def calcNormal(v1, v2, v3):
    a = np.asarray(v1)
    b = np.asarray(v2)
    c = np.asarray(v3)
    u = np.subtract(b,a)
    v = np.subtract(c,a)
    normal = np.linalg.norm(np.cross(u,v))

    return normal

def projectPoint(p,normal):
    return 0

def clockwise(v1,v2,v3,center, normal):
    # normalX = rotate normal 90 around y
    # normalY = rotate normal 90 around x
    # project each vertex onto the x and y axis (using dot product)
    # then use atan2 to find angles
    # sort by angles
    return 0


def orient(a, b, c, p):
    """
    	| Ax Ay Az 1 |
    	| Bx By Bz 1 |
    	| Cx Cy Cz 1 |
    	| Px Py Pz 1 |
    	Return determinant
        Returns a positive value when the point p is
        above the plane defined by a, b and c; a negative value
        if p is under the plane; and exactly 0 if p is directly on
        the plane.
    """

    matrix = np.matrix([[a[0], a[1], a[2], 1],
                       [b[0], b[1], b[2], 1],
                       [c[0], c[1], c[2], 1],
                       [p[0], p[1], p[2], 1]])

    determinant = np.linalg.det(matrix)
    return determinant


def inSphere(a, b, c, d, p):
    """
	| Ax Ay Az (Ax^2 + Ay^2 + Az^2) 1 |
	| Bx By Bz (Bx^2 + By^2 + Bz^2) 1 |
	| Cx Cy Cz (Cx^2 + Cy^2 + Cz^2) 1 |
	| Dx Dy Dz (Dx^2 + Dy^2 + Dz^2) 1 |
	| Px Py Pz (Px^2 + Py^2 + Pz^2) 1 | 
	return determinant
	a positive value is returned if p is inside the sphere; a negative if
    p is outside; and exactly 0 if p is directly on the sphere.
    """
    aSquare = (a[0]*a[0]) + (a[1]*a[1]) + (a[2]*a[2])
    bSquare = (b[0] * b[0]) + (b[1] * b[1]) + (b[2] * b[2])
    cSquare = (c[0] * c[0]) + (c[1] * c[1]) + (c[2] * c[2])
    dSquare = (d[0] * d[0]) + (d[1] * d[1]) + (d[2] * d[2])
    pSquare = (p[0] * p[0]) + (p[1] * p[1]) + (p[2] * p[2])

    matrix = np.matrix([[a[0], a[1], a[2], aSquare, 1],
                       [b[0], b[1], b[2], bSquare, 1],
                       [c[0], c[1], c[2], cSquare, 1],
                       [d[0], d[1], d[2], dSquare, 1],
                       [p[0], p[1], p[2], pSquare, 1]])

    determinant = np.linalg.det(matrix)
    return determinant


def flip(tet1, tet2):
    """
    Input: Two adjacent tetrahedra τ and τa
    Output: A flip is performed, or not
    1: if case #1 then
    2: flip23(τ , τa)
    3: push tetrahedra pabd, pbcd and pacd on stack
    4: else if case #2 AND Tp has tetrahedron pdab
    then
    5: flip32(τ , τa, pdab)
    6: push pacd and pbcd on stack
    7: else if case #3 AND τ and τa are in config44 with
    τb and τc then
    8: flip44(τ , τa, τb, τc)
    9: push on stack the 4 tetrahedra created
    10: else if case #4 then
    11: flip23(τ , τa)
    12: push tetrahedra pabd, pbcd and pacd on stack
    13: end if
    """


def flip14(p,t):
    # a flip 14 inserts a point into a tetrahedra, splitting it into 4
    newTetras = []
    adjacency = {}

    # Make a new tetra from vertex of each face of original tetra + new point
    for face in t:
        tet = tetra([p,face[0],face[1],face[2]])
        # Set neighbours based on shared faces
        for tFace in tet:
            # if the face is not one of the original tetra faces, it will only neighbour the new tetras we create
            if tFace != face:
                if tFace in adjacency:
                    for neighbour in adjacency[tFace]:
                        neighbour.neighbours[tFace] = tet
                        tet.neighbours[tFace] = neighbour
                    adjacency[tFace].append(tet)
                else:
                    adjacency[tFace] = [tet]
            # if the face is one of the original tetra faces, it borders external tetras that we need to update
            else:
                t.neighbours[tFace].neighbours[tFace] = tet
                tet.neighbours[tFace] = t.neighbours[tFace]
        newTetras.append(tet)
    return newTetras

def flip23(t, ta):
    # Two tetrahedrons sharing  face bcd
    # Return two tetrahedrons from same vertices, but sharing face ace
    t1 = tetra((t.vertices[0],t.vertices[1],ta.vertices[0], t.vertices[2])) # NOT SURE OF INDEXING HERE
    t2 = tetra ((t.vertices[0],t.vertices[1],ta.vertices[0], t.vertices[3])) # NOT SURE OF INDEXING HERE
    # how to update neighbours?
    return [t1, t2]

def flip32(t, ta):
    # opposite of 23
    return 0

def flip44(t, ta):
    # do something
    return 0








