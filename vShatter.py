import maya.cmds as cmds
import random

def vShatter(object):
    sampleCount = 10
    # need flag here to run different sample methods
    samplePoints = randomSample(object, sampleCount)
    cmds.setAttr(object + '.visibility', 0)
    brokenGeoName = object + "_broken"
    brokenGroup = cmds.group(em=True, name=brokenGeoName)
    doShatter(object,brokenGroup,samplePoints)


# Get a uniformly distributed sample of points from an object's bounding box
def randomSample(object, sampleCount):
    bBox = cmds.exactWorldBoundingBox(object)
    pointsX = [random.uniform(bBox[0], bBox[3]) for i in range(sampleCount)]
    pointsY = [random.uniform(bBox[1], bBox[3]) for i in range(sampleCount)]
    pointsZ = [random.uniform(bBox[2], bBox[5]) for i in range(sampleCount)]
    samplePoints = zip(pointsX,pointsY,pointsZ)
    return samplePoints

# Given an object and a set of sample points
# Divide object into voronoi polygons (with sample points as centers).
def doShatter(object, brokenGroup, samplePoints):

    for pointFrom in samplePoints:
        # New copy of geo for every polygon center
        # Allows us to carve away rest of geometry and leave polygon
        objectCopy = cmds.duplicate(object)
        if (objectCopy[0] == None):
            print("no object found")
            print str(objectCopy)
        cmds.setAttr(str(objectCopy[0]) + '.visibility', 1)
        cmds.parent(objectCopy, brokenGroup)
        for pointTo in samplePoints:
            if pointTo != pointFrom:
                aim = [(vec1-vec2) for (vec1,vec2) in zip(pointFrom,pointTo)]

                # Perpendicular bisector of both points
                centerPoint = [(vec1 + vec2)/2 for (vec1,vec2) in zip(pointTo,pointFrom)]

                # Angle of cutting plane
                planeAngle = cmds.angleBetween( euler=True, v1=[0,0,1], v2=aim )

                # Apply cutting plane and close hole made in geometry
                cmds.polyCut(objectCopy[0], df=True, cutPlaneCenter = centerPoint, cutPlaneRotate = planeAngle)
                cmds.polyCloseBorder(objectCopy[0])
                cmds.polyTriangulate(objectCopy[0])
                cmds.polyQuad(objectCopy[0])

        cmds.xform(objectCopy, cp=True)
        print str(objectCopy)

    cmds.xform(brokenGroup, cp=True)

object = cmds.ls(sl=True)

for obj in object:
    vShatter(obj)