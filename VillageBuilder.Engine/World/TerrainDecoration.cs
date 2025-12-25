using System;

namespace VillageBuilder.Engine.World
{
    /// <summary>
    /// Simple color structure for terrain decorations (no Raylib dependency in Engine)
    /// </summary>
    public struct DecorationColor
    {
        public byte R, G, B, A;

        public DecorationColor(byte r, byte g, byte b, byte a = 255)
        {
            R = r; G = g; B = b; A = a;
        }
    }

    /// <summary>
    /// Represents environmental decorations and features on terrain tiles.
    /// These add visual variety and life to the world without affecting gameplay.
    /// </summary>
    public class TerrainDecoration
    {
        public DecorationType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        // Animation properties
        public float AnimationPhase { get; set; }  // 0.0 to 1.0, for cyclic animations
        public float AnimationSpeed { get; set; }  // How fast it animates

        // Variation
        public int VariantIndex { get; set; }  // Which visual variant to use

        // ASCII-only mode (set to true if font doesn't support emojis)
        public static bool UseAsciiOnly { get; set; } = false;

        public TerrainDecoration(DecorationType type, int x, int y, int variantIndex = 0)
        {
            Type = type;
            X = x;
            Y = y;
            VariantIndex = variantIndex;
            AnimationPhase = (float)Random.Shared.NextDouble();
            AnimationSpeed = 0.5f + (float)Random.Shared.NextDouble() * 0.5f;
        }

        /// <summary>
        /// Update animation state
        /// </summary>
        public void UpdateAnimation(float deltaTime)
        {
            AnimationPhase += AnimationSpeed * deltaTime;
            if (AnimationPhase > 1.0f)
            {
                AnimationPhase -= 1.0f;
            }
        }

        /// <summary>
        /// Get the glyph to render for this decoration
        /// </summary>
        public string GetGlyph()
        {
            // ASCII-only mode: skip emojis, use safe symbols only
            if (UseAsciiOnly)
            {
                return Type switch
                {
                    DecorationType.TreeOak => "?",
                    DecorationType.TreePine => "?",
                    DecorationType.TreeDead => "†",
                    DecorationType.BushRegular => "?",
                    DecorationType.BushBerry => "?",
                    DecorationType.BushFlowering => "?",
                    DecorationType.FlowerWild => "*",
                    DecorationType.FlowerRare => "?",
                    DecorationType.GrassTuft => VariantIndex switch
                    {
                        0 => "\"",
                        1 => "'",
                        2 => ",",
                        _ => "."
                    },
                    DecorationType.TallGrass => "\"",
                    DecorationType.Fern => "~",
                    DecorationType.Reeds => "|",
                    DecorationType.RockBoulder => "?",
                    DecorationType.RockPebble => "·",
                    DecorationType.StumpOld => "o",
                    DecorationType.LogFallen => "?",
                    DecorationType.Mushroom => "?",
                    DecorationType.BirdFlying => "v",
                    DecorationType.BirdPerched => "^",
                    DecorationType.Butterfly => "*",
                            DecorationType.RabbitSmall => "r",
                            DecorationType.DeerGrazing => "d",
                            DecorationType.FishInWater => "f",
                            DecorationType.FoxHunting => "x",
                            DecorationType.WolfPack => "w",
                            DecorationType.BearGrizzly => "B",
                            DecorationType.BoarWild => "b",
                            _ => "·"
                        };
                    }

            // Full Unicode/Emoji mode
            return Type switch
            {
                // Trees - multiple variants
                DecorationType.TreeOak => VariantIndex switch
                {
                    0 => "??",
                    1 => "??",
                    _ => "?"
                },
                DecorationType.TreePine => VariantIndex switch
                {
                    0 => "??",
                    1 => "??",
                    _ => "?"
                },
                DecorationType.TreeDead => "??",

                // Bushes
                DecorationType.BushRegular => VariantIndex switch
                {
                    0 => "?",
                    1 => "?",
                    _ => "?"
                },
                DecorationType.BushBerry => "??",
                DecorationType.BushFlowering => "??",

                // Flowers
                DecorationType.FlowerWild => VariantIndex switch
                {
                    0 => "?",
                    1 => "?",
                    2 => "?",
                    _ => "?"
                },
                DecorationType.FlowerRare => VariantIndex switch
                {
                    0 => "??",
                    1 => "??",
                    _ => "??"
                },

                // Grass/Plants
                DecorationType.GrassTuft => VariantIndex switch
                {
                    0 => "\"",
                    1 => "\"",
                    2 => "'",
                    _ => ","
                },
                DecorationType.TallGrass => "??",
                DecorationType.Fern => "??",
                DecorationType.Reeds => "|",

                // Rocks/Natural Features
                DecorationType.RockBoulder => VariantIndex switch
                {
                    0 => "?",
                    1 => "?",
                    _ => "?"
                },
                DecorationType.RockPebble => "·",
                DecorationType.StumpOld => "?",
                DecorationType.LogFallen => "?",
                DecorationType.Mushroom => VariantIndex switch
                {
                    0 => "??",
                    1 => "?",
                    _ => "?"
                },

                // Wildlife (visual only)
                DecorationType.BirdFlying => VariantIndex switch
                {
                    0 => "??",
                    1 => "??",
                    _ => "v"
                },
                DecorationType.BirdPerched => "??",
                DecorationType.Butterfly => VariantIndex switch
                {
                    0 => "??",
                    1 => "?",
                    _ => "*"
                        },
                        DecorationType.RabbitSmall => "??",
                        DecorationType.DeerGrazing => "??",
                        DecorationType.FishInWater => "??",
                        DecorationType.FoxHunting => "??",
                        DecorationType.WolfPack => "??",
                        DecorationType.BearGrizzly => "??",
                        DecorationType.BoarWild => "??",

                        _ => "?"
                    };
                }

        /// <summary>
        /// Get color for this decoration (can vary by season, time, etc.)
        /// Rendering layer will convert DecorationColor to its native color type
        /// </summary>
        public DecorationColor GetColor(int seasonIndex = 0, float timeOfDay = 0.5f)
        {
            // Base color
            var color = Type switch
            {
                // Trees - green, but darker in winter (season 3)
                DecorationType.TreeOak => seasonIndex == 3 
                    ? new DecorationColor(80, 100, 80) 
                    : new DecorationColor(100, 180, 100),
                DecorationType.TreePine => new DecorationColor(60, 140, 80),
                DecorationType.TreeDead => new DecorationColor(120, 100, 80),

                // Bushes - vibrant greens
                DecorationType.BushRegular => new DecorationColor(120, 200, 120),
                DecorationType.BushBerry => new DecorationColor(100, 150, 200),
                DecorationType.BushFlowering => new DecorationColor(255, 150, 200),

                // Flowers - bright colors
                DecorationType.FlowerWild => VariantIndex switch
                {
                    0 => new DecorationColor(255, 200, 100),  // Yellow
                    1 => new DecorationColor(255, 150, 150),  // Pink
                    2 => new DecorationColor(200, 150, 255),  // Purple
                    _ => new DecorationColor(255, 255, 255)   // White
                },
                DecorationType.FlowerRare => new DecorationColor(255, 100, 180),

                // Grass variations
                DecorationType.GrassTuft => new DecorationColor(140, 200, 140),
                DecorationType.TallGrass => new DecorationColor(180, 220, 120),
                DecorationType.Fern => new DecorationColor(100, 180, 100),
                DecorationType.Reeds => new DecorationColor(160, 180, 100),

                // Rocks - gray tones
                DecorationType.RockBoulder => new DecorationColor(140, 140, 150),
                DecorationType.RockPebble => new DecorationColor(160, 160, 165),
                DecorationType.StumpOld => new DecorationColor(100, 80, 60),
                DecorationType.LogFallen => new DecorationColor(120, 90, 70),
                DecorationType.Mushroom => VariantIndex switch
                {
                    0 => new DecorationColor(255, 100, 100),  // Red cap
                    1 => new DecorationColor(200, 150, 100),  // Brown
                    _ => new DecorationColor(220, 220, 180)   // Pale
                },

                // Wildlife - natural colors
                DecorationType.BirdFlying => new DecorationColor(100, 100, 120),
                DecorationType.BirdPerched => new DecorationColor(200, 150, 100),
                DecorationType.Butterfly => new DecorationColor(255, 200, 150),
                DecorationType.RabbitSmall => new DecorationColor(200, 180, 160),
                DecorationType.DeerGrazing => new DecorationColor(150, 120, 90),
                DecorationType.FishInWater => new DecorationColor(150, 200, 220),

                _ => new DecorationColor(255, 255, 255)
            };

            // Apply day/night dimming
            if (timeOfDay < 0.3f || timeOfDay > 0.8f)
            {
                float dimFactor = timeOfDay < 0.3f 
                    ? timeOfDay / 0.3f 
                    : 1.0f - ((timeOfDay - 0.8f) / 0.2f);
                color.R = (byte)(color.R * dimFactor);
                color.G = (byte)(color.G * dimFactor);
                color.B = (byte)(color.B * dimFactor);
            }

            return color;
        }

        /// <summary>
        /// Returns true if this decoration blocks pathfinding
        /// </summary>
        public bool IsBlocking()
        {
            return Type switch
            {
                DecorationType.TreeOak => true,
                DecorationType.TreePine => true,
                DecorationType.TreeDead => true,
                DecorationType.RockBoulder => true,
                _ => false  // Most decorations don't block movement
            };
        }
    }
    
    /// <summary>
    /// Types of terrain decorations
    /// </summary>
    public enum DecorationType
    {
        // Trees
        TreeOak,
        TreePine,
        TreeDead,
        
        // Bushes
        BushRegular,
        BushBerry,
        BushFlowering,
        
        // Flowers
        FlowerWild,
        FlowerRare,
        
        // Grass/Plants
        GrassTuft,
        TallGrass,
        Fern,
        Reeds,
        
        // Rocks/Features
        RockBoulder,
        RockPebble,
        StumpOld,
        LogFallen,
        Mushroom,
        
                                // Wildlife (visual only, moves)
                                BirdFlying,
                                BirdPerched,
                                Butterfly,
                                RabbitSmall,
                                DeerGrazing,
                                FishInWater,
                                // NEW: Predators and other wildlife
                                FoxHunting,
                                WolfPack,
                                BearGrizzly,
                                BoarWild,

                                // NEW: Building sprites (for sprite mode rendering)
                                // Full building icons (centered overview)
                                BuildingHouse,
                                BuildingFarm,
                                BuildingWarehouse,
                                BuildingWorkshop,
                                BuildingMine,
                                BuildingLumberyard,
                                BuildingMarket,
                                BuildingWell,
                                BuildingTownHall,

                                // NEW: Building component sprites (detailed per-tile rendering)
                                // Floors
                                BuildingFloorWood,
                                BuildingFloorStone,
                                BuildingFloorCarpet,
                                BuildingFloorDirt,

                                // Walls
                                BuildingWallBrick,
                                BuildingWallStone,
                                BuildingWallWood,
                                BuildingWallPlaster,

                                // Doors
                                BuildingDoorClosed,
                                BuildingDoorOpen,
                                BuildingDoorLocked,

                                // Windows
                                BuildingWindowDay,
                                BuildingWindowNight,
                                BuildingWindowBroken,

                                // Roofs
                                BuildingRoofTiles,
                                BuildingRoofThatch,
                                BuildingRoofShingles,
                                BuildingRoofWood,

                                // Foundations
                                BuildingFoundationStone,
                                BuildingFoundationWood,

                                // Decorations
                                BuildingChimney,
                                BuildingFence,
                                BuildingSign
                            }
                        }
