# Preamble
For the sake of reviewing, all modified game code will be labeled like this :

```csharp
// mod
// new code ...
// original
// old code ...
```

The folder "Documentation~" has been added to the project for the sake of providing screenshots in this document. This folder will be ignored by the Unity editor. Please view this file with a previewer (gitHub has one by default) to have the full experience.

Some objects were made into prefabs for multi-scene possiblity.

## Optimizations

### Material batching

After inspecting the project materials in the folder [Materials](Assets/1_Graphics/Materials/) I identified several materials which were used on multiple objects in the scene.

To do this, I used the "_Find references in scene_" context option when clicking on a material asset.

- The [Water](Assets/1_Graphics/Materials/Water.mat) material is used in 81 scene objects.

- The [Cylinder](Assets/1_Graphics/Materials/Cylinder.mat) material is used in 184 scene objects.

Enabling GPU instantiation on both of those materials lowered batches from 31 to 25 and saved 184 (visible in the Statistics popup in the Game view) thus reducing draw calls. A minor increase in FPS can be seen (3~5 FPS).

![Before](Assets/Documentation~/BeforeBatching.png)
![After](Assets/Documentation~/AfterBatching.png)

### Canvas splitting

All the UI objects of the game are present in the main game scene under **GameManager/UI** and are under one **Canvas** component. Many of those UI objects are animated, which will prompt constant draw calls to the GPU to refresh the modified UI.

Unity draws UI using the **Canvas** component to group together UI which needs to be refreshed. Since there is only one **Canvas** component in the whole UI hierarchy, the whole UI will get **refreshed everytime one element changes**.

To optimize this I **added a Canvas component on each main UI object**. I examined the animation clips from the [GameManagerAnimator](Assets/1_Graphics/Animation/GameManager/GameManagerAnimator.controller) controller to see which UI object was animated.

More details of how Unity manages UI and optimization [here](https://unity.com/how-to/unity-ui-optimization-tips).

## Pooling

- Added `BOOL_TILES_POOLING` to [RemoteConfig](Assets/3_Scripts/Utils/RemoteConfig.cs).

- Project uses "Theme" based structure, so I put my pooling system [here](Assets/3_Scripts/Tower/TilePool.cs).

- The tile pool prefab was placed in the main scene just under the "**Tower**" object.

- I used an extension method to place the `ReturnToPool` method directly in the `TowerTile` class, this makes it more coherent.

- `TilePool` has 2 public methods :

    `TowerTile GetTile(bool isNormal, int specialIndex, Action<TowerTile> Configure)` : Needs a description of which tile to provide. It will apply the provided configuration once the tile is selected from the corresponding pool.

    `void ReturnTile(TowerTile tile)` : Will deactivate the tile and make it available to be selected by `GetTile`.

- The configuration method injected in `GetTile` makes it easy to provide extra configuration if needed, depending on how the project spawned tiles before. I chose not to put all the configuration in it for the sake of **keeping code untouched if it didn't need to be modified.**

## Missions

Missions are picked from a list on game start.

The 4 types of missions I made are :

- `Reach combo` : Reach a certain combo count.

- `Win games` : Win a certain number of games.

- `Trigger explosions` : Trigger a certain number of explosions.

- `Win in time` : Win before the end of the timer.

Each mission type is described in a [MissionData](Assets/3_Scripts/MissionData.cs) scriptable object which also contains the difficulty levels of this mission type and the associated reward.

Once the objective and difficulty level are picked for a mission, a new object [Mission](Assets/3_Scripts/Mission.cs) is created, which is a lightweight representation of a mission's data.

It should be simple to add a new `Objective` or `Difficulty`.
All missions provide the player with currency. There is only one type of currency for now but the [CurrencyManager](Assets/3_Scripts/CurrencyManager.cs) has support for extending the types of currencies available.

Completed missions are replaced at the end of a game. Some mission's progress will be reset between games.

## Extras

The folder [ThirdParty](Assets/ThirdParty/) has been added to the project to store the assets I've used for making the Mission UI. The UI assets were provided by Kenney [Kenney](https://www.kenney.nl/assets/ui-pack).