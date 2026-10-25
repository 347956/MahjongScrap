using UnityEngine;
using BepInEx;
using LethalLib.Modules;
using BepInEx.Logging;
using System.IO;
using MahjongScrap.Configuration;
using System.Collections.Generic;

namespace MahjongScrap {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class Plugin : BaseUnityPlugin {
        internal static new ManualLogSource Logger = null!;
        internal static PluginConfig BoundConfig { get; private set; } = null!;
        public static AssetBundle? ModAssets;

        private static readonly string[] TileNames = {
            "EastTile", "SouthTile", "WestTile", "NorthTile",
            "GreenDragontile", "RedDragonTile", "WhiteDragonTile",
            "PinzuOneTile", "PinzuTwoTile", "PinzuThreeTile", "PinzuFourTile", "PinzuFiveTile",
            "PinzuSixTile", "PinzuSevenTile", "PinzuEightTile", "PinzuNineTile",
            "SouzuOneTile", "SouzuTwoTile", "SouzuThreeTile", "SouzuFourTile", "SouzuFiveTile",
            "SouzuSixTile", "SouzuSevenTile", "SouzuEightTile", "SouzuNineTile",
            "ManzuOneTile", "ManzuTwoTile", "ManzuThreeTile", "ManzuFourTile", "ManzuFiveTile",
            "ManzuSixTile", "ManzuSevenTile", "ManzuEightTile"
        };

        private static readonly string[] RedTileNames = {
            "ManzuRedFiveTile", "PinzuRedFiveTile", "SouzuRedFiveTile"
        };

        private static readonly string[] PaintingNames = {
            "PaintingPortraitIchihime", "PaintingPortraitJoseph", "PaintingPortraitGroup",
            "PaintingYakuman", "PaintingHalloween", "PaintingWinter"
        };

        private const string HalloweenPaintingName = "PaintingHalloween";
        private const string WinterPaintingName = "PaintingWinter";

        private void Awake() {
            Logger = base.Logger;

            BoundConfig = new PluginConfig(base.Config);

            var bundleName = "mahjongscrapmodassets";
            ModAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), bundleName));
            if (ModAssets == null) {
                Logger.LogError($"Failed to load custom assets.");
                return;
            }

            //Checks if Tiles are enabled > Registers and loads the tiles if enabled
            bool tilesEnabled = BoundConfig.EnableMahjongTiles.Value;
            if (tilesEnabled)
            {
                Logger.LogInfo($"EnableMahjongTiles = [{tilesEnabled}] > enabling Mahjong Tiles");
                RegisterScrap(TileNames, BoundConfig.MahjongTileSpawnWeight.Value);
                Logger.LogInfo("Done loading normal tiles");
                RegisterScrap(RedTileNames, BoundConfig.MahjongRedTileSpawnWeight.Value);
                Logger.LogInfo("Done loading red fives");
            }
            else
            {
                Logger.LogInfo($"EnableMahjongTiles = [{tilesEnabled}] > skipped loading Mahjong Tiles");
            }

            //Checks if Paintings are enabled > Registers and loads the paintings if enabled
            bool paintingsEnabled = BoundConfig.EnableMahjongSoulPaintings.Value;
            if (paintingsEnabled)
            {
                Logger.LogInfo($"EnableMahjongSoulPaintings = [{paintingsEnabled}] > enabling Mahjong Soul Paintings");
                RegisterScrap(PaintingNames, BoundConfig.MahjongSoulPaintingsSpawnWeight.Value);
                Logger.LogInfo("Done loading mahjong soul paintings");
            }
            else
            {
                Logger.LogInfo($"EnableMahjongSoulPaintings = [{paintingsEnabled}] > skipped loading Mahjong Soul Paintings");
            }
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        //Loads the named items from the asset bundle and registers them as scrap. Names that can't be found are logged and skipped.
        private void RegisterScrap(IEnumerable<string> assetNames, int spawnWeight)
        {
            bool enableHalloween = BoundConfig.EnableMahjongSoulHalloweenPaintings.Value;
            bool enableWinter = BoundConfig.EnableMahjongSoulWinterPaintings.Value;
            foreach (Item item in LoadItems(assetNames))
            {
                NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);

                //Seasonal paintings will be loaded but set with a 0 spawnweight. This way they'll still be in game as scrap for save files but not spawnable on moons.
                if (item.name.Equals(HalloweenPaintingName) && !enableHalloween)
                {
                    Logger.LogInfo("Halloween paintings are disabled");
                    Items.RegisterScrap(item, 0, Levels.LevelTypes.All);
                }
                else if (item.name.Equals(WinterPaintingName) && !enableWinter)
                {
                    Logger.LogInfo("Winter paintings are disabled");
                    Items.RegisterScrap(item, 0, Levels.LevelTypes.All);
                }
                else
                {
                    Items.RegisterScrap(item, spawnWeight, Levels.LevelTypes.All);
                }
            }
        }

        private List<Item> LoadItems(IEnumerable<string> assetNames)
        {
            List<Item> items = new List<Item>();
            foreach (string assetName in assetNames)
            {
                Item? item = ModAssets!.LoadAsset<Item>(assetName);
                if (item == null || item.spawnPrefab == null)
                {
                    Logger.LogWarning($"Could not load item [{assetName}] from the asset bundle, skipping it");
                    continue;
                }
                items.Add(item);
            }
            return items;
        }
    }
}
