using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace RGBHighlight
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(GUID, MOD_NAME, MOD_VERSION)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [R2API.Utils.R2APISubmoduleDependency(nameof(CommandHelper))]
    public class RGBHighlight : BaseUnityPlugin
    {
        public const string GUID = "com.Lunzir.RGBHighlight", MOD_NAME = "RGBHighlight", MOD_VERSION = "1.1.0";
        static Dictionary<string, HighLightStruct> Instance;
        public void Awake()
        {
            ModConfig.InitConfig(Config);
            if (ModConfig.EnableMod.Value)
            {
                gameObject.hideFlags |= HideFlags.HideAndDontSave;

                InitHighlightData();

                On.RoR2.Run.Start += Run_Start;
                On.RoR2.SceneDirector.Start += SceneDirector_Start;

                On.RoR2.Highlight.GetColor += Highlight_GetColor;
                On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
                On.RoR2.MultiShopController.OnPurchase += MultiShopController_OnPurchase;
                On.RoR2.BarrelInteraction.OnInteractionBegin += BarrelInteraction_OnInteractionBegin;

                //On.RoR2.RouletteChestController.Opened.OnEnter += RouletteChest_Opened_OnEnter;
                On.EntityStates.Barrel.Opened.OnEnter += Barrel_Opened_OnEnter;
                On.EntityStates.Barrel.ActivateFan.OnEnter += ActivateFan_OnEnter;
                On.RoR2.ShrineBloodBehavior.FixedUpdate += ShrineBloodBehavior_FixedUpdate;
                On.RoR2.ShrineBossBehavior.FixedUpdate += ShrineBossBehavior_FixedUpdate;
                On.RoR2.ShrineChanceBehavior.FixedUpdate += ShrineChanceBehavior_FixedUpdate;
                On.RoR2.ShrineCombatBehavior.FixedUpdate += ShrineCombatBehavior_FixedUpdate;
                On.RoR2.ShrineHealingBehavior.FixedUpdate += ShrineHealingBehavior_FixedUpdate;
                On.RoR2.ShrineRestackBehavior.FixedUpdate += ShrineRestackBehavior_FixedUpdate;
                CommandHelper.AddToConsoleWhenReady();
            }
        }
        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);
            UpdateHighlight();
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            UpdateHighlight();
        }
        private static void InitHighlightData()
        {
            if (Instance != null)
            {
                Instance.Clear();
            }
            Instance = new Dictionary<string, HighLightStruct>()
            {
                {"barrel1", new HighLightStruct("Prefabs/NetworkedObjects/Chest/Barrel1", ModConfig.Barrel1Color.Value)},
                {"casinochest", new HighLightStruct("Prefabs/NetworkedObjects/Chest/CasinoChest", ModConfig.CasinoChestColor.Value, false, true, 10f)},
                {"categorychestdamage", new HighLightStruct("Prefabs/NetworkedObjects/Chest/CategoryChestDamage", ModConfig.CategoryChestDamageColor.Value)},
                {"categorychesthealing", new HighLightStruct("Prefabs/NetworkedObjects/Chest/CategoryChestHealing", ModConfig.CategoryChestHealingColor.Value)},
                {"categorychestutility", new HighLightStruct("Prefabs/NetworkedObjects/Chest/CategoryChestUtility", ModConfig.CategoryChestUtilityColor.Value)},
                {"chest1", new HighLightStruct("Prefabs/NetworkedObjects/Chest/Chest1", ModConfig.Chest1Color.Value)},
                {"chest1stealthedvariant", new HighLightStruct("Prefabs/NetworkedObjects/Chest/Chest1StealthedVariant", ModConfig.Chest1StealthedVariantColor.Value)},
                {"chest2", new HighLightStruct("Prefabs/NetworkedObjects/Chest/Chest2", ModConfig.Chest2Color.Value)},
                {"drone1broken", new HighLightStruct("Prefabs/NetworkedObjects/BrokenDrones/Drone1Broken", ModConfig.Drone1BrokenColor.Value, false)},
                {"drone2broken", new HighLightStruct("Prefabs/NetworkedObjects/BrokenDrones/Drone2Broken", ModConfig.Drone2BrokenColor.Value, false)},
                {"duplicatorlarge", new HighLightStruct("Prefabs/NetworkedObjects/Chest/DuplicatorLarge", ModConfig.DuplicatorLargeColor.Value, true, false)},
                {"duplicatormilitary", new HighLightStruct("Prefabs/NetworkedObjects/Chest/DuplicatorMilitary", ModConfig.DuplicatorMilitaryColor.Value, true, false)},
                {"duplicatorwild", new HighLightStruct("Prefabs/NetworkedObjects/Chest/DuplicatorWild", ModConfig.DuplicatorWildColor.Value,true, false)},
                {"duplicatorblue", new HighLightStruct("Prefabs/NetworkedObjects/Chest/Duplicator", HighlightColor.LightBlue, true, false)},
                {"duplicatorpurple", new HighLightStruct("Prefabs/NetworkedObjects/Chest/Duplicator", HighlightColor.Pink, true, false)},
                {"duplicator", new HighLightStruct("Prefabs/NetworkedObjects/Chest/Duplicator", ModConfig.DuplicatorColor.Value,true, false)},
                {"emergencydronebroken", new HighLightStruct("Prefabs/NetworkedObjects/BrokenDrones/EmergencyDroneBroken", ModConfig.EmergencyDroneBrokenColor.Value, false)},
                {"equipmentbarrel", new HighLightStruct("Prefabs/NetworkedObjects/Chest/EquipmentBarrel", ModConfig.EquipmentBarrelColor.Value, false)},
                {"equipmentdronebroken", new HighLightStruct("Prefabs/NetworkedObjects/BrokenDrones/EquipmentDroneBroken", ModConfig.EquipmentDroneBrokenColor.Value, false)},
                {"flamedronebroken", new HighLightStruct("Prefabs/NetworkedObjects/BrokenDrones/FlameDroneBroken", ModConfig.FlameDroneBrokenColor.Value, false)},
                {"freechestterminalshipping", new HighLightStruct("Prefabs/NetworkedObjects/Chest/FreeChestTerminalShippingDrone", ModConfig.FreeChestMultiShopColor.Value)},
                {"goldchest", new HighLightStruct("Prefabs/NetworkedObjects/Chest/GoldChest", ModConfig.GoldChestColor.Value)},
                {"goldshoresbeacon", new HighLightStruct("Prefabs/NetworkedObjects/GoldshoresBeacon", ModConfig.GoldshoresBeaconColor.Value, true, false)},
                {"humanfan", new HighLightStruct("Prefabs/NetworkedObjects/HumanFan", ModConfig.HumanFanColor.Value, true, false)},
                {"lockbox", new HighLightStruct("Prefabs/NetworkedObjects/Lockbox", ModConfig.LockboxColor.Value)},
                {"lockboxvoid", new HighLightStruct("Prefabs/NetworkedObjects/LockboxVoid", ModConfig.LockboxVoidColor.Value)},
                //{"logpickup", new ColorStruct("Prefabs/NetworkedObjects/LogPickup", ModConfig.LogPickupColor.Value)},
                {"logpickup", new HighLightStruct("Prefabs/NetworkedObjects/LogPickup2", ModConfig.LogPickup2Color.Value, false)},
                {"greentored", new HighLightStruct("Prefabs/NetworkedObjects/LunarCauldron, GreenToRed Variant", ModConfig.LunarCauldronGreenColor.Value, true, false)},
                {"redtowhite", new HighLightStruct("Prefabs/NetworkedObjects/LunarCauldron, RedToWhite Variant", ModConfig.LunarCauldronRedColor.Value, true, false)},
                {"whitetogreen", new HighLightStruct("Prefabs/NetworkedObjects/LunarCauldron, WhiteToGreen", ModConfig.LunarCauldronWhiteColor.Value, true, false)},
                {"lunarchest", new HighLightStruct("Prefabs/NetworkedObjects/Chest/LunarChest", ModConfig.LunarChestColor.Value)},
                {"lunarshopterminal", new HighLightStruct("Prefabs/NetworkedObjects/Chest/LunarShopTerminal", ModConfig.LunarShopTerminalColor.Value)},
                {"megadronebroken", new HighLightStruct("Prefabs/NetworkedObjects/BrokenDrones/MegaDroneBroken", ModConfig.MegaDroneBrokenColor.Value, false, false)},
                {"newtstatue", new HighLightStruct("Prefabs/NetworkedObjects/NewtStatue", ModConfig.MegaDroneBrokenColor.Value, false, false)},
                {"missiledronebroken", new HighLightStruct("Prefabs/NetworkedObjects/BrokenDrones/MissileDroneBroken", ModConfig.MissileDroneBrokenColor.Value, false, false)},
                {"multishopequipmentterminal", new HighLightStruct("Prefabs/NetworkedObjects/Chest/MultiShopEquipmentTerminal", ModConfig.MultiShopEquipmentTerminalColor.Value, false, false)},
                {"multishoplargeterminal", new HighLightStruct("Prefabs/NetworkedObjects/Chest/MultiShopLargeTerminal", ModConfig.MultiShopLargeTerminalColor.Value, false, false)},
                {"multishopterminal", new HighLightStruct("Prefabs/NetworkedObjects/Chest/MultiShopTerminal", ModConfig.MultiShopTerminalColor.Value, false, false)},
                {"radartower", new HighLightStruct("Prefabs/NetworkedObjects/RadarTower", ModConfig.RadarTowerColor.Value)},
                {"scavbackpack", new HighLightStruct("Prefabs/NetworkedObjects/ScavBackpack", ModConfig.ScavBackpackColor.Value)},
                {"scavlunarbackpack", new HighLightStruct("Prefabs/NetworkedObjects/ScavLunarBackpack", ModConfig.ScavLunarBackpackColor.Value)},
                {"scrapper", new HighLightStruct("Prefabs/NetworkedObjects/Chest/Scrapper", ModConfig.ScrapperColor.Value, false, false)},
                {"shrineblood", new HighLightStruct("Prefabs/NetworkedObjects/Shrines/ShrineBlood", ModConfig.ShrineBloodColor.Value, true, false)},
                {"shrineboss", new HighLightStruct("Prefabs/NetworkedObjects/Shrines/ShrineBoss", ModConfig.ShrineBossColor.Value)},
                {"shrinechance", new HighLightStruct("Prefabs/NetworkedObjects/Shrines/ShrineChance", ModConfig.ShrineChanceColor.Value, true, false)},
                {"shrinecleanse", new HighLightStruct("Prefabs/NetworkedObjects/Shrines/ShrineCleanse", ModConfig.ShrineCleanseColor.Value, true, false)},
                {"shrinecombat", new HighLightStruct("Prefabs/NetworkedObjects/Shrines/ShrineCombat", ModConfig.ShrineCombatColor.Value)},
                {"shrinegoldshoresaccess", new HighLightStruct("Prefabs/NetworkedObjects/Shrines/ShrineGoldshoresAccess", ModConfig.ShrineGoldshoresAccessColor.Value)},
                {"shrinehealing", new HighLightStruct("Prefabs/NetworkedObjects/Shrines/ShrineHealing", ModConfig.ShrineHealingColor.Value, true, false)},
                {"shrinerestack", new HighLightStruct("Prefabs/NetworkedObjects/Shrines/ShrineRestack", ModConfig.ShrineRestackColor.Value, true, false)},
                {"teleporter", new HighLightStruct("Prefabs/NetworkedObjects/Teleporters/Teleporter1", ModConfig.TeleporterColor.Value, true, false)},
                {"lunarteleporter", new HighLightStruct("Prefabs/NetworkedObjects/Teleporters/LunarTeleporter Variant", ModConfig.TeleporterColor.Value, true, false)},
                {"timedchest", new HighLightStruct("Prefabs/NetworkedObjects/TimedChest", ModConfig.TimedChestColor.Value, false, false)},
                {"turret1broken", new HighLightStruct("Prefabs/NetworkedObjects/BrokenDrones/Turret1Broken", ModConfig.Turret1BrokenColor.Value, false, false)},
                {"voidchest", new HighLightStruct("Prefabs/NetworkedObjects/Chest/VoidChest", ModConfig.VoidChestColor.Value)},
                {"voidcoinbarrel", new HighLightStruct("Prefabs/NetworkedObjects/Chest/VoidCoinBarrel", ModConfig.VoidCoinBarrelColor.Value)},
                {"voidtriple", new HighLightStruct("Prefabs/NetworkedObjects/Chest/VoidTriple", ModConfig.VoidTripleColor.Value)},

                {"green-yellow", new HighLightStruct("Prefabs/NetworkedObjects/LunarCauldron, GreenToRed Variant", HighlightColor.Yellow,true, false)},
                {"green-blue", new HighLightStruct("Prefabs/NetworkedObjects/LunarCauldron, GreenToRed Variant", HighlightColor.LightBlue,true, false)},
                {"green-purple", new HighLightStruct("Prefabs/NetworkedObjects/LunarCauldron, GreenToRed Variant", HighlightColor.Pink,true, false)},
                {"red-yellow", new HighLightStruct("Prefabs/NetworkedObjects/LunarCauldron, RedToWhite Variant", HighlightColor.Yellow,true, false)},
                {"red-blue", new HighLightStruct("Prefabs/NetworkedObjects/LunarCauldron, RedToWhite Variant", HighlightColor.LightBlue,true, false)},
                {"red-purple", new HighLightStruct("Prefabs/NetworkedObjects/LunarCauldron, RedToWhite Variant", HighlightColor.Pink,true, false)},
                {"white-yellow", new HighLightStruct("Prefabs/NetworkedObjects/LunarCauldron, WhiteToGreen", HighlightColor.Yellow, true, false)},
                {"white-blue", new HighLightStruct("Prefabs/NetworkedObjects/LunarCauldron, WhiteToGreen", HighlightColor.LightBlue,false)},
                {"white-purple", new HighLightStruct("Prefabs/NetworkedObjects/LunarCauldron, WhiteToGreen", HighlightColor.Pink,true, false)},
            };
            foreach (KeyValuePair<string, HighLightStruct> kv in Instance)
            {
                switch (kv.Value.HighlightColor)
                {
                    case HighlightColor.None:
                    case HighlightColor.White:
                    case HighlightColor.Gray:
                    case HighlightColor.Red:
                    case HighlightColor.DarkRed:
                    case HighlightColor.LightRed:
                    case HighlightColor.VioletRed:
                        kv.Value.Tag = "red";
                        kv.Value.Color32 = new Color32(byte.MaxValue, 1, 1, byte.MaxValue);
                        break;
                    case HighlightColor.Yellow:
                    case HighlightColor.LightYellow:
                    case HighlightColor.Orange:
                    case HighlightColor.DarkOrange:
                        kv.Value.Tag = "yellow";
                        kv.Value.Color32 = new Color32(byte.MaxValue, byte.MaxValue, 1, byte.MaxValue);
                        break;
                    case HighlightColor.Green:
                    case HighlightColor.DarkGreen:
                    case HighlightColor.LightGreen:
                        kv.Value.Tag = "green";
                        kv.Value.Color32 = new Color32(1, byte.MaxValue, 1, byte.MaxValue);
                        break;
                    case HighlightColor.Cyan:
                        kv.Value.Tag = "cyan";
                        kv.Value.Color32 = new Color32(1, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                        break;
                    case HighlightColor.Blue:
                    case HighlightColor.LightBlue:
                        kv.Value.Tag = "blue";
                        kv.Value.Color32 = new Color32(1, 1, byte.MaxValue, byte.MaxValue);
                        break;
                    case HighlightColor.Purple:
                    case HighlightColor.Pink:
                        kv.Value.Tag = "purple";
                        kv.Value.Color32 = new Color32(byte.MaxValue, 1, byte.MaxValue, byte.MaxValue);
                        break;
                }
            }
        }

        private void UpdateHighlight()
        {
            if (Instance != null)
            {
                foreach (KeyValuePair<string, HighLightStruct> keyValue in Instance)
                {
                    GameObject gameObject = Resources.Load<GameObject>(keyValue.Value.Path);
                    try
                    {
                        if (gameObject)
                        {
                            Highlight highlight = gameObject.GetComponent<Highlight>();
                            //highlight.strength = 10;
                            highlight.isOn = true;
                        }
                    }
                    catch (Exception)
                    {
                        Logger.LogError($"{keyValue.Value.Path} 出现问题");
                    }
                    finally { }
                }
            }
            // 第二关的秘密按钮
            foreach (PressurePlateController pressurePlate in FindObjectsOfType<PressurePlateController>())
            {
                Highlight highlight = pressurePlate.gameObject.GetComponent<Highlight>();
                highlight.highlightColor = Highlight.HighlightColor.unavailable;
                highlight.isOn = true;
            }
            // 雪地图神龛、大锅重写
            foreach (var component in FindObjectsOfType<PurchaseInteraction>())
            {
                string searchName = component.gameObject.name.ToLower();
                if (searchName.Contains("snowy")
                    || searchName.ToLower().Contains("sandy")
                    || searchName.ToLower().Contains("cauldron")
                    || searchName.ToLower().Contains("timedchest")
                    || searchName.ToLower().Contains("newtstatue")
                    || searchName.ToLower().Contains("goldchest")
                    || searchName.ToLower().Contains("humanfan")
                    || searchName.ToLower().Contains("shopterminal"))
                {
                    component.gameObject.GetComponent<Highlight>().isOn = true;
                }
            }
        }
        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
            if (IsCurrentMapInBazaar() || self.Networkavailable)
            {
                return;
            }
            string serchName = self.gameObject.name.ToLower();
            try
            {
                foreach (KeyValuePair<string, HighLightStruct> keyValue in Instance) // 通用部分物品的用法
                {
                    if (serchName.StartsWith(keyValue.Key))
                    {
                        if (!keyValue.Value.AlwaysShowOn)
                        {
                            HighlightOff(self.gameObject.GetComponent<Highlight>());
                        }
                        if (keyValue.Value.ShouldDestroy)
                        {
                            DestroyObject(self.gameObject, keyValue.Value.DelayDestroy);
                        }
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"{serchName} 在 PurchaseInteraction_OnInteractionBegin 出错了");
            }
            finally
            {

            }
        }
        private void MultiShopController_OnPurchase(On.RoR2.MultiShopController.orig_OnPurchase orig, MultiShopController self, Interactor interactor, PurchaseInteraction purchaseInteraction)
        {
            orig(self, interactor, purchaseInteraction);
            if (!self.Networkavailable)
            {
                foreach (GameObject terminal in self.terminalGameObjects)
                {
                    HighlightOff(terminal.GetComponent<Highlight>());
                    DestroyObject(terminal);
                }
                DestroyObject(self.gameObject);
            }
        }
        private void BarrelInteraction_OnInteractionBegin(On.RoR2.BarrelInteraction.orig_OnInteractionBegin orig, BarrelInteraction self, Interactor activator)
        {
            orig(self, activator);
            HighlightOff(self.gameObject.GetComponent<Highlight>());
            if (Instance.TryGetValue("barrel1", out HighLightStruct @struct))
            {
                if (@struct.ShouldDestroy)
                {
                    DestroyObject(self.gameObject);
                }
            }
        }
        private void Barrel_Opened_OnEnter(On.EntityStates.Barrel.Opened.orig_OnEnter orig, EntityStates.Barrel.Opened self)
        {
            //Send("Barrel_Opened_OnEnter");
            orig(self);
            HighlightOff(self.outer.gameObject.GetComponent<Highlight>());
            DestroyObject(self.outer.gameObject);
        }
        private void ActivateFan_OnEnter(On.EntityStates.Barrel.ActivateFan.orig_OnEnter orig, EntityStates.Barrel.ActivateFan self)
        {
            //Send("ActivateFan_OnEnter");
            orig(self);
            HighlightOff(self.outer.gameObject.GetComponent<Highlight>());
        }
        //private void RouletteChest_Opened_OnEnter(On.RoR2.RouletteChestController.Opened.orig_OnEnter orig, EntityStates.EntityState self)
        //{
        //    //Send("Chest_Opened_OnEnter");
        //    orig(self);
        //    DestroyObject(self.outer.gameObject);
        //}
        private void ShrineBloodBehavior_FixedUpdate(On.RoR2.ShrineBloodBehavior.orig_FixedUpdate orig, ShrineBloodBehavior self)
        {
            orig(self);
            int maxCount = self.maxPurchaseCount;
            int buyCount = Reflection.GetFieldValue<int>(self, "purchaseCount");
            if (buyCount >= maxCount)
            {
                HighlightOff(self.gameObject.GetComponent<Highlight>());
                DestroyObject(self.gameObject);
            }
        }
        private void ShrineBossBehavior_FixedUpdate(On.RoR2.ShrineBossBehavior.orig_FixedUpdate orig, ShrineBossBehavior self)
        {
            orig(self);
            int maxCount = self.maxPurchaseCount;
            int buyCount = Reflection.GetFieldValue<int>(self, "purchaseCount");
            if (buyCount >= maxCount)
            {
                HighlightOff(self.gameObject.GetComponent<Highlight>());
                DestroyObject(self.gameObject);
            }
        }
        private void ShrineChanceBehavior_FixedUpdate(On.RoR2.ShrineChanceBehavior.orig_FixedUpdate orig, ShrineChanceBehavior self)
        {
            orig(self);
            int maxCount = self.maxPurchaseCount;
            int buyCount = Reflection.GetFieldValue<int>(self, "successfulPurchaseCount");
            if (buyCount >= maxCount)
            {
                HighlightOff(self.gameObject.GetComponent<Highlight>());
                DestroyObject(self.gameObject);
            }
        }
        private void ShrineCombatBehavior_FixedUpdate(On.RoR2.ShrineCombatBehavior.orig_FixedUpdate orig, ShrineCombatBehavior self)
        {
            orig(self);
            int maxCount = self.maxPurchaseCount;
            int buyCount = Reflection.GetFieldValue<int>(self, "purchaseCount");
            if (buyCount >= maxCount)
            {
                HighlightOff(self.gameObject.GetComponent<Highlight>());
                DestroyObject(self.gameObject);
            }
        }
        private void ShrineHealingBehavior_FixedUpdate(On.RoR2.ShrineHealingBehavior.orig_FixedUpdate orig, ShrineHealingBehavior self)
        {
            orig(self);
            int maxCount = self.maxPurchaseCount;
            int buyCount = self.purchaseCount;
            if (buyCount >= maxCount)
            {
                HighlightOff(self.gameObject.GetComponent<Highlight>());
            }
        }
        private void ShrineRestackBehavior_FixedUpdate(On.RoR2.ShrineRestackBehavior.orig_FixedUpdate orig, ShrineRestackBehavior self)
        {
            orig(self);
            int maxCount = self.maxPurchaseCount;
            int buyCount = Reflection.GetFieldValue<int>(self, "purchaseCount");
            if (buyCount >= maxCount)
            {
                HighlightOff(self.gameObject.GetComponent<Highlight>());
                DestroyObject(self.gameObject);
            }
        }
        
        private void HighlightOff(Highlight highlight)
        {
            highlight.isOn = false;
            highlight.enabled = false;
        }
        private void DestroyObject(GameObject gameObject, float delay = 2)
        {
            if (ModConfig.EnableAfterBuyDestroy.Value)
            {
                //Renderer renderer = gameObject.GetComponent<Highlight>().targetRenderer;
                //renderer.forceRenderingOff = true;
                //renderer.material.color = new Color(1, 1, 1, 1);
                UnityEngine.Object.Destroy(gameObject, delay); 
            }
        }
        private Color Highlight_GetColor(On.RoR2.Highlight.orig_GetColor orig, Highlight self)
        {
            Color32? color32 = null;
            if (!ModConfig.EnableRGB.Value)
            {
                foreach (KeyValuePair<string, HighLightStruct> keyValue in Instance)
                {
                    // 这里很奇怪，不能用字符串 StartWith() 或者 Contain()
                    // 第一次精准匹配
                    string searchName = self.gameObject.name.Replace("(Clone)", "").Trim();
                    if (searchName.ToLower() == keyValue.Key)
                    {
                        self.isOn = true;
                        color32 = SelectColor(keyValue.Value.HighlightColor);
                        break;
                    }
                    // 第二次模糊查询
                    searchName = self.gameObject.name.ToLower();
                    if (searchName.Contains(keyValue.Key) || searchName.StartsWith(keyValue.Key))
                    {
                        self.isOn = true;
                        color32 = SelectColor(keyValue.Value.HighlightColor);
                        break;
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string, HighLightStruct> keyValue in Instance)
                {
                    string searchName = self.gameObject.name.Replace("(Clone)", "").Trim();
                    if (searchName.ToLower() == keyValue.Key)
                    {
                        self.isOn = true;
                        color32 = RGBColor(keyValue.Value);
                        break;
                    }
                    searchName = self.gameObject.name.ToLower();
                    if (searchName.Contains(keyValue.Key) || searchName.StartsWith(keyValue.Key))
                    {
                        self.isOn = true;
                        color32 = RGBColor(keyValue.Value);
                        break;
                    }
                }
            }

            if (color32 is null)
            {
                return orig(self);
            }
            else
            {
                return (Color32)color32;
            }
        }

        private Color32 SelectColor(HighlightColor prefabColor)
        {
            switch (prefabColor)
            {
                case HighlightColor.None:
                    return new Color32(1, 1, 1, byte.MaxValue);
                case HighlightColor.White:
                    return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                case HighlightColor.Gray:
                    return new Color32(byte.MaxValue / 2, byte.MaxValue / 2, byte.MaxValue / 2, byte.MaxValue);
                case HighlightColor.Red:
                    return new Color32(byte.MaxValue, 1, 1, byte.MaxValue);
                case HighlightColor.DarkRed:
                    return ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier3ItemDark);
                case HighlightColor.LightRed:
                    return new Color32(byte.MaxValue, byte.MaxValue / 2, byte.MaxValue / 2, byte.MaxValue);
                case HighlightColor.VioletRed:
                    return new Color32(byte.MaxValue, byte.MaxValue / 4, byte.MaxValue / 2, byte.MaxValue);
                case HighlightColor.Purple:
                    return new Color32(byte.MaxValue, 1, byte.MaxValue, byte.MaxValue);
                case HighlightColor.Pink:
                    return new Color32(byte.MaxValue, byte.MaxValue / 2, byte.MaxValue, byte.MaxValue);
                case HighlightColor.Orange:
                    return new Color32(byte.MaxValue, byte.MaxValue / 2, 1, byte.MaxValue);
                case HighlightColor.DarkOrange:
                    return ColorCatalog.GetColor(ColorCatalog.ColorIndex.Teleporter);
                case HighlightColor.Green:
                    return new Color32(1, byte.MaxValue, 1, byte.MaxValue);
                case HighlightColor.DarkGreen:
                    return ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier2ItemDark);
                case HighlightColor.LightGreen:
                    return new Color32(byte.MaxValue / 2, byte.MaxValue, byte.MaxValue / 2, byte.MaxValue);
                case HighlightColor.Yellow:
                    return new Color32(byte.MaxValue, byte.MaxValue, 1, byte.MaxValue);
                case HighlightColor.LightYellow:
                    return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue / 2, byte.MaxValue);
                case HighlightColor.Blue:
                    return new Color32(1, 1, byte.MaxValue, byte.MaxValue);
                case HighlightColor.LightBlue:
                    return new Color32(byte.MaxValue / 4, byte.MaxValue / 2, byte.MaxValue, byte.MaxValue);
                case HighlightColor.Cyan:
                    return new Color32(1, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            }
            return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        }
        private Color32 RGBColor(HighLightStruct @struct)
        {
            if (@struct.Tag == "red")
            {
                if (@struct.Color32.r >= 250) @struct.Tag = "yellow";
                @struct.Color32 = new Color32((byte)(@struct.Color32.r + ModConfig.RGBInterval.Value), 1, 1, byte.MaxValue);
            }
            else if (@struct.Tag == "yellow")
            {
                if (@struct.Color32.g >= 250) @struct.Tag = "green";
                @struct.Color32 = new Color32(byte.MaxValue, (byte)(@struct.Color32.g + ModConfig.RGBInterval.Value), 1, byte.MaxValue);
            }
            else if (@struct.Tag == "green")
            {
                if (@struct.Color32.r <= 5) @struct.Tag = "cyan";
                @struct.Color32 = new Color32((byte)(@struct.Color32.r - ModConfig.RGBInterval.Value), byte.MaxValue, 1, byte.MaxValue);
            }
            else if (@struct.Tag == "cyan")
            {
                if (@struct.Color32.b >= 250) @struct.Tag = "blue";
                @struct.Color32 = new Color32(1, byte.MaxValue, (byte)(@struct.Color32.b + ModConfig.RGBInterval.Value), byte.MaxValue);
            }
            else if (@struct.Tag == "blue")
            {
                if (@struct.Color32.g <= 5) @struct.Tag = "purple";
                @struct.Color32 = new Color32(1, (byte)(@struct.Color32.g - ModConfig.RGBInterval.Value), byte.MaxValue, byte.MaxValue);
            }
            else if (@struct.Tag == "purple")
            {
                if (@struct.Color32.r >= 250) @struct.Tag = "next";
                @struct.Color32 = new Color32((byte)(@struct.Color32.r + ModConfig.RGBInterval.Value), 1, byte.MaxValue, byte.MaxValue);
            }
            else if (@struct.Tag == "next")
            {
                if (@struct.Color32.b <= 5) @struct.Tag = "red";
                @struct.Color32 = new Color32(byte.MaxValue, 1, (byte)(@struct.Color32.b - ModConfig.RGBInterval.Value), byte.MaxValue);
            }
            return @struct.Color32;
        }
        private bool IsCurrentMapInBazaar()
        {
            return SceneManager.GetActiveScene().name == "bazaar";
        }
        public static void Send(string message)
        {
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = message
            });
        }
        [ConCommand(commandName = "reload_rgb", flags = 0, helpText = "重读配置文件")]
        private static void Command_ReloadColor(ConCommandArgs args)
        {
            ModConfig.ColorConfigFile.Reload();
            InitHighlightData();
        }
        internal class HighLightStruct
        {
            public string Path { get; set; }
            public HighlightColor HighlightColor { get; set; }
            public bool AlwaysShowOn { get; set; } // 总是显示高亮
            public bool ShouldDestroy { get; set; } // 是否该删除
            public float DelayDestroy { get; set; } // 删除时间
            public string Tag;
            public Color32 Color32;
            
            public HighLightStruct(string path, HighlightColor highlightColor, bool alwaysShowOn = false, bool shouldDestroy = true, float delayDestroy = 3)
            {
                Path = path;
                HighlightColor = highlightColor;
                AlwaysShowOn = alwaysShowOn;
                ShouldDestroy = shouldDestroy;
                DelayDestroy = delayDestroy;
            }
        }
    }

    enum HighlightColor
    {
        None, White, Gray, Red, DarkRed, LightRed, VioletRed, Purple, Pink, Orange, DarkOrange, Green, DarkGreen, LightGreen, Yellow, LightYellow, Cyan, Blue, LightBlue
    }

    class ModConfig
    {
        public static ConfigFile ColorConfigFile;

        public static ConfigEntry<bool> EnableMod;
        public static ConfigEntry<bool> EnableRGB;
        public static ConfigEntry<byte> RGBInterval;
        public static ConfigEntry<bool> EnableAfterBuyDestroy;
        public static ConfigEntry<HighlightColor> Barrel1Color;
        public static ConfigEntry<HighlightColor> CasinoChestColor;
        public static ConfigEntry<HighlightColor> CategoryChestDamageColor;
        public static ConfigEntry<HighlightColor> CategoryChestHealingColor;
        public static ConfigEntry<HighlightColor> CategoryChestUtilityColor;
        public static ConfigEntry<HighlightColor> Chest1Color;
        public static ConfigEntry<HighlightColor> Chest1StealthedVariantColor;
        public static ConfigEntry<HighlightColor> Chest2Color;
        public static ConfigEntry<HighlightColor> Drone1BrokenColor;
        public static ConfigEntry<HighlightColor> Drone2BrokenColor;
        public static ConfigEntry<HighlightColor> DuplicatorColor;
        public static ConfigEntry<HighlightColor> DuplicatorLargeColor;
        public static ConfigEntry<HighlightColor> DuplicatorMilitaryColor;
        public static ConfigEntry<HighlightColor> DuplicatorWildColor;
        public static ConfigEntry<HighlightColor> EmergencyDroneBrokenColor;
        public static ConfigEntry<HighlightColor> EquipmentBarrelColor;
        public static ConfigEntry<HighlightColor> EquipmentDroneBrokenColor;
        public static ConfigEntry<HighlightColor> FlameDroneBrokenColor;
        public static ConfigEntry<HighlightColor> FreeChestMultiShopColor;
        public static ConfigEntry<HighlightColor> GoldChestColor;
        public static ConfigEntry<HighlightColor> GoldshoresBeaconColor;
        public static ConfigEntry<HighlightColor> HumanFanColor;
        public static ConfigEntry<HighlightColor> LockboxColor;
        public static ConfigEntry<HighlightColor> LockboxVoidColor;
        public static ConfigEntry<HighlightColor> LogPickup2Color;
        public static ConfigEntry<HighlightColor> LunarCauldronGreenColor;
        public static ConfigEntry<HighlightColor> LunarCauldronRedColor;
        public static ConfigEntry<HighlightColor> LunarCauldronWhiteColor;
        public static ConfigEntry<HighlightColor> LunarChestColor;
        public static ConfigEntry<HighlightColor> LunarShopTerminalColor;
        public static ConfigEntry<HighlightColor> MegaDroneBrokenColor;
        public static ConfigEntry<HighlightColor> MissileDroneBrokenColor;
        public static ConfigEntry<HighlightColor> MultiShopEquipmentTerminalColor;
        public static ConfigEntry<HighlightColor> MultiShopLargeTerminalColor;
        public static ConfigEntry<HighlightColor> MultiShopTerminalColor;
        public static ConfigEntry<HighlightColor> NewtStatueColor;
        public static ConfigEntry<HighlightColor> RadarTowerColor;
        public static ConfigEntry<HighlightColor> ScavBackpackColor;
        public static ConfigEntry<HighlightColor> ScavLunarBackpackColor;
        public static ConfigEntry<HighlightColor> ScrapperColor;
        public static ConfigEntry<HighlightColor> ShrineBloodColor;
        public static ConfigEntry<HighlightColor> ShrineBossColor;
        public static ConfigEntry<HighlightColor> ShrineChanceColor;
        public static ConfigEntry<HighlightColor> ShrineCleanseColor;
        public static ConfigEntry<HighlightColor> ShrineCombatColor;
        public static ConfigEntry<HighlightColor> ShrineGoldshoresAccessColor;
        public static ConfigEntry<HighlightColor> ShrineHealingColor;
        public static ConfigEntry<HighlightColor> ShrineRestackColor;
        public static ConfigEntry<HighlightColor> TimedChestColor;
        public static ConfigEntry<HighlightColor> TeleporterColor;
        public static ConfigEntry<HighlightColor> Turret1BrokenColor;
        public static ConfigEntry<HighlightColor> VoidChestColor;
        public static ConfigEntry<HighlightColor> VoidCoinBarrelColor;
        public static ConfigEntry<HighlightColor> VoidTripleColor;

        public static void InitConfig(ConfigFile config)
        {
            ColorConfigFile = config;
            EnableMod = ColorConfigFile.Bind("Setting 设置", "EnableMod", true, "Enable the mod.\n启用模组");
            if (EnableMod.Value)
            {
                EnableRGB = ColorConfigFile.Bind("Setting 设置", "EnableRGB", true, "If enabled... Welcome to DJ Vanilla. If disable, use the following parameter colors.\n启用RGB模式，如果启用，全部物体都会...摇起来！不启用，会使用下方颜色。");
                if (EnableRGB.Value)
                {
                    RGBInterval = ColorConfigFile.Bind("Setting 设置", "Level", (byte)2, "RGB switching frequency, level 1 to 10\nRGB切换级别，1-10级");
                    if (RGBInterval.Value < 1)
                    {
                        RGBInterval.Value = 1;
                    }
                    if (RGBInterval.Value > 10)
                    {
                        RGBInterval.Value = 10;
                    }
                }
                EnableAfterBuyDestroy = ColorConfigFile.Bind("Setting 设置", "EnableAfterBuyDestroy", true, "If enabled will delete object after purchase.\n启用购买后删除物体");
                Barrel1Color = ColorConfigFile.Bind("Setting 设置", "Barrel1 Color", HighlightColor.Yellow, "Barrel\n钱桶");
                ScrapperColor = ColorConfigFile.Bind("Setting 设置", "Scrapper Color", HighlightColor.Cyan, "Scrapper\n收割机");
                CasinoChestColor = ColorConfigFile.Bind("Setting 设置", "CasinoChest Color", HighlightColor.Cyan, "Adaptive Chest\n适配宝箱");
                Chest1Color = ColorConfigFile.Bind("Setting 设置", "Chest1 Color", HighlightColor.White, "Chest\n宝箱");
                Chest2Color = ColorConfigFile.Bind("Setting 设置", "Chest2 Color", HighlightColor.Green, "Large Chest\n巨大宝箱");
                GoldChestColor = ColorConfigFile.Bind("Setting 设置", "GoldChest Color", HighlightColor.VioletRed, "Legendary Chest\n传奇宝箱");
                EquipmentBarrelColor = ColorConfigFile.Bind("Setting 设置", "EquipmentBarrel Color", HighlightColor.Orange, "Equipment Barrel\n武器装备桶");
                Chest1StealthedVariantColor = ColorConfigFile.Bind("Setting 设置", "Chest1StealthedVariant Color", HighlightColor.Purple, "Cloaked Chest\n被遮盖的宝箱");
                CategoryChestDamageColor = ColorConfigFile.Bind("Setting 设置", "CategoryChestDamage Color", HighlightColor.LightRed, "Chest - Damage\n宝箱 - 伤害");
                CategoryChestHealingColor = ColorConfigFile.Bind("Setting 设置", "CategoryChestHealing Color", HighlightColor.LightGreen, "Chest - Healing\n宝箱 - 治疗");
                CategoryChestUtilityColor = ColorConfigFile.Bind("Setting 设置", "CategoryChestUtility Color", HighlightColor.Pink, "Chest - Utility\n宝箱 - 辅助");
                LockboxColor = ColorConfigFile.Bind("Setting 设置", "Lockbox Color", HighlightColor.VioletRed, "Rusty Lockbox\n生锈带锁箱");
                LockboxVoidColor = ColorConfigFile.Bind("Setting 设置", "LockboxVoid Color", HighlightColor.Purple, "Encrusted Lockbox\n结壳的带锁箱");
                LunarChestColor = ColorConfigFile.Bind("Setting 设置", "LunarChest Color", HighlightColor.LightBlue, "Lunar Pod\n月球舱");
                LunarShopTerminalColor = ColorConfigFile.Bind("Setting 设置", "LunarShopTerminal Color", HighlightColor.LightBlue, "Lunar Bud\n月球蓓蕾");
                TimedChestColor = ColorConfigFile.Bind("Setting 设置", "TimedChest Color", HighlightColor.Cyan, "Timed Security Chest\n限时保险箱");
                VoidChestColor = ColorConfigFile.Bind("Setting 设置", "VoidChest Color", HighlightColor.Purple, "Void Cradle\n虚空宝箱");
                VoidCoinBarrelColor = ColorConfigFile.Bind("Setting 设置", "VoidCoinBarrel Color", HighlightColor.Purple, "Void Marker\n虚空标记");
                MultiShopTerminalColor = ColorConfigFile.Bind("Setting 设置", "MultiShopTerminal Color", HighlightColor.White, "Multishop Terminal\n多重商店白装");
                MultiShopLargeTerminalColor = ColorConfigFile.Bind("Setting 设置", "MultiShopLargeTerminal Color", HighlightColor.Green, "Multishop Terminal(Tier 2)\n多重商店绿装");
                MultiShopEquipmentTerminalColor = ColorConfigFile.Bind("Setting 设置", "MultiShopEquipmentTerminal Color", HighlightColor.Orange, "Multishop Equipment Terminal\n多重商店主动装备");
                FreeChestMultiShopColor = ColorConfigFile.Bind("Setting 设置", "FreeChestMultiShop Color", HighlightColor.DarkOrange, "Shipping Request Form\n白色装备（运输申请单）");
                VoidTripleColor = ColorConfigFile.Bind("Setting 设置", "VoidTriple Color", HighlightColor.Purple, "Void Potential\n虚空潜能三选一");
                DuplicatorWildColor = ColorConfigFile.Bind("Setting 设置", "DuplicatorWild Color", HighlightColor.Yellow, "Overgrown 3D Printer\n黄色打印机");
                Drone1BrokenColor = ColorConfigFile.Bind("Setting 设置", "Drone1Broken Color", HighlightColor.LightYellow, "Broken Gunner Drone\n损坏的抢手无人机");
                Drone2BrokenColor = ColorConfigFile.Bind("Setting 设置", "Drone2Broken Color", HighlightColor.LightGreen, "Broken Healing Drone\n损坏的治疗无人机");
                EmergencyDroneBrokenColor = ColorConfigFile.Bind("Setting 设置", "EmergencyDroneBroken Color", HighlightColor.LightYellow, "Broken Emergency Drone\n损坏的应急无人机");
                EquipmentDroneBrokenColor = ColorConfigFile.Bind("Setting 设置", "EquipmentDroneBroken Color", HighlightColor.DarkOrange, "Broken Equipment Drone\n损坏的装备无人机");
                FlameDroneBrokenColor = ColorConfigFile.Bind("Setting 设置", "FlameDroneBroken Color", HighlightColor.DarkRed, "Broken Incinerator Drone\n损坏的焚烧无人机");
                MissileDroneBrokenColor = ColorConfigFile.Bind("Setting 设置", "MissileDroneBroken Color", HighlightColor.Green, "Broken Missile Drone\n损坏的导弹无人机");
                Turret1BrokenColor = ColorConfigFile.Bind("Setting 设置", "Turret1Broken Color", HighlightColor.DarkGreen, "Broken Gunner Turret\n损坏的枪手机枪塔");
                MegaDroneBrokenColor = ColorConfigFile.Bind("Setting 设置", "MegaDroneBroken Color", HighlightColor.Cyan, "Broken TC-280\n损坏的TC-280");
                DuplicatorColor = ColorConfigFile.Bind("Setting 设置", "Duplicator Color", HighlightColor.White, "3D Printer(Tier 1)\n白色打印机");
                DuplicatorLargeColor = ColorConfigFile.Bind("Setting 设置", "DuplicatorLarge Color", HighlightColor.Green, "3D Printer(Tier 2)\n绿色打印机");
                DuplicatorMilitaryColor = ColorConfigFile.Bind("Setting 设置", "DuplicatorMilitary Color", HighlightColor.Red, "Mili-Tech Printer\n红色打印机");
                LunarCauldronRedColor = ColorConfigFile.Bind("Setting 设置", "LunarCauldronRedToWhite Color", HighlightColor.White, "White Cauldron\n白色大锅");
                LunarCauldronWhiteColor = ColorConfigFile.Bind("Setting 设置", "LunarCauldronWhiteToGreen Color", HighlightColor.Green, "Greed Cauldron\n绿色大锅");
                LunarCauldronGreenColor = ColorConfigFile.Bind("Setting 设置", "LunarCauldronGreenToRed Color", HighlightColor.Red, "Red Cauldron\n红色大锅");
                LogPickup2Color = ColorConfigFile.Bind("Setting 设置", "LogPickup Color", HighlightColor.Pink, "Log Book\n日志文件");
                RadarTowerColor = ColorConfigFile.Bind("Setting 设置", "RadarTower Color", HighlightColor.Cyan, "Radar Scanner\n雷达扫描装置");
                ScavBackpackColor = ColorConfigFile.Bind("Setting 设置", "ScavBackpack Color", HighlightColor.Cyan, "Scavenger's Sack\n清道夫的大口袋");
                ScavLunarBackpackColor = ColorConfigFile.Bind("Setting 设置", "ScavLunarBackpack Color", HighlightColor.LightBlue, "Scavenger's Lunar Sack\n清道夫的月币大口袋");
                GoldshoresBeaconColor = ColorConfigFile.Bind("Setting 设置", "GoldshoresBeacon Color", HighlightColor.LightYellow, "Halcyon Beacons\n哈雷肯信标");
                ShrineBloodColor = ColorConfigFile.Bind("Setting 设置", "ShrineBlood Color", HighlightColor.LightRed, "Shrine of Blood\n鲜血神龛");
                ShrineBossColor = ColorConfigFile.Bind("Setting 设置", "ShrineBoss Color", HighlightColor.LightBlue, "Shrine of the Mountain\n山之神龛");
                ShrineChanceColor = ColorConfigFile.Bind("Setting 设置", "ShrineChance Color", HighlightColor.LightYellow, "Shrine of Chance\n机率神龛");
                ShrineCleanseColor = ColorConfigFile.Bind("Setting 设置", "ShrineCleanse Color", HighlightColor.Yellow, "Cleansing Pool\n净化池");
                ShrineCombatColor = ColorConfigFile.Bind("Setting 设置", "ShrineCombat Color", HighlightColor.Pink, "Shrine of Combat\n战斗神龛");
                ShrineHealingColor = ColorConfigFile.Bind("Setting 设置", "ShrineHealing Color", HighlightColor.LightGreen, "Shrine of the Woods\n森林神龛");
                ShrineRestackColor = ColorConfigFile.Bind("Setting 设置", "ShrineRestack Color", HighlightColor.Pink, "Shrine of Order\n秩序");
                ShrineGoldshoresAccessColor = ColorConfigFile.Bind("Setting 设置", "ShrineGoldshoresAccess Color", HighlightColor.Yellow, "Altar of Gold\n黄金祭坛");
                TeleporterColor = ColorConfigFile.Bind("Setting 设置", "TeleporterColor Color", HighlightColor.DarkOrange, "Teleporter\n传送门");
                NewtStatueColor = ColorConfigFile.Bind("Setting 设置", "NewtStatue Color", HighlightColor.LightBlue, "Newt Statue\n纽特祭坛");
                HumanFanColor = ColorConfigFile.Bind("Setting 设置", "HumanFan Color", HighlightColor.White, "Fan\n风扇");
            }
        }
    }
}
