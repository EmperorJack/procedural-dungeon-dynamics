import maya.cmds as cmds
import itertools as itools
import numpy as np
import sys
import maya.mel as mel
import Sampler
import vShatter



# ------ FACE ------------------------------------------
class Face:
    def __init__(self, vertices, tessellation):
        self.center = findCenter(vertices)
        self.vertices = vertices
        for v in self.vertices:
            tessellation.addFaceToVertex(v,self)

    def printCoords(self):
        for v in self.vertices:
            print v



def makeFace(vertices, tessellation):
    print "Making face"
    a = tessellation.vertexToFaces.get(vertices[0], None)
    b = tessellation.vertexToFaces.get(vertices[1], None)
    c = tessellation.vertexToFaces.get(vertices[2], None)
    if a and b and c:
        faceList = [x for x in [a, b, c] if x]
        if len(faceList) < 3:
            print "Making new face"
            return Face([vertices[0], vertices[1], vertices[2]],tessellation)
        else:
            sharedList = list(set(faceList[0]) & set(faceList[1]) & set(faceList[2]))
            if len(sharedList) == 0:
                print "Making new face"
                return Face([vertices[0], vertices[1], vertices[2]],tessellation)
            else:
                print "Existing face found"
                return sharedList[0]
    else:
        return Face([vertices[0], vertices[1], vertices[2]],tessellation)


# ------ TETRA -----------------------------------------
class Tetra:
    def __init__(self, vertices, tessellation):
        self.center = findCenter(vertices)

        self.vertices = clockwise(vertices[0],vertices[1],vertices[2],vertices[3])

        self.faces = []
        # Faces are all possible 3 vertex combinations of the 4 points
        for f in itools.combinations(vertices, 3):
            newFace = makeFace([f[0], f[1], f[2]], tessellation)
            self.faces.append(newFace)
            tessellation.addTetraToFace(newFace,self)

    def printCoords(self):
        for v in self.vertices:
            if v == None:
                print "NONE VERTEX FOUND"
            print v

    def printFaces(self):
        for f in self.faces:
            f.printCoords()
            print "\n"

    def makeGeo(self):
        geoFaces = []
        for f in self.faces:
            v1 = f.vertices[0]
            v2 = f.vertices[1]
            v3 = f.vertices[2]
            geoFaces.append(cmds.polyCreateFacet(p=[v1, v2, v3])[0])
        tri = cmds.polyUnite(geoFaces)
        cmds.select(tri[0] + ".vtx[*]")
        cmds.polyMergeVertex()
        cmds.delete(tri, ch=True)
        return tri

# ------ TESSELLATION ------------------------------------
class Tessellation:

    def __init__(self,object):
        self.faceToTets = {}
        self.vertexToFaces = {}
        self.vertices = []
        self.tetras = []
        self.object = object

        #self.boundingTetra = None

    def makeGeo(self):
        for t in self.tetras:
            t.makeGeo()

    def getCenters(self):
        return [t.center for t in self.tetras]

    def addVertex(self,v):
        self.vertices.append(v)
        if v not in self.vertexToFaces:
            self.vertexToFaces[v] = []
        else:
            print "ERROR WHILE ADDING: Vertex already in tessellation"

    def addFaceToVertex(self,v,f):
        if v in self.vertexToFaces:
            vertFaces = self.vertexToFaces[v]
            if f not in vertFaces:
                self.vertexToFaces[v].append(f)
            else:
                print "ERROR WHILE ADDING: Face already attached to vertex"
        else:
            self.vertexToFaces[v] = [f]

    def removeFaceFromVertex(self,v,f):
        if f in self.vertexToFaces[v]:
            self.vertexToFaces[v].remove(f)
        else:
            print "ERROR WHILE REMOVING: vertex not attached to face"

    def addFace(self,f):
        for v in f.vertices:
            if f not in self.vertexToFaces[v]:
                self.vertexToFaces[v].append(f)
        if f not in self.facesToTets:
            self.facesToTets[f] = []
        else:
            print "ERROR WHILE ADDING: Face already in tessellation"

    def removeFace(self,f):
        for v in f.vertices:
            self.removeFaceFromVertex(v,f)

    def addTetraToFace(self,f,t):
        if f in self.faceToTets:
            faceTemp = self.faceToTets[f]
            faceTets = faceTemp[:]
            if t not in faceTets:
                if len(faceTets) < 2:
                    self.faceToTets[f].append(t)
                else:
                    print "ERROR WHILE ADDING: face already has 2 tets attached."
                    print "FACE:"
                    f.printCoords()
                    print "----- Attached tets: -----"
                    for n in faceTets:
                        n.printCoords()
                    print "--------------------------"
                    for tt in faceTets:
                        t.makeGeo()
                        print "tet has ",
                        print len(t.faces),
                        print " faces"
                        if t in self.tetras:
                            print "tet in tetra list"
                        else:
                            print "tet not in tetra list!!!"
                            #self.faceToTets[f].remove(tt)
                    #quit("Incorrect edge setup")
                    print "Attaching"
                    self.faceToTets[f].append(t)
            else:
                print "ERROR WHILE ADDING: tet already attached to face"
        else :
            self.faceToTets[f] = [t]

    def removeTetraFromFace(self,f,t):
        if t in self.faceToTets[f]:
            self.faceToTets[f].remove(t)
        else:
            print "ERROR IN REMOVING: tet not assigned to face"


    def addTetra(self,t):
        if t not in self.tetras:
            self.tetras.append(t)
        else:
            print "ERROR: Tetra already in tessellation"



    def neigbourFromFace(self,f,t):
        allTets = self.faceToTets[f]
        neighbour = allTets[:]
        if t in neighbour:
            neighbour.remove(t)
            noOfNeighbours = len(neighbour)
            if noOfNeighbours == 1:
                return neighbour[0]
            elif noOfNeighbours == 0:
                return None
            else:
                print "ERROR: MORE THAN ONE NEIGHBOUR FOR FACE"
                print "Returning first neighbour"
                return neighbour[0]

    def neighboursOfTet(self,t):
        neighbours = []
        for f in t.faces:
            n = self.neigbourFromFace(f,t)
            if n:
                neighbours.append(n)
        return neighbours


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
        a = (center[0], center[1] + maxDiff * 3, center[2])
        b = (center[0], center[1] - maxDiff, center[2] + maxDiff * 3)
        c = (center[0] + maxDiff * 3, center[1] - maxDiff, center[2] - maxDiff * 2)
        d = (center[0] - maxDiff * 3, center[1] - maxDiff, center[2] - maxDiff * 2)
        verts = [a, b, c, d]

        return Tetra(verts,self)

    def printInfo(self):
        print "========================================================="
        print "                     TESSELLATION                        "
        print "========================================================="
        print len(self.tetras),
        print " tetras in tessellation."
        print "Bounding Tetra:"
        self.boundingTetra.printCoords()
        self.printTetras()


    def printTetras(self):
        for t in self.tetras:
            print "=== TETRA ================================="
            t.printCoords()
            neighbours = self.neighboursOfTet(t)
            print len(neighbours),
            print " neighbours of tet"
            for n in neighbours:
                n.printCoords()



    def tessellate(self,points):
        if self.object:
            for p in points:
                v = (p[0], p[1], p[2])
                self.addVertex(v)
            bBox = cmds.exactWorldBoundingBox(self.object)
            center = cmds.objectCenter(self.object)
            self.boundingTetra = self.makeBigTriangle(center, bBox)
            self.addTetra(self.boundingTetra)

            for v in self.vertices:
                self.insertPoint(v)
        else:
            print "Error: No object assigned to tessellation"

    def insertPoint(self,p):
        print "\nInserting point at ",
        print p

        #print "Debugging: Not checking circumsphere"
        #holeTetras = []
        #holeTetras.append(self.walk(p))

        # find all tetras who's circumspheres contain s
        holeTetras = self.findConflicting(p)

        print len(holeTetras),
        print " conflicting tetras found."
        print "--------------------------"
        for c in holeTetras:
            c.printCoords()
        print "--------------------------"

        # remove containing tetras from T
        holeFaces = self.makeHole(holeTetras)

        print len(holeFaces),
        print " border faces found."
        print "Border face neighbours: ",
        for hf in holeFaces:
            print len(self.faceToTets[hf]),
        print "\n"

        # create new tetras by joining hole faces with s
        self.fillHole(p,holeFaces)

        print "Tetras after point insertion: ",
        print len(self.tetras)
        "--------------------------"
        for t in self.tetras:
            t.printCoords()
        "--------------------------"

    def findConflicting(self,p):
        containingTet = self.walk(p)
        print "Containing tetra: "
        print containingTet.printCoords()
        print "Tetra has ",
        print len(containingTet.vertices),
        print " vertices"
        for v in containingTet.vertices:
            print v

        conflictingTets = []
        visited = []
        conflictingTets = self.findCircumsphereTets(p,containingTet,conflictingTets,visited)
        if len(conflictingTets) == 0:
            #containingTet.makeGeo()
            print "ERROR: Containing tetra found but not in circumsphere"
            pointSphere = cmds.polySphere(r = 0.3, sx= 2, sy=2)
            cmds.move(p[0], p[1], p[2], pointSphere)
            containingTet.makeGeo()
        return conflictingTets

    def findCircumsphereTets(self,p,t, conflictingTets, visited):
        visited.append(t)
        verts = t.vertices
        if inSphere(verts[0],verts[1],verts[2],verts[3],p) >= 0:
            conflictingTets.append(t)
            print "Conflicting tetra found:"
            t.printCoords()
            neighbours = self.neighboursOfTet(t)
            print "Tetra has ",
            print len(neighbours),
            print " neighbours"
            for n in neighbours:
                if n not in visited:
                    conflictingTets = self.findCircumsphereTets(p, n, conflictingTets, visited)
        return conflictingTets

    def walk(self,p):
        t = self.tetras[0]
        visited = []
        return self.tetContainsPoint(p, t, visited)

    def tetContainsPoint(self, p, t, visited):
        visited.append(t)
        nextTet = None
        for f in t.faces:
            n = self.neigbourFromFace(f,t)
            if n:
                verts = clockwise(f.vertices[0], f.vertices[1], f.vertices[2], t.center)
                a = orient(verts[0],verts[1],verts[2],t.center)
                b = orient(verts[0],verts[1],verts[2],p)
                c = a * b
                # THIS IS WHAT'S BREAKING, PRETTY SURE
                if c < 0 and a != b and n not in visited:
                    nextTet = n
                    break
        if nextTet:
            return self.tetContainsPoint(p,nextTet,visited)
        else:
            return t

    def makeHole(self,holeTetras):
        # Remove tetras from set, return the bordering faces
        borderFaces = []
        internalHoleFaces = []

        # First, build lists of faces that border the hole and faces internal to the hole
        for t in holeTetras:
            for f in t.faces:
                n = self.neigbourFromFace(f,t)
                if not n or n not in holeTetras:
                    # If the face borders a non-hole tetra or the edge of the tessellation
                    # Add it to the list of faces to keep
                    if f not in borderFaces:
                        borderFaces.append(f)
                else:
                    # Otherwise; remove face entirely from tessellation
                    if f not in internalHoleFaces:
                        internalHoleFaces.append(f)

        # Then remove tetras + internal faces from tessellation
        for t in holeTetras:
            for f in t.faces:
                self.removeTetraFromFace(f,t)
                if f in internalHoleFaces:
                    for v in f.vertices:
                        self.removeFaceFromVertex(v, f)
            if t in self.tetras:
                self.tetras.remove(t)
            else:
                print "ERROR: Trying to remove tet that's not in tessellation"
                t.printCoords()
                t.makeGeo()
                quit()

        return borderFaces

    def fillHole(self,p,holeFaces):
        "--------------------------"
        print "Filling hole"

        for f in holeFaces:
            "--------------------------"
            print "face: "
            f.printCoords()
            print "face has ",
            print len(self.faceToTets[f]),
            print "neighbours"
            t = Tetra([f.vertices[0],f.vertices[1],f.vertices[2],p], self)
            print "Adding tetra "
            t.printCoords()
            neighbours = self.neighboursOfTet(t)
            print "Tetra has ",
            print len(neighbours),
            print " neighbours"
            #t.printFaces()
            self.tetras.append(t)

    def shatter(self):
        if self.object:
            cmds.setAttr(object[0] + '.visibility', 0)
            brokenGeoName = object[0] + "_broken"
            brokenGroup = cmds.group(em=True, name=brokenGeoName)
            for t in self.tetras:
                objectCopy = cmds.duplicate(self.object)
                for n in self.neighboursOfTet(t):
                    aim = [(vec1 - vec2) for (vec1, vec2) in zip(t.center, n.center)]

                    # Perpendicular bisector of both points
                    centerPoint = [(vec1 + vec2) / 2 for (vec1, vec2) in zip(t.center, n.center)]

                    # Angle of cutting plane
                    planeAngle = cmds.angleBetween(euler=True, v1=[0, 0, 1], v2=aim)

                    # Apply cutting plane and close hole made in geometry
                    cmds.polyCut(objectCopy[0], df=True, cutPlaneCenter=centerPoint, cutPlaneRotate=planeAngle)
                    cmds.polyCloseBorder(objectCopy[0])
                    cmds.polyTriangulate(objectCopy[0])
                    cmds.polyQuad(objectCopy[0])
                cmds.xform(objectCopy, cp=True)
                print str(objectCopy)
            cmds.xform(brokenGroup, cp=True)
        else:
            print "Error: Tessellation has no object assigned"




# Find the centroid point from a set of vertices
def findCenter(vertices):
    vertCount = len(vertices)
    vCoords = []
    for v in vertices:
        vCoords.append(v)
    sumVert = [sum(x) for x in zip(*vCoords)]
    center = [a / vertCount for a in sumVert]
    return tuple([center[0], center[1], center[2]])

def clockwise(v1,v2,v3,v4):
    # sort v1,v2,v3 so that they're clockwise with respect to v4
    # return [sorted(v1,v2,v3),v4]

    a = v1
    b = v2
    c = v3
    d = v4

    normal = calcNormal(a, b, c)
    directionVector = np.subtract(b,d)
    direction = np.dot(normal,directionVector)
    #print "Direction: ",
    #print direction
    if (direction < 0):
        return [v3,v2,v1,v4]
    else:
        return [v1,v2,v3,v4]


def calcNormal(v1, v2, v3):
    a = np.asarray(v1)
    b = np.asarray(v2)
    c = np.asarray(v3)
    u = np.subtract(b, a)
    v = np.subtract(c, a)
    #normal = np.linalg.norm(np.cross(u, v))
    normal = np.cross(u,v)
    return normal

def orient(a, b, c, d):
    """
    	| Ax Ay Az 1 |
    	| Bx By Bz 1 |
    	| Cx Cy Cz 1 |
    	| Px Py Pz 1 |
    	Return determinant
        Returns a positive value when the point d is
        above the plane defined by a, b and c; a negative value
        if d is under the plane; and exactly 0 if p is directly on
        the plane.
    """

    matrix = np.matrix([[a[0], a[1], a[2], 1],
                        [b[0], b[1], b[2], 1],
                        [c[0], c[1], c[2], 1],
                        [d[0], d[1], d[2], 1]])

    determinant = np.linalg.det(matrix)
    return determinant

def inSphere(a,b,c,d,e):

    aSquare = (a[0] * a[0]) + (a[1] * a[1]) + (a[2] * a[2])
    bSquare = (b[0] * b[0]) + (b[1] * b[1]) + (b[2] * b[2])
    cSquare = (c[0] * c[0]) + (c[1] * c[1]) + (c[2] * c[2])
    dSquare = (d[0] * d[0]) + (d[1] * d[1]) + (d[2] * d[2])
    pSquare = (e[0] * e[0]) + (e[1] * e[1]) + (e[2] * e[2])


    matrix = np.matrix([[a[0], a[1], a[2], aSquare, 1],
                        [b[0], b[1], b[2], bSquare, 1],
                        [c[0], c[1], c[2], cSquare, 1],
                        [d[0], d[1], d[2], dSquare, 1],
                        [e[0], e[1], e[2], pSquare, 1]])

    determinant = np.linalg.det(matrix)
    orientValue = orient(a,b,c,d)
    #return determinant * orientValue
    return determinant


def getVertices(object):
    #verts = cmds.ls('%s.vtx[:]' % obj, fl=True)
    #vtxIndexList = cmds.getAttr(object + ".vrts", multiIndices=True)
    #print verts
    #print vtxIndexList
    cmds.select(object)
    mel.eval("PolySelectConvert 3")
    vertList = cmds.ls(sl=1, fl=1)
    print vertList



object = cmds.ls(sl=True)
for obj in object:
    dt = Tessellation(obj)
    s = Sampler.VertexSampler(obj)
    points = s.randomSamples(12)
    dt.tessellate(points)
    #dt.makeGeo()
    dt.shatter()
    dt.printInfo()


#points = [[0,0,0],[-1,0,-1]]
#points = [[0,0,0]]
#points = [[0,0,0],[2,2,2]]
#points = [[0,0,0],[2,2,2],[-1,0,-1]]
#points = []


"""
def vertexSampler(object, samplePercent):
    cmds.select(object + ".vtx[*]", r=True)
    vertPosTemp = cmds.xform(object + '.vtx[*]', q=True, ws=True, t=True)
    vertices = zip(*[iter(vertPosTemp)] * 3)
    print object,
    print " has ",
    print len(vertices),
    print " vertices."
    if samplePercent < 100 and samplePercent > 0:
        print "Sampling ",
        print samplePercent,
        print " percent (",
        h = len(vertices) * (samplePercent / 100.0)
        print h,
        print " rounded to",
        numberOfSamples = (int)(round(h))
        print numberOfSamples,
        print " vertices)"
        return random.sample(set(vertices),numberOfSamples)
    else:
        print "Sampling all vertices"
        return vertices


object = cmds.ls(sl=True)
for obj in object:
    getVertices(obj)
    dt = Tessellation()
    points = vertexSampler(obj,50)
    #print points
    #points = [[0,0,0]]
    #points = [[0, 0, 0],[-5.0,-5.0,-5.0]]
    #points = [[-5.0, -5.0, 5.0], [5.0, -5.0, 5.0]]
    #points = [[0,0,0],[1,0,0],[0,1,0],[0,0,1]]

    #dt.tessellate(obj,points)
    #dt.makeGeo()
    #dt.printInfo()
"""

