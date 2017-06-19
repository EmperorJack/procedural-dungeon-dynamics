

# Procedural Dungeon Dynamics

*By Jack Purvis, Mark Kuggeleijn and Haylem Rayner*

#### Objective
“To create a tool to aid the creation of procedural, dungeon-focused environments that include destructible objects and simulate the flow of realistic crowd movements”


----------


#### Individual Components
Jack: Procedural Dungeon Generation
Mark: Fracturing
Haylem: Crowd Simulation


#### Usage

This Unity project can be opened like any project in Unity. It may require version 5.6 to be at least used. The python fracturing scripts are run inside Maya and have been tested with Maya 2016.

----------


**Dungeon Generation**

To generate dungeons from within the Unity editor:

- Open the scene `dungeonTest`.
- Click `Window` then `Procedural Pipeline` to open the custom window.
- Drag the `ProceduralPipeline` game object from the hierarchy into the slot at the top of the window.
- Press the `Perform` button to generate a new dungeon.

To generate dungeons at runtime:

- Run the game.
- Click the `Generate Dungeon` button or press the `space` key to generate a new dungeon.



**Fracturing**

In Maya:
- The fracturing script requires an installation of NumPy; http://www.numpy.org/
- Copy `BowyerWatson.py` and `Sampler.py` into a location reachable by the Maya Python path
- Import geometry.  Make sure the object has no non-standard ASCII characters in the name.
- Manually select the geometry
- In the Script Editor, `import BowyerWatson`
- The following commands can then be run:
  - `BowyerWatson.randomShatter(numberOfSamples, debug)` : selects numberOfSamples points at random from within the object's bounding box. The debug parameter is a boolean - if True, generates geometry displaying the tessellation.  The tessellation geometry will not have correct normals, it is purely for visual debugging.  This is the same across all commands.
  - `BowyerWatson.unweightedShatter(numberOfSamples, debug)` : selects numberOfSamples vertices of the mesh to use as points for the            tessellation.
  - `BowyerWatson.unweightedShatterPercent(samplePercent, debug)` : selects samplePercent of vertices to use as tessellation points.
  - `BowyerWatson.weightedShatter(numberOfSamples, samplePercent, debug)` : selects samplePercent of vertices to use as tessellation        points, then selects numberOfSamples of the resulting tetrahedra and uses their centroids as the points for a new tessellation.
- Export the geometry, remembering to keep the broken geometry grouped.  The file names must end in `_whole` for the original geometry, and `_broken` for the shattered group.
 
In Unity:
- Import the `_whole` and `_broken` geometry.  There should be output in the log stating that attributes have been set on them.
- Create an empty GameObject in the scene.  Name it something ending in `_breakable`.
- Parent instances of \_whole and \_broken to it by dragging them into the explorer.
- Give the \_whole geometry a new `Physical Material`, or use one of the ones in `Assets/Materials`
- Attach the `WholeObject` script in `Scripts/Shattered/` to the \_whole instance.
- Drag the instance of \_broken into the `Broken Geo` field (in the attached WholeObject script).
- Optional: Set the `Collision Threshold` (default is 1). I have used 1 for brittle objects such as plates, 2 for slightly sturdier objects like crates.
- Optional: Set the `Shatter Sound`. 
- Press `Initialise Broken Geo` to copy physical and visual materials from \_whole to \_broken, or make sure `Initialize On Start` is checked to copy them across at runtime.
- Drag the \_breakable instance into `Prefabs/Breakable/`.  This makes it a unity prefab.
- The prefab can then be assigned to the lists in `DungeonObjectPlacer`, and be placed in the scene when a new dungeon is generated.


**Crowd Simulation**

At runtime:
- Note that a new dungeon must be generated at runtime before the crowd simulation will work.
- Once generated click `Add Group` to add a new group to the simulation.
- The `Swap Group` button will swap the active group for adding goals and agents, it will highlight the group colour.
- The action button toggles state between `Add Goal` and `Add Agent`.
- Use `Default Settings` to reset parameters.
- The toggle `Pause` will toggle the simulation affecting agent velocities.
- The toggle `Revive` will toggle whether agents are removed from their active group and added to a random other group when they reach their goal.


**Camera Controls**

- Use the `W` and `S` keys to move forward and backward.
- Use the `A` and `D` keys to move left and right.
- Use the `Q` and `E` keys to turn left and right.
- Use the `R` and `F` keys to move up and down.
- Use the `Z` key to use a wide camera angle.
- Use the `X` key to use a top down camera angle.

**General Controls**

- Use the `Escape` key to exit the game.
- Use the `L` key to toggle the GUI.

----------


#### Parameters

**Dungeon Generation**

To change dungeon parameters from within the Unity editor:

- Select the `DungeonGenerator` game object in the hierarchy
- Useful *Dungeon Layout Generator* parameters:
  - `Grid Size` - size of the grid width and height
  - `Minimum Room Size` - minimum width and height of a room
  - `Room Buffer` - minimum amount of grid cells between areas
  - `Min Room Width Height Ratio` - minimum width - height ratio, less is wider rooms, more is taller rooms
  - `Max Room Width Height Ratio` - maximum width - height ratio, less is wider rooms, more is taller rooms
  - `Max Depth` - maximum node depth of the BSP tree
  - `Add Loops From Level` - minimum layer from the bottom (i.e: maxDepth - currentDepth) to add loops to
  - `Add Loops To Level` - maximum layer from the bottom (i.e: maxDepth - currentDepth) to add loops to
  - `Loop Spawn Chance` - chance of a loop spawning (0 - 1)
  - `Allow Loops Between Two Rooms` - whether or not two areas that are already connected can be connected again
- Useful *Dungeon Asset Populator* parameters:
  - `* Prefab` - Unity prefab to instantiate as that asset
  - `Torch Spacing` - number of grid cells to space between torches
- Useful *Dungeon Anchor Generator* parameters:
  - `Center Spacing` - number of grid cells between objects placed in the center of rooms
  - `Edge Spacing` - number of grid cells between objects placed around the edges of rooms
  - `Edge Buffer` - number of grid cells between the edge and center objects
- Useful *Dungeon Object Placer* parameters:
  - The following parameters apply to each of the anchor types (Edge, Corner and Center) separately
  - `* Object Prefabs` - list of possible Unity prefabs to instantiate
  - `* Translation Offset` - maximum translation offset that is applied to an instantiated object
  - `* Rotation Offset` - maximum rotation offset that is applied to an instantiated object
  - `* Spawn Chance` - chance of an object spawning (0 - 1)

To change dungeon parameters at runtime:

- Ensure the Dungeon Generation GUI is shown by clicking the `Toggle Dungeon Generation GUI` button.
- Configure the available parameters as listed above.

**Fracturing**

In Maya:
- In all scripts, the `debug` parameter is a boolean - if True, generates geometry displaying the tessellation.  The tessellation geometry will not have correct normals, it is purely for visual debugging.
- In `randomShatter` and `unweightedShatter`, `numberOfSamples` is the amount of sample points to be inserted into the tessellation.  The more points, the longer the script will take to run, and the more the mesh will be subdivided
- In `unweightedShatterPercent`, `samplePercent` is the percentage of mesh vertices to use as tessellation points.
- In `weightedShatter`, `samplePercent` is the percentage of mesh vertices to use as initial tessellation points.  These will not be used to subdivide the mesh, but will be used to provide further points to be sampled.
- In `weightedShatter`, `numberOfSamples` is the amount of tetrahedra centroids from the first tessellation to sample as points for mesh subdivision.

In Unity (`WholeGeo`):
- `BrokenGeo` : The imported geometry group representing the shattered object
- `Collision Threshold` : The velocity threshold a collision must reach or pass to trigger the script
- `Shatter Sound` : Sound to be played when script is triggered.  Will have a small amount of random pitch offset applied at runtime.
- `Initialize On Start` : If the physical properties of the whole geometry have not been copied over, attempt to do so at runtime. 

**Crowd Simulation**

Parameters must be changed at run time, click the `Toggle Crowd Simulation GUI` button and adjust the corresponding sliders, these parameters affect the shared potential field, and so are not specific to each group. :

- `Update Period` sets the frame period for updating the crowd simulation. Decreasing this will decrease performance.
- `Max Density` sets the density cut off above which the velocity field will be dominated by the average velocity of the crowd.
- `Time Weight` the weighted cost moving from cell to cell, decreasing this will decrease the agents preference to arrive at the goal by the fastest route.
- `Distance Weight` the weighted cost moving from cell to cel which is weighted by the velocity between those cell. Increasing this will increase the agents preference to arrive at the goal by the shortest route.
- `Density Exponent` the contribution for each agent to the density of the potential fields, increasing this decreases the contribution of each aget.
- `Avoidance Time Steps` the number of timesteps ahead of each agent along which discomfort is added to the potential field to allow other groups to anticipate their movements. Increasing this will decrease performance.
- `Object Avoidance` the weight which objects add discomfort to the grid, 0 will mean agents do not avoid objects, 3 will increase avoidance.

----------


#### Attributions

Models:
- www.turbosquid.com (Ancient Vase, Tall Vase, Warrior Statue, Cat Statue, Plinth)
- The remainder were created by us.

Textures:
- www.textures.com

Audio:
- www.freesfx.co.uk
- www.freesound.org
- Torch flame - http://freesound.org/people/stratcat322/sounds/233189/

Sprites:
- Torch flame - https://opengameart.org
