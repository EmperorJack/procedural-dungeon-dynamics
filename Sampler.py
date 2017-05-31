import maya.cmds as cmds
import maya.mel as mel
import random
import itertools as itools
import numpy as np
import sys

# TO DO:
# - Get angle of vertex normals to surrounding face normals
# - How to find best????



class VertexSampler:
    def __init__(self, object):
        self.object = object
        cmds.select(object + ".vtx[*]", r=True)
        vertPosTemp = cmds.xform(object + '.vtx[*]', q=True, ws=True, t=True)
        self.vertexPositions = zip(*[iter(vertPosTemp)] * 3)
        self.vertexNames = self.getVertexNames(object)

    def getVertexNames(self,object):
        cmds.select(object)
        mel.eval("PolySelectConvert 3")
        vertList = cmds.ls(sl=1, fl=1)
        cmds.select(cl=True)
        return vertList

    def randomSamples(self,numberOfSamples):
        return random.sample(set(self.vertexPositions), numberOfSamples)

    def randomPercent(self,samplePercent):
        numberOfSamples = len(self.vertices) * (samplePercent / 100.0)
        return self.randomAmount(int(round(numberOfSamples)))




