import maya.cmds as cmds
import maya.OpenMaya as py
import maya.api.OpenMaya as om
import random
import itertools as itools
import numpy as np
import sys

class vertex:
    def __init__(self, x,y,z):
        self.coords = (x,y,z)
        self.faces = []

class face:
    def __init__(self, vertices):
        self.tetras = []
        self.center = findCenter(vertices)
        # calcClockwise(vertices,self)
        self.normal = calcNormal(vertices[0], vertices[1], vertices[2])
        self.vertices = clockwise(vertices[0], vertices[1], vertices[2], self.center, self.normal)
        for v in self.vertices:
            if self not in v.faces:
                v.faces.append(self)

    def printCoords(self):
        for v in self.vertices:
            print v.coords

class tetra:
    def __init__(self, vertices):
        self.neighbours = {}
        self.vertices = vertices
        self.center = findCenter(vertices)
        # faces are all possible 3 vertex combinations of the 4 points
        faces = []
        # The combination code is working fine
        for f in itools.combinations(vertices, 3):
            print "Adding face to tetra at:"
            print f[0].coords
            print f[1].coords
            print f[2].coords
            newFace = makeFace(f[0], f[1], f[2])
            if newFace.tetras:
                neighbour = newFace.tetras[0]
                self.neighbours[newFace] = neighbour
                neighbour.neighbours[newFace] = self
            if self not in newFace.tetras:
                newFace.tetras.append(self)
            faces.append(newFace)
        self.faces = tuple(faces)
        #print "\nnumber of faces:"
        #print len(self.faces)

    def makeGeo(self):
        print "\nmaking geo"
        geoFaces = []
        #print "\nNumber of faces: "
        #print len(self.faces)
        for f in self.faces:
            v1 = f.vertices[0].coords
            v2 = f.vertices[1].coords
            v3 = f.vertices[2].coords
            geoFaces.append(cmds.polyCreateFacet(p=[v1,v2,v3])[0])
        tri = cmds.polyUnite(geoFaces)
        cmds.select(tri[0] + ".vtx[*]")
        cmds.polyMergeVertex()
        cmds.delete(tri, ch=True)
        return tri

    def printCoords(self):
        for v in self.vertices:
            print v.coords

    def printFaces(self):
        for f in self.faces:
            f.printCoords()

class triangulation:


    def __init__(self, object,points):
        self.tetras = []
        self.vertices = []
        for p in points:
            self.vertices.append(vertex(p[0], p[1], p[2]))

        bBox = cmds.exactWorldBoundingBox(object)
        center = cmds.objectCenter(object)
        self.boundingTetra = self.makeBigTriangle(center, bBox)
        print "Big triangle faces:"
        self.boundingTetra.printFaces()
        self.tetras.append(self.boundingTetra)
        # self.faces.append(self.boundingTet.faces)  THIS MIGHT BE WRONG
        self.Tessalate(self.vertices)

    def makeBigTriangle(self, center, bBox):
        """
        Make a tetrahedron that encompasses a 3d space
        :param center: Center point of the area
        :param bBox: Bounding box of the area
        :return: A new tetrahedron with all points of the bounding box internal to it
        """
        xDiff = bBox[3] - bBox[0]
        yDiff = bBox[4] - bBox[1]
        zDiff = bBox[5] - bBox[2]
        maxDiff = max(xDiff, yDiff, zDiff)
        sys.stdout.write(maxDiff)
        a = vertex(center[0], center[1] + maxDiff * 3, center[2])
        b = vertex(center[0], center[1] - maxDiff, center[2] + maxDiff * 3)
        c = vertex(center[0] + maxDiff * 3, center[1] - maxDiff, center[2] - maxDiff * 2)
        d = vertex(center[0] - maxDiff * 3, center[1] - maxDiff, center[2] - maxDiff * 2)
        verts = [a, b, c, d]
        print "\nMaking big triangle at "
        print a.coords
        print b.coords
        print c.coords
        print d.coords
        return tetra(verts)

    def removeTetra(self,tet):
        """
        Remove a tetrahedron from the tessellation 
        :param tet: Tetra to be removed
        :return: void
        """
        #if tet != self.boundingTetra:
            #tet.faces = []
            #tet.center = None

        for f in tet.faces:
            f.tetras.remove(tet)
        for f, n in tet.neighbours.items():
            #f.tetras.remove(tet)
            del n.neighbours[f]
            if len(f.tetras) == 0:
                for v in f.vertices:
                    v.faces.remove(f)

        tet.neighbours = {}
        if (tet in self.tetras):
            self.tetras.remove(tet)

    def Tessalate(self,points):
        for p in points:
            print "########################################"
            # All tetrahedra whose circumscribed spheres contain the point p are removed from the triangulation.
            print "Inserting point"
            print p.coords
            containingTetra = walk(p, self)
            print "Containing tetra: "
            containingTetra.printCoords()
            #containingTetra.printFaces()
            holeTetras = []
            visited = []
            holeTetras = tetsInSphere(p,containingTetra,holeTetras,visited)
            print "Number of hole tetras: "
            print len(holeTetras)
            holeBorderFaces = []
            # Find the edge faces of the hole
            for t in holeTetras:
                for f in t.faces:
                    print "Checking Hole tetra face:"
                    f.printCoords()
                    shared = False
                    if f in self.boundingTetra.faces:
                        print "Face borders bounding tri"
                        shared = True
                    else:
                        print "Face does not border bounding tri"
                        for n in f.tetras:
                            if n not in holeTetras:
                                print "Face borders non-hole tri"
                                shared = True
                            else:
                                print "Face does not border non-hole tri"
                        if len(f.tetras) == 0:
                            print "Face does not have ANY bordering tetras"
                    if shared:
                        holeBorderFaces.append(f)
                self.removeTetra(t)
            print "\nNumber of hole faces: "
            print len(holeBorderFaces)

            # Now fill in the hole made by removing the tetrahedrons
            for f in holeBorderFaces:
                f.printCoords()
                newTetra = tetra([f.vertices[0], f.vertices[1], f.vertices[2], p])
                # newTetra.printCoords()
                self.tetras.append(newTetra)

            print "\nNumber of tetras in dt after point insertion: "
            print len(self.tetras)

def makeFace(v1,v2,v3):
    #print "Making face at"
    #print vertices[0].coords
    #print vertices[1].coords
    #print vertices[2].coords

    a = v1.faces
    b = v2.faces
    c = v3.faces

    print "v1 faces:"
    print len(a)
    #for f in a:
    #    f.printCoords()
    print "v2 faces:"
    print len(b)
    #for f in b:
    #    f.printCoords()
    print "v3 faces:"
    print len(c)
    #for f in c:
    #    f.printCoords()

    #sharedFaces = list(set.intersection(*(set(x) for x in [a, b, c] if x)))

    faceList = [x for x in [a,b,c] if x]

    if len(faceList) < 3:
        print "At least one vertex without attached face, making new face"
        return face([v1,v2,v3])
    else:
        print "Vertices with faces found"
        sharedList = list(set(faceList[0]) & set(faceList[1]) & set(faceList[2]))
        if len(sharedList) > 0:
            print "Shared face found:"
            #sharedList[0].printCoords()
            return sharedList[0]
        else:
            print "Face not shared"
            return face([v1,v2,v3])


def tetsInSphere(p,tet, inSphereList, visited):
    """
    Find the list of tetrahedrons whos circumspheres include the point p
    :param p: 
    :param tet: starting tetrahedron
    :param inSphere: list of tetras who meet the criteria
    :param visited: list of tetras already checked
    :return: a list of tetraherons
    """
    visited.append(tet)
    if inSphere(tet.vertices[0], tet.vertices[1], tet.vertices[2], tet.vertices[3], p) >= 0:
        inSphereList.append(tet)
        for f in tet.faces:
            for n in f.tetras:
                if n not in visited:
                    tetsInSphere(p,n,inSphereList, visited)
    return inSphereList


def contains(p, tet, visited):
    neighbours = tet.neighbours
    if not tet.center:
        print "TET WITHOUT CENTER FOUND:"
        tet.printCoords()

    if len(neighbours) > 0:
        for face, neighbour in tet.neighbours.iteritems():
            a = orient(face.vertices[0], face.vertices[1], face.vertices[2], tet.center)
            b = orient(face.vertices[0], face.vertices[1], face.vertices[2], p)
            if a != b and neighbour not in visited:
                visited.append(tet)
                return contains(p, neighbour,visited)
    return tet


# Orthogonal walk
def walk(p, dt):
    print "\nNumber of tetras in dt: "
    print len(dt.tetras)
    tet = dt.tetras[0]
    visited = []
    if len(dt.tetras) == 1:
        print "Only bounding box found"
        return dt.boundingTetra
    else:
        return contains(p, tet, visited)


# Find the centroid point from a set of vertices
def findCenter(vertices):
    vertCount = len(vertices)
    # fuck
    vCoords = []
    for v in vertices:
        vCoords.append(v.coords)
    sumVert = [sum(x) for x in zip(*vCoords)]
    # you
    center = [a / vertCount for a in sumVert]
    # return (a / vertCount for a in [sum(x) for x in zip(vertices)])
    return vertex(center[0],center[1],center[2])


def calcClockwise(vertices, triFace, tetCenter):
    for tri in itools.combinations(vertices, 3):
        normal = calcNormal(tri.vertices[0], tri.vertices[1], tri.vertices[2])
        a = np.asarray(tri[0])
        c = np.asarray(tetCenter)
        cVector = np.dot(normal, c)
        if (cVector > 0):
            triFace.vertices = tri
            triFace.normal = normal
            return
    triFace.vertices = (vertices)
    triFace.normal = (normal)
    return


def calcNormal(v1, v2, v3):
    a = np.asarray(v1.coords)
    b = np.asarray(v2.coords)
    c = np.asarray(v3.coords)
    u = np.subtract(b, a)
    v = np.subtract(c, a)
    normal = np.linalg.norm(np.cross(u, v))

    return normal


def projectPoint(p, normal):
    return 0


def clockwise(v1, v2, v3, center, normal):
    vSorted = [v1, v2, v3]
    # Bubble sort
    for i in range(1, -1):
        for x in range(0, i + 1):
            a = vSorted[x]
            b = vSorted[x + 1]
            ab = np.linalg.dot(normal, np.cross(a.coords - center, b.coords - center))
            if ab > 0:
                vSorted[x] = b
                vSorted[x + 1] = a
    return tuple(vSorted)


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
    test = p.coords[0]
    matrix = np.matrix([[a.coords[0], a.coords[1], a.coords[2], 1],
                        [b.coords[0], b.coords[1], b.coords[2], 1],
                        [c.coords[0], c.coords[1], c.coords[2], 1],
                        [p.coords[0], p.coords[1], p.coords[2], 1]])

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
    aSquare = (a.coords[0] * a.coords[0]) + (a.coords[1] * a.coords[1]) + (a.coords[2] * a.coords[2])
    bSquare = (b.coords[0] * b.coords[0]) + (b.coords[1] * b.coords[1]) + (b.coords[2] * b.coords[2])
    cSquare = (c.coords[0] * c.coords[0]) + (c.coords[1] * c.coords[1]) + (c.coords[2] * c.coords[2])
    dSquare = (d.coords[0] * d.coords[0]) + (d.coords[1] * d.coords[1]) + (d.coords[2] * d.coords[2])
    pSquare = (p.coords[0] * p.coords[0]) + (p.coords[1] * p.coords[1]) + (p.coords[2] * p.coords[2])

    matrix = np.matrix([[a.coords[0], a.coords[1], a.coords[2], aSquare, 1],
                        [b.coords[0], b.coords[1], b.coords[2], bSquare, 1],
                        [c.coords[0], c.coords[1], c.coords[2], cSquare, 1],
                        [d.coords[0], d.coords[1], d.coords[2], dSquare, 1],
                        [p.coords[0], p.coords[1], p.coords[2], pSquare, 1]])

    determinant = np.linalg.det(matrix)
    return determinant


object = cmds.ls(sl=True)
#points = [[0,0,0],[2,2,2],[-1.5,-1.5,-1.5]]
#points = [[0,0,0]]
points = [[0,0,0],[2,2,2],[1,0,1],[-1,0,-1]]
#points = []
for obj in object:
    dt = triangulation(obj,points)
    for t in dt.tetras:
        t.makeGeo()
