# Preamble
For the sake of reviewing, all modified game code will be labeled like this :

```csharp
// mod
// new code ...
// original
// old code ...
```


## Pooling

- Added "BOOL_TILES_POOLING" to [RemoteConfig](Assets/3_Scripts/Utils/RemoteConfig.cs).
- Project uses "Theme" based structure, so I put my pooling system [here](Assets/3_Scripts/Tower/TilePool.cs).
- The tile pool prefab was placed in the main scene just under the "Tower" object.
- I used an extension method to place the `ReturnToPool` method directly in the `TowerTile` class, this makes it more coherent.
- TilePool has 2 public methods :

    `TowerTile GetTile(bool isNormal, int specialIndex, Action<TowerTile> Configure)` : Needs a description of which tile to provide. It will apply the provided configuration once the tile is selected from the corresponding pool.

    `void ReturnTile(TowerTile tile)` : Will deactivate the tile and make it available to be selected by `GetTile`.
- The configuration method injected in `GetTile` makes it easy to provide extra configuration if needed, depending on how the project spawned tiles before. I chose not to put all the configuration in it for the sake of keeping code untouched if it didn't need to be modified.