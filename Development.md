# Character

## Controls

### Basic movement

- left mouse on hold
- idle character looks to the mouse
- run to the last clicked(held) mouse position
- run on hold


### Animations/Sprites Renderer

![animacie_mys](https://user-images.githubusercontent.com/72377071/128131939-9c1299e2-f0fb-4990-ba60-8d24d8816a15.jpg)

Animation/sprite renders depending on angle between horizontal vector and mouse vector.

### Smoothing movement

Movement is done by calculating distance between two points (mouse position and player position). Treshold is being used for smoothing movement, since glitch occured when mouse was held too close to the player model, where two animations (idle and movement) constantly swapped between each other in fast intervals.

![movement_smoothing](https://user-images.githubusercontent.com/72377071/128168117-be84ba22-b298-4069-a167-e24e5cc95aab.jpg)

### Dash Effect

![Dash](https://user-images.githubusercontent.com/72377071/128591123-5d39874a-7e22-419a-9192-acdcc7097bb5.jpg)

# World

## World terrain generation.

### Perlin Noise
Method to calculate noise.

### Whittakers diagram approach to create biomes
Combination of multiple steps of Perlin Noise (or another similar noise function), each with a different frequency and amplitude.

Procedural generation of terrain is done by creating **three** generated maps. World is also divided into **chunks** (32x32) for performance and further work with NPCS and mobs.
World consist of **biomes** which are being determind by 3 map types. **Heat** map indicating temperatures, **Precipitation** map indicationg moisture and **Height** map showing elevation. 

### Chunks

Are being held in dictionary. Key is first pixel(left bottom) and WroldChunk class is value.
![chunks](https://user-images.githubusercontent.com/72377071/133770797-4ebfa2f6-3db4-4a5a-9c38-1826c6fa6840.jpg)


### Height Map

Firstly creates **Perlin Noise** values, which are altered by lacunarity, persistance seed etc. Perlin noise works with offset, so to alter it you have to move it, add to it multiply it etc. After that output is send to function which calculates factors, and distance to edges which results in generating one big island being generated and surrounded by ocean.
![heightmap](https://user-images.githubusercontent.com/72377071/133771420-5605fd4f-dce8-4230-aee0-42a9855e3290.png)

### Precipitation Map

Is also created with perlin noise and modified with lacunarity persistance seeds and scales etc.
![mopisture](https://user-images.githubusercontent.com/72377071/133771458-63067ee3-cb6e-4b27-a1b2-b7730131ca97.png)

### Heat Map

Uses height map as base. Generated heat for each tile is affected by height of tile and latitude (distance from equator). The higher tiles are the lower temperature is as well as the higher latitude is the lower temperature is. This results in earth-like climate (hot in the middle and cold in the poles)
heat map + height map
![nvfJH](https://user-images.githubusercontent.com/72377071/133771960-2f351e33-13ea-4401-ae65-ff3f6dc7c727.png)

### Results

![map_example1](https://user-images.githubusercontent.com/72377071/133772021-5e0b3360-0f80-4272-8469-ccdf4ce3a8ba.jpg)

### Performance Update
Each chunk (32x32) is represented by mesh. Within each mesh there are 32 x 32 quads that represents 1 tile. Tile details are held in **TDTile** structure. This increases performance a lot reducing number of gameobject being rendered significantly. 

### Biomes detection
Each tile is represented by **TDTile** structure. In the first cycle of map creation, quads within chunks area created, and each tile recieve (based heat, moisture and height values) biome. This biome is TDTile structure. Next cycle through the map assigns texture based on previously recieved biome type. Each tile also hold information about it's neighbourhood(left, topleft, right,, topright, ... tiles pointers). This is usefull for decetion biome borders.
Each tile is assigned TDTile class, holding intel specifically about this tile. Each tile has pointer on it's neighbour in 8 directions. This is used to render borders of biomes and hills

#### Biome borders
*Flood filling* -like technique is used for detection of biome changes. Specific textures are used on borders of biomes

##### Before determining edging tiles
![edgeTiles_before](https://user-images.githubusercontent.com/72377071/135444988-3a01b13b-3b1f-4c99-b292-9f74cb0ebeb4.jpg)

##### After determining edging tiles
![edgeTiles_after](https://user-images.githubusercontent.com/72377071/135444993-62e02576-e10d-40bb-ab9c-d15d9b95d28d.jpg)

### Hills generation
Generating of world also creates gives every single tile **z-index**. This is considered to be pseudo 3rd space coordinate. All it does is simply determining on what level of height is object supposed to be. there are 3 zindex levels currently. When these differ in neighbourhood, hill cliffs are generated. 

### Stairs
Staircase is placed on the first suitable place. Next staircase position must be within treshold (multiple connected stair tiles), or next position is calculated by this formula: **NextStaircase = LastPlaced + Radius**;

### Key objects
When assigning biome type to the tile, each tile pointer is passed into the corresponding biome array. These arrays are shuffled, and from each biome array is randomly picked tile that meets condition to place key object into it. (minimum radius is dynamically calculated, based on map dimensions)

### Trees spawning
Trees are spaawned using perlin noise with high base scale. Values are then filtered, if value higher than thrashhold was generated, save 1 into map, 0 otherwise. 1 means Tree can be spawned on this location 0 oterwise. When rendering trees, each one checks it's surroundings, and only are spawned if specific criteria are matched(such as no other tree is in minimal radius).

### Object pooling
This method is used quite often in thesis. For chunk loading, object rendering etc. Basically reuses old components that are initialized when the game starts, instad of instantiating and destroying constantly. This is much more efficient. ObjectPool is represented by simple array of gameobjects. When awaken, desired number of objects in pool is instantiated and set inactive. Then whenever new object is required, GetPooledObject is called instad of instantiating new one. When the object is not needed later, it's activation state is set to false.(recycling)

### Chunk Loading
Chunks are loaded in ChunkLoader. Method from tihs class (LoadChunks) is perioadically called within Map's update. According to player's position, chunks within radius are loaded or unloaded from the scene. Chunks are created in the beggining by ObjectPool class, so inactive chunks are actually recycled to render new ones.

### Trees
Tree map is rendered in ChunkGenerator. Perlin noise is used to create basic map, then loop through this matrix is required to determine highest value of the neighbour radius. When highes value is picked, tree is plantet to tree map for each chunk. This is later used to know where to place trees without necessity to save it's location.

### Objects
When first visiting chunk, objects are randomly scattered throughout chunk and saved into WorldChunk class alongisde other intel. This intel is then saved to JSON file.

### Save / Load system
For each world seed subfolder is created to hold appropriate files. Player's information is saved in Player.JSON file whenever player quit's game. Chunks (trees, textures, objects, ..) are saved only **once** when chunk is visited for the first time. After that only loading from JSON is happening. Each chunk has it's own JSON representation named with it's key position(bottom left of the chunk)

### Key objects ( chests )

Shuffles biome tiles for random loop order. min_distance is calculated from map dimensions. Random tiles from each biome are being picked to place key object in them. If tile does not meet requirements, it is skipped and next one is being processed. This process is done only when mape is being generated for the first time. Selected coordinates are saved into JSON file, from which they are later recieved.

### Valkability of edge tiles(mountains)
When player stands on tile that is cosidered to be hillEdge tile, proper method is started. This method either determines walkable offset, or if tile is corner tile, calculating diagonal and dividing quad into triangles is done for correct walkability for tile.

# Enemies
### Pathfinding
A* algorithm is used to find shortest path. Characters/PathFinding.cs. A* ignores path between two walls connected only through the corner.

### Spawning
Each rendered chunk has 3 ObjectPools attached. One of them is entity object pool handling spawning creatures within rendered chunks. EntitySpawner within ChunkCreator is used to spawn in random positions near player. These positions are checked by isSpawnable function.
