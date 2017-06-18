

# Procedural Dungeon Dynamics

*By Jack Purvis, Mark Kuggeleijn and Haylem Rayner*

#### Objective
“To create a tool to aid the creation of procedural, dungeon-focused environments that include destructible objects and simulate the flow of realistic crowd movements”


----------


#### Individual Components
Jack: Procedural Dungeon Generation
Mark: Fracturing
Haylem: Crowd Simulation


----------


#### Usage

This Unity project can be opened like any project in Unity. It may require version 5.6 to be at least used. The python fracturing scripts are run inside Maya and have been tested with Maya 2016.

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

In Unity:

**Crowd Simulation**

At runtime:
- Note that a new dungeon must be generated at runtime before the crowd simulation will work.
- Once generated click `Add Group` to add a new group to the simulation
- The `Swap Group` button will swap the active group for adding goals and agents, it will highlight the group colour
- The action button toggles state between `Add Goal` and `Add Agent`
- Use `Default Settings` to reset parameters
- The toggle `Pause` will toggle the simulation affecting agent velocities


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

**Crowd Simulation**

Parameters must be changed at run time, click the `Toggle Crowd Simulation GUI` button and adjust the corresponding sliders:

- `Update Period` sets the frame period for updating the crowd simulation. Decreasing this will decrease performance.
- `Max Density` sets the density cut off above which the velocity field will be dominated by the average velocity of the crowd.
- `Time Weight` the weighted cost moving from cell to cell, decreasing this will decrease the agents preference to arrive at the goal by the fastest route.
- `Distance Weight` the weighted cost moving from cell to cel which is weighted by the velocity between those cell. Increasing this will increase the agents preference to arrive at the goal by the shortest route.
- `Density Exponent` the contribution for each agent to the density of the potential fields, increasing this decreases the contribution of each aget.
- `Avoidance Time Steps` the number of timesteps ahead of each agent along which discomfort is added to the potential field to allow other groups to anticipate their movements. Increasing this will decrease performance.
- `Object Avoidance` the weight which objects add discomfort to the grid, 0 will mean agents do not avoid objects, 3 will increase avoidance.

Note: These parameters affect the shared potential field, and so are not specific to each group. 
----------


#### Attributions

Models:
- www.turbosquid.com (Ancient Vase, Tall Vase, Warrior Statue, Cat Statue, Plinth)
- The remainder were modelled by us.

Textures:
- www.textures.com

Audio:
- www.freesfx.co.uk
- www.freesound.org
- Torch flame -  http://freesound.org/people/stratcat322/sounds/233189/
