import maya.cmds as cmds
import itertools as itools
import numpy as np
import sys
import maya.mel as mel
import Sampler
import random



# ------ FACE ------------------------------------------
class Face:
    def __init__(self, vertices, tessellation):
        self.center = findCenter(vertices)
        self.vertices = vertices
        tessellation.addFaceEdges(self)
        for v in self.vertices:
            tessellation.addFaceToVertex(v,self)
            if tessellation.boundingTetra:
                if v in tessellation.boundingTetra.vertices and self not in tessellation.outerFaces:
                    tessellation.outerFaces.append(self)
            else:
                if self not in tessellation.outerFaces:
                    tessellation.outerFaces.append(self)


    def printCoords(self):
        for v in self.vertices:
            print v





def makeFace(vertices, tessellation):
    #print "Making face"
    a = tessellation.vertexToFaces.get(vertices[0], None)
    b = tessellation.vertexToFaces.get(vertices[1], None)
    c = tessellation.vertexToFaces.get(vertices[2], None)
    if a and b and c:
        faceList = [x for x in [a, b, c] if x]
        if len(faceList) < 3:
            #print "Making new face"
            return Face([vertices[0], vertices[1], vertices[2]],tessellation)
        else:
            sharedList = list(set(faceList[0]) & set(faceList[1]) & set(faceList[2]))
            if len(sharedList) == 0:
                #print "Making new face"
                return Face([vertices[0], vertices[1], vertices[2]],tessellation)
            else:
                #print "Existing face found"
                return sharedList[0]
    else:
        return Face([vertices[0], vertices[1], vertices[2]],tessellation)




# ------ TETRA -----------------------------------------
class Tetra:
    def __init__(self, vertices, tessellation):
        self.center = findCenter(vertices)

        self.vertices = clockwise(vertices[0],vertices[1],vertices[2],vertices[3])
        self.volume = self.calcVolume(self.vertices)
        self.faces = []
        # Faces are all possible 3 vertex combinations of the 4 points
        for f in itools.combinations(vertices, 3):
            newFace = makeFace([f[0], f[1], f[2]], tessellation)
            self.faces.append(newFace)
            tessellation.addTetraToFace(newFace,self)
            if newFace in tessellation.outerFaces and self not in tessellation.outerTets:
                tessellation.outerTets.append(self)

    def calcVolume(self,vertices):
        a = np.subtract(vertices[0],vertices[3])
        b = np.subtract(vertices[1],vertices[3])
        c = np.subtract(vertices[2],vertices[3])
        volume = (1.000/6.000) * np.linalg.norm(np.dot(a,np.cross(b,c)))
        return volume

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
        self.edges = {}
        self.tetras = []
        self.object = object

        self.boundingTetra = None
        self.outerFaces = []
        self.outerTets = []

    def makeGeo(self):
        groupName = str(object) + "_tessellation"
        tessellationGroup = cmds.group(em=True, name=groupName)
        for t in self.tetras:
            tetGeo = t.makeGeo()
            cmds.parent(tetGeo[0], tessellationGroup)

    def getCenters(self):
        return [t.center for t in self.tetras]

    def getVolumes(self):
        return [t.volume for t in self.tetras]

    def getTetsByVolume(self):
        sortedTets = sorted(self.tetras, key=lambda x: x.volume)
        return sortedTets

    def getCentersByVolume(self):
        return [t.center for t in self.getTetsByVolume()]

    def addVertex(self,v):
        self.vertices.append(v)
        if v not in self.vertexToFaces:
            self.vertexToFaces[v] = []
        else:
            print "ERROR WHILE ADDING: Vertex already in tessellation"

    def addEdge(self, v1, v2):
        if v1 in self.edges:
            if v2 not in self.edges[v1]:
                self.edges[v1].append(v2)
        else:
            self.edges[v1] = [v2]
        if v2 in self.edges:
            if v1 not in self.edges[v2]:
                self.edges[v2].append(v1)
        else:
            self.edges[v2] = [v1]

    def addFaceEdges(self,f):
        self.addEdge(f.vertices[0],f.vertices[1])
        self.addEdge(f.vertices[1],f.vertices[2])
        self.addEdge(f.vertices[2],f.vertices[0])

    def removeEdge(self,v1,v2):
        if v1 in self.edges:
            if v2 in self.edges[v1]:
                self.edges[v1].remove(v2)
        if v2 in self.edges:
            if v1 in self.edges[v2]:
                self.edges[v2].remove(v1)

    def removeEdgesByVertex(self,v):
        if v in self.edges:
            for n in self.edges[v]:
                if n in self.edges and v in self.edges[n]:
                    self.edges[n].remove(v)
            del self.edges[v]

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
        #else:
        #    print "ERROR WHILE REMOVING: vertex not attached to face"

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
        #print "\nInserting point at ",
        #print p

        #print "Debugging: Not checking circumsphere"
        #holeTetras = []
        #holeTetras.append(self.walk(p))

        # find all tetras who's circumspheres contain s
        holeTetras = self.findConflicting(p)

        #print len(holeTetras),
        #print " conflicting tetras found."
        #print "--------------------------"
        #for c in holeTetras:
        #    c.printCoords()
        #print "--------------------------"

        # remove containing tetras from T
        holeFaces = self.makeHole(holeTetras)

        #print len(holeFaces),
        #print " border faces found."
        #print "Border face neighbours: ",
        #for hf in holeFaces:
        #    print len(self.faceToTets[hf]),
        #print "\n"

        # create new tetras by joining hole faces with s
        self.fillHole(p,holeFaces)

        #print "Tetras after point insertion: ",
        #print len(self.tetras)
        #"--------------------------"
        #for t in self.tetras:
        #    t.printCoords()
        #"--------------------------"

    def findConflicting(self,p):
        containingTet = self.walk(p)
        #print "Containing tetra: "
        #print containingTet.printCoords()
        #print "Tetra has ",
        #print len(containingTet.vertices),
        #print " vertices"
        #for v in containingTet.vertices:
        #    print v

        conflictingTets = []
        visited = []
        conflictingTets = self.findCircumsphereTets(p,containingTet,conflictingTets,visited)
        if len(conflictingTets) == 0:
            #containingTet.makeGeo()
            print "ERROR: Containing tetra found but not in circumsphere"
            #pointSphere = cmds.polySphere(r = 0.3, sx= 2, sy=2)
            #cmds.move(p[0], p[1], p[2], pointSphere)
            #containingTet.makeGeo()
        return conflictingTets

    def findCircumsphereTets(self,p,t, conflictingTets, visited):
        visited.append(t)
        verts = t.vertices
        if inSphere(verts[0],verts[1],verts[2],verts[3],p) >= 0:
            conflictingTets.append(t)
            #print "Conflicting tetra found:"
            #t.printCoords()
            neighbours = self.neighboursOfTet(t)
            #print "Tetra has ",
            #print len(neighbours),
            #print " neighbours"
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
                if f in self.outerFaces:
                    self.outerFaces.remove(f)
            if t in self.tetras:
                self.tetras.remove(t)
            else:
                print "ERROR: Trying to remove tet that's not in tessellation"
                t.printCoords()
                #t.makeGeo()
                #quit()
            if t in self.outerTets:
                self.outerTets.remove(t)

        return borderFaces

    def fillHole(self,p,holeFaces):
        #"--------------------------"
        #print "Filling hole"

        for f in holeFaces:
            #"--------------------------"
            #print "face: "
            #f.printCoords()
            #print "face has ",
            #print len(self.faceToTets[f]),
            #print "neighbours"
            t = Tetra([f.vertices[0],f.vertices[1],f.vertices[2],p], self)
            #print "Adding tetra "
            #t.printCoords()
            neighbours = self.neighboursOfTet(t)
            #print "Tetra has ",
            #print len(neighbours),
            #print " neighbours"
            #t.printFaces()
            self.tetras.append(t)

    def finish(self):
        for v in self.boundingTetra.vertices:
            # remove edges from tessellation
            self.removeEdgesByVertex(v)
        for t  in self.outerTets:
            for f in t.faces:
                self.removeTetraFromFace(f,t)
                if f in self.outerFaces:
                    for v in f.vertices:
                        self.removeFaceFromVertex(v, f)
            # remove tet from tessellation
            if t in self.tetras:
                self.tetras.remove(t)
        self.outerFaces = []
        self.outerTets = []

    def shatter(self):
        if self.object:
            #cmds.setAttr(str(self.object) + '.visibility', 0)
            brokenGeoName = str(self.object) + "_broken"
            brokenGroup = cmds.group(em=True, name=brokenGeoName)
            for v in self.vertices:
                objectCopy = cmds.duplicate(self.object)
                cmds.setAttr(objectCopy[0] + '.visibility', 1)
                cmds.parent(objectCopy, brokenGroup)
                for n in self.edges[v]:
                    aim = [(vec1 - vec2) for (vec1, vec2) in zip(v, n)]

                    # Perpendicular bisector of both points
                    centerPoint = [(vec1 + vec2) / 2 for (vec1, vec2) in zip(v, n)]

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


def randomShatter(numberOfSamples,debug):
    object = cmds.ls(sl=True)
    for obj in object:
        dt = Tessellation(obj)
        bBox = cmds.exactWorldBoundingBox(obj)
        pointsX = [random.uniform(bBox[0], bBox[3]) for i in range(numberOfSamples)]
        pointsY = [random.uniform(bBox[1], bBox[3]) for i in range(numberOfSamples)]
        pointsZ = [random.uniform(bBox[2], bBox[5]) for i in range(numberOfSamples)]
        points = zip(pointsX, pointsY, pointsZ)
        dt.tessellate(points)
        if debug:
            dt.makeGeo()
        dt.shatter()


def unweightedShatter(numberOfSamples, debug):
    object = cmds.ls(sl=True)
    for obj in object:
        dt = Tessellation(obj)
        s = Sampler.VertexSampler(obj)
        points = s.randomSamples(numberOfSamples)
        dt.tessellate(points)
        dt.finish()
        if debug:
            dt.makeGeo()
        dt.shatter()

def unweightedShatterPercent(samplePercent, debug):
    object = cmds.ls(sl=True)
    for obj in object:
        dt = Tessellation(obj)
        s = Sampler.VertexSampler(obj)
        points = s.randomPercent(samplePercent)
        dt.tessellate(points)
        if debug:
            dt.makeGeo()
        dt.finish()
        dt.shatter()

def weightedShatter(numberOfSamples,samplePercent, debug):
    object = cmds.ls(sl=True)
    for obj in object:
        dt = Tessellation(obj)
        s = Sampler.VertexSampler(obj)
        points = s.randomPercent(samplePercent)
        dt.tessellate(points)
        dt.finish()
        newPoints = dt.getCentersByVolume()[0:numberOfSamples]
        dt2 = Tessellation(obj)
        dt2.tessellate(newPoints)
        dt2.finish()
        if debug:
            dt2.makeGeo()
        dt2.shatter()


def selectedShatter(numberOfSamples, debug):
    #object = cmds.ls(ls=True)
    object = cmds.filterExpand(sm = 31)
    print object
    objectName = ""
    points = []
    for obj in object:
        objName = obj.split('.vtx')[0]
        if objectName == "":
            objectName = objName

        if objName == objectName:
            vertex = cmds.xform(obj, q=True, ws=True, t=True)
            #print "Sampling vertex",
            #print obj,
            #print ": ",
            #print vertex
            if vertex not in points:
                points.append(vertex)

    if objectName != "":
        if len(points) > 0:
            object = cmds.ls(objectName)
            print object
            print points[0]
            dt = Tessellation(object)
            dt.tessellate(points)
            #dt.makeGeo()
            dt.finish()
            #dt.shatter()
            #newPoints = dt.getCentersByVolume()[0:numberOfSamples]
            newPoints = random.sample(dt.getCenters(),numberOfSamples)
            dt2 = Tessellation(object)
            dt2.tessellate(newPoints)
            dt2.finish()
            if (debug):
                dt2.makeGeo()
            dt2.shatter()
        else:
            print "ERROR: No points inserted"
