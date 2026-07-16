using BepInEx.Configuration;
using ChaosMod.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using static BuffContainer;
using static UnityEngine.EventSystems.EventTrigger;

namespace ChaosMod.Events
{
    public class EventManager : MonoBehaviour
    {
        public static readonly List<Event> Events = new List<Event>();
        public static Dictionary<string, int> eventsOnCooldown = new Dictionary<string, int>();
        public static Dictionary<string, UnityEngine.Object> prefabs = new Dictionary<string, UnityEngine.Object>();
        public static AssetBundle chaosBundle = null;
        public static void FillList()
        {
            Events.Clear();
            //1-10
            Events.Add(new Event().SetEntry("Perk Overdose", 0f, PerkOverdose)); //10 of a random perk
            Events.Add(new Event().SetEntry("Bloodbug Infestation", 0f, BloodbugHorde)); //10 Bloodbugs
            Events.Add(new Event().SetEntry("House M.D.", 30f, SpawnHouseMD));
            Events.Add(new Event().SetEntry("Random Perk", 0f, RandomPerk));
            Events.Add(new Event().SetEntry("Random Item", 0f, RandomItem));
            Events.Add(new Event().SetEntry("You are playing in IRON KNUCKLE mode. No perks for you!", 0f, IronKnuckle, 30)); //Remove all Perks (and only perks)
            Events.Add(new Event().SetEntry("Jumpscare!", 0f, Jumpscare));
            Events.Add(new Event().SetEntry("It's Turbo Time!", 0f, TurboTime)); //Prepare for launch
            Events.Add(new Event().SetEntry("FEAST MODE ACTIVATED", 20f, FeastMode)); //Prepare for lunch
            Events.Add(new Event().SetEntry("Roach Rain", 20f, SkyDiamonds));
            //11-20
            Events.Add(new Event().SetEntry("Yarr Harr", 0f, PirateShip));
            Events.Add(new Event().SetEntry("Yahoo!", 0f, PlayerLaunch)); //Forward Launch
            Events.Add(new Event().SetEntry("Will you be my buddy?", 0f, SpawnBuddies)); // 10 buddies
            Events.Add(new Event().SetEntry("Moving Day", 0f, SpawnFurniture));
            Events.Add(new Event().SetEntry("Old Spice Train", 0f, OldSpiceTrain));
            Events.Add(new Event().SetEntry("Advertisement", 4f, JoeBiden));
            Events.Add(new Event().SetEntry("The Red Carpet", 0f, RedCarpet)); //Red Roach Rum
            Events.Add(new Event().SetEntry("Prop Magnet", 0f, PropMagnet));
            Events.Add(new Event().SetEntry("Spawn Shrek", 25f, SpawnShrek));
            Events.Add(new Event().SetEntry("Gift Rain", 10f, HappyBirthday));
            //21-30
            Events.Add(new Event().SetEntry("BBQ CHICKEN ALERT", 12f, BBQChickenAlert));
            Events.Add(new Event().SetEntry("Random Trinket", 0f, RandomTrinket));
            Events.Add(new Event().SetEntry("Random Binding", 60f, RandomBinding));
            Events.Add(new Event().SetEntry("Random Artifact", 0f, RandomArtifact, 60)); //maybe too op
            Events.Add(new Event().SetEntry("Low Gravity", 15f, LowGravity));
            Events.Add(new Event().SetEntry("Butterfingers", 0f, ButterFingers));
            Events.Add(new Event().SetEntry("Drunk", 15f, Drunk, 6));
            Events.Add(new Event().SetEntry("Give up, you're surrounded", 0f, TurretCircle));
            Events.Add(new Event().SetEntry("Turn props into loot", 0f, PropLoot, 30));
            Events.Add(new Event().SetEntry("I'll take that, it's mine now", 0f, YoinkItem));
            //31
            Events.Add(new Event().SetEntry("Double Event!", 0f, DoubleRandomEvent)); //needs to be at the end beacuse I'm too lazy to do it differently


            foreach (var ev in Events)
            {
                if (!ChaosSettings.eventEnabled.ContainsKey(ev.name))
                    ChaosSettings.eventEnabled.Add(ev.name, true);
            }

            ChaosSettings.Load();
        }
        public static void RandomEvent(bool ignoreDouble = false)
        {
            if (chaosBundle == null)
                LoadBundle();
            if (Events.Count == 0)
                FillList();

            foreach (Event val in Events)
                if (eventsOnCooldown.TryGetValue(val.name, out int c))
                     if (c > 1)
                        eventsOnCooldown[val.name] -= 1;
                     else
                        eventsOnCooldown.Remove(val.name);

            List<Event> valid = Events.Where(e => ChaosSettings.eventEnabled.TryGetValue(e.name, out bool on) && on).ToList();

            if (valid.Count == 0)
                return;

            List<Event> offCooldown = valid.Where(e => !eventsOnCooldown.TryGetValue(e.name, out int c)).ToList();

            if (offCooldown.Any())
                valid = offCooldown;

            int range = valid.Count;
            if (ignoreDouble)
                range--;

            Event randEvent = valid[UnityEngine.Random.Range(0, range)];
            EventEntry entry = ChaosUI.instance.AddEntry(randEvent.name, randEvent.time, ignoreDouble);
            randEvent.action?.Invoke(entry);

            if (randEvent.cooldown > 0)
                eventsOnCooldown[randEvent.name] = randEvent.cooldown;
        }
        public static void LoadBundle()
        {
            string dllPath = Assembly.GetExecutingAssembly().Location;
            string folder = Path.GetDirectoryName(dllPath);
            string path = Path.Combine(folder, "chaosassets");

            if (!File.Exists(path))
            {
                Debug.LogError($"[ChaosMod - LoadBundle] AssetBundle not found - Make sure it's in the same folder as the .dll file and it's named 'chaosassets' | {path}");
                return;
            }
            else
                chaosBundle = AssetBundle.LoadFromFile(path);

            if (chaosBundle == null)
            {
                Debug.LogError("[ChaosMod - LoadBundle] Failed to load ChaosMod AssetBundle");
                return;
            }
            else
                Debug.Log($"[ChaosMod - LoadBundle] AssetBundle Loaded!");

            prefabs["House"] = chaosBundle.LoadAsset<GameObject>("House");
            prefabs["Jumpscare"] = chaosBundle.LoadAsset<GameObject>("Jumpscare");
            prefabs["TurboTime"] = chaosBundle.LoadAsset<AudioClip>("TurboTime");
            prefabs["FeastMode"] = chaosBundle.LoadAsset<AudioClip>("FeastMode");
            prefabs["SkyDiamonds"] = chaosBundle.LoadAsset<AudioClip>("SkyDiamonds");
            prefabs["PirateShip"] = chaosBundle.LoadAsset<GameObject>("PirateShip");
            prefabs["ShipCollide"] = chaosBundle.LoadAsset<AudioClip>("ShipCollide");
            prefabs["Yahoo"] = chaosBundle.LoadAsset<AudioClip>("Yahoo");
            prefabs["Train"] = chaosBundle.LoadAsset<GameObject>("Train");
            prefabs["OldSpice1"] = chaosBundle.LoadAsset<AudioClip>("OldSpice1");
            prefabs["OldSpice2"] = chaosBundle.LoadAsset<AudioClip>("OldSpice2");
            prefabs["OldSpice3"] = chaosBundle.LoadAsset<AudioClip>("OldSpice3");
            prefabs["TrainHit"] = chaosBundle.LoadAsset<AudioClip>("TrainHit");
            prefabs["JoeBiden"] = chaosBundle.LoadAsset<GameObject>("JoeBiden");
            prefabs["BDay"] = chaosBundle.LoadAsset<AudioClip>("BDay");
            prefabs["BBQChicken"] = chaosBundle.LoadAsset<AudioClip>("BBQChickenAudio");
            prefabs["BBQChickenPlatform"] = chaosBundle.LoadAsset<GameObject>("BBQChickenPlatform");
            prefabs["Shrek"] = chaosBundle.LoadAsset<GameObject>("Shrek");
            prefabs["ButterfingerSlip"] = chaosBundle.LoadAsset<AudioClip>("slip");
            prefabs["YoinkAudio"] = chaosBundle.LoadAsset<AudioClip>("minenow");
            prefabs["Hello"] = chaosBundle.LoadAsset<AudioClip>("den_samgrub_bonus_sound");
        }
        public static void PlayAudio(AudioClip clip, float distortion = 0f, float volume = 1f, AudioMixerGroup mixerGroup = null)
        {
            if (clip == null) return;

            GameObject go = new GameObject(clip.name);
            go.transform.position = ENT_Player.playerObject.transform.position;

            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;

            if (distortion > 0f)
            {
                AudioDistortionFilter filter = go.AddComponent<AudioDistortionFilter>();
                filter.distortionLevel = distortion;
            }

            if (mixerGroup)
                source.outputAudioMixerGroup = mixerGroup;

            source.Play();

            Destroy(go, clip.length);
        }
        private static Perk GetRandomPerk(string[] tags = null)
        {
            List<Perk> perkAssets = CL_AssetManager.GetFullCombinedAssetDatabase().perkAssets;
            if (perkAssets == null || perkAssets.Count == 0)
            {
                Debug.LogError("[ChaosMod - GetRandomPerk] Fail due to no/null perks");
                return null;
            }

            if (tags == null)
                return perkAssets[UnityEngine.Random.Range(0, perkAssets.Count)];

            List<Perk> perkPool = new List<Perk>();

            foreach (var perk in perkAssets)
            {
                string perkName = perk.name;

                bool flagged = false;

                foreach (var tag in tags)
                {
                    if (perkName.ToLower().Contains(tag.ToLower()))
                    {
                        flagged = true;
                        break;
                    }
                }

                if (flagged)
                    perkPool.Add(perk);
            }

            return perkPool[UnityEngine.Random.Range(0,perkPool.Count)];
        }
        private static GameObject GetRandomItem(string[] tags = null)
        {
            List<GameObject> itemAssets = CL_AssetManager.GetFullCombinedAssetDatabase().itemPrefabs;
            if (itemAssets == null || itemAssets.Count == 0)
            {
                Debug.LogError("[ChaosMod - GetRandomItem] Fail due to no/null items");
                return null;
            }

            if (tags == null)
                return itemAssets[UnityEngine.Random.Range(0, itemAssets.Count)];

            List<GameObject> itemPool = new List<GameObject>();

            foreach (var item in itemAssets)
            {
                string itemName = item.name;

                bool flagged = false;

                foreach (var tag in tags)
                {
                    if (itemName.ToLower().Contains(tag.ToLower()))
                    {
                        flagged = true;
                        break;
                    }
                }

                if (flagged)
                    itemPool.Add(item);
            }

            return itemPool[UnityEngine.Random.Range(0, itemPool.Count)];
        }
        private static BuffContainer GetBlankBuffContainer(Dictionary<string,float> buffs)
        {
            BuffContainer container = new BuffContainer();
            container.buffs = new List<BuffContainer.Buff>();

            foreach (KeyValuePair<string, float> entry in buffs)
            {
                BuffContainer.Buff buff = new BuffContainer.Buff();
                buff.id = entry.Key;
                buff.maxAmount = entry.Value;
                container.buffs.Add(buff);
            }

            container.loseOverTime = false;
            container.buffTime = 1;
            container.desc = container.id = "";

            return container;
        }
        private static List<GameObject> SpawnObjectCircle(GameObject original, Vector3 center, EventEntry entry, float amount = 10f, float radius = 2f, bool faceAway = false)
        {
            List<GameObject> objects = new List<GameObject>();
            for (int i = 0; i < amount; i++)
            {
                float angle = i * Mathf.PI * 2f / amount;

                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Vector3 spawnPos = center + offset;

                GameObject obj = Instantiate(original, spawnPos, Quaternion.identity, CL_EventManager.currentLevel.transform);
                objects.Add(obj);

                if (entry)
                    entry.relatedObjects.Add(obj);

                if (faceAway)
                    obj.transform.localRotation = Quaternion.LookRotation(spawnPos - center);
                else
                    obj.transform.localRotation = Quaternion.LookRotation(center - spawnPos);
            }
            return objects;
        }
        //Event Methods
        private static void PerkOverdose(EventEntry entry)
        {
            string[] tags =
            {
                "_C1_",
                "_Rho_",
                "_T1_",
                "_T2_",
                "_T3_",
                "_T4_",
                "_TX_"
            };

            ENT_Player.GetPlayer().AddPerk(GetRandomPerk(tags), 10);
        }
        private static void BloodbugHorde(EventEntry entry)
        {
            Vector3 center = ENT_Player.GetPlayer().transform.position;
            GameObject original = EntityHolder.GetEntityObject("denizen_bloodbug");

            SpawnObjectCircle(original, center, entry);
        }
        private static void SpawnHouseMD(EventEntry entry)
        {
            GameObject obj = Instantiate((GameObject)prefabs["House"], Camera.main.transform.position + Camera.main.transform.forward * 10, Quaternion.identity);
            entry.relatedObjects.Add(obj);

            Shader matShader = Shader.Find("Unlit/Unlit Transparent Color");

            if (matShader == null)
            {
                Debug.LogError("[ChaosMod - SpawnHouseMD] Could not find game shader!");
                return;
            }

            foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    mat.shader = matShader;
                }
            }

            AudioSource song = obj.GetComponent<AudioSource>();
            song.volume = 0.65f;
            song.outputAudioMixerGroup = AudioUtils.GetEffectsMixer();
            song.gameObject.AddComponent<AudioDistortionFilter>().distortionLevel = 0.05f;

            obj.AddComponent<HouseAI>();
        }
        private static void RandomPerk(EventEntry entry)
        {
            string[] tags =
            {
                "_C1_",
                "_Rho_",
                "_T1_",
                "_T2_",
                "_T3_",
                "_T4_",
                "_TX_"
            };

            ENT_Player.GetPlayer().AddPerk(GetRandomPerk(tags));
        }
        private static void RandomTrinket(EventEntry entry)
        {
            string[] tags =
            {
                "_Trinket_"
            };
            
            Item item = GetRandomItem(tags).GetComponent<Item_Object>().itemData;
            ENT_Player.GetInventory().AddItemToInventoryScreen(new Vector3(0f, 0f, 1f) + UnityEngine.Random.insideUnitSphere * 0.01f, item, true, true, true);
        }
        private static void RandomBinding(EventEntry entry)
        {
            string[] tags =
            {
                "_Binding_"
            };

            Perk binding = ENT_Player.GetPlayer().AddPerk(GetRandomPerk(tags));
            GameObject babysitter = new GameObject();
            babysitter.AddComponent<PerkBabySitter>().StartBabySitting(binding,60f);
            entry.relatedObjects.Add(babysitter);
            babysitter.name = "Binding Babysitter: "+binding.name;
        }
        private static void RandomItem(EventEntry entry)
        {
            Item item = GetRandomItem().GetComponent<Item_Object>().itemData;
            ENT_Player.GetInventory().AddItemToInventoryScreen(new Vector3(0f, 0f, 1f) + UnityEngine.Random.insideUnitSphere * 0.01f, item, true, true, true);
        }
        private static void RandomArtifact(EventEntry entry)
        {
            string[] tags =
            {
                "_Artifact_"
            };

            Item item = GetRandomItem(tags).GetComponent<Item_Object>().itemData;
            ENT_Player.GetInventory().AddItemToInventoryScreen(new Vector3(0f, 0f, 1f) + UnityEngine.Random.insideUnitSphere * 0.01f, item, true, true, true);
        }
        private static void YoinkItem(EventEntry entry)
        {
            Inventory inv = ENT_Player.GetInventory();
            if (inv.bagItems.Count == 0) return;
            PlayAudio((AudioClip)prefabs["YoinkAudio"], 0f, 0.5f, AudioUtils.GetEffectsMixer());
            Item item = inv.bagItems[UnityEngine.Random.Range(0, ENT_Player.GetInventory().bagItems.Count)];
            inv.bagItems.Remove(item);
            item.Destroy();
            inv.CalculateEncumberance();
        }
        private static void IronKnuckle(EventEntry entry)
        {
            ENT_Player.GetPlayer().RemoveAllPerks(false);
        }
        private static void Jumpscare(EventEntry entry)
        {
            GameObject obj = Instantiate(
                (GameObject)prefabs["Jumpscare"],
                ENT_Player.playerObject.transform.position,
                Quaternion.identity
            );

            entry.relatedObjects.Add(obj);

            obj.transform.localScale *= 6f;

            AudioSource audio = obj.GetComponentInChildren<AudioSource>();

            audio.outputAudioMixerGroup = AudioUtils.GetEffectsMixer();
            audio.volume = 0.6f;

            VideoPlayer player = obj.GetComponentInChildren<VideoPlayer>();
            player.Play();

            Renderer r = obj.GetComponent<Renderer>();
            r.material.shader = chaosBundle.LoadAsset<Shader>("VideoOverlay");

            obj.AddComponent<VideoOverlayThinker>();

            Destroy(obj, 0.25f);
        }
        private static void TurboTime(EventEntry entry)
        {
            PlayAudio((AudioClip)prefabs["TurboTime"], 0f, 0.5f, AudioUtils.GetEffectsMixer());
            ENT_Player.playerObject.AddForce(Vector3.up * 30);
        }
        private static void FeastMode(EventEntry entry)
        {
            PlayAudio((AudioClip)prefabs["FeastMode"], 0f, 0.75f, AudioUtils.GetEffectsMixer());
            GameObject go = new GameObject();
            go.AddComponent<FeastModeThinker>().entry = entry;
            entry.relatedObjects.Add(go);
            go.name = "FeastMode";
        }
        private static void SkyDiamonds(EventEntry entry)
        {
            PlayAudio((AudioClip)prefabs["SkyDiamonds"], 0f, 0.25f, AudioUtils.GetEffectsMixer());
            GameObject go = new GameObject();
            go.AddComponent<RoachRain>().entry = entry;
            entry.relatedObjects.Add(go);
            go.name = "SkyDiamonds";
        }
        private static void PirateShip(EventEntry entry)
        {
            GameObject ship = Instantiate((GameObject)prefabs["PirateShip"]);
            entry.relatedObjects.Add(ship);
            ship.transform.position = ENT_Player.GetPlayer().transform.position + (Vector3.up * 400);
            ship.GetComponent<AudioSource>().volume = 0.75f;
            ship.GetComponent<AudioSource>().outputAudioMixerGroup = AudioUtils.GetEffectsMixer();
            ship.GetComponent<AudioDistortionFilter>().distortionLevel = 0.75f;
            ship.AddComponent<PirateAI>();

            GameObject indicate = ship.transform.GetChild(0).gameObject;
            IndicatorThinker thinker = indicate.AddComponent<IndicatorThinker>();

            var renderers = ship.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.white * 0.75f);
                }
            }
        }
        private static void PlayerLaunch(EventEntry entry)
        {
            PlayAudio((AudioClip)prefabs["Yahoo"], 0.3f, 0.4f, AudioUtils.GetEffectsMixer());
            ENT_Player.playerObject.AddForce((ENT_Player.GetPlayer().transform.forward * 7) + (Vector3.up * 2));
        }
        private static void SpawnBuddies(EventEntry entry)
        {
            Vector3 center = ENT_Player.GetPlayer().transform.position;

            float amount = 6;
            if (Main.hardMode)
                amount = 15;

            GameObject original = EntityHolder.GetEntityObject("denizen_drone_buddy");

            if (original == null)
            {
                Debug.LogError("[ChaosMod - SpawnBuddies] Unable to find drone buddy (denizen_drone_buddy)");
                return;
            }

            SpawnObjectCircle(original, center, entry, amount);
        }
        private static void SpawnFurniture(EventEntry entry)
        {
            float amount = 15;
            if (Main.hardMode)
                amount = 30;

            List<GameObject> propList = EntityHolder.GetEntityObjectList("prop_");

            if (propList.Count == 0)
            {
                Debug.LogError("[ChaosMod - SpawnFurniture] Unable to find any props");
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                GameObject copy = Instantiate(propList[UnityEngine.Random.Range(0, propList.Count)],CL_EventManager.currentLevel.transform);
                entry.relatedObjects.Add(copy);

                float x = UnityEngine.Random.Range(1f, 3f);
                float z = UnityEngine.Random.Range(1f, 3f);

                if (UnityEngine.Random.value > 0.5f)
                    x = -x;

                if (UnityEngine.Random.value > 0.5f)
                    z = -z;

                copy.transform.position = ENT_Player.GetPlayer().transform.position + new Vector3(x,0,z);
            }
        }
        private static void OldSpiceTrain(EventEntry entry)
        {
            GameObject train = Instantiate((GameObject)prefabs["Train"]);
            entry.relatedObjects.Add(train);

            Vector2 circle = UnityEngine.Random.insideUnitCircle.normalized;

            Vector3 randomDirection = new Vector3(circle.x, 0, circle.y).normalized;
            train.transform.position = ENT_Player.GetPlayer().transform.position + (Vector3.up / 2) + (randomDirection * 200);
            train.transform.rotation = Quaternion.LookRotation(-randomDirection,Vector3.up) * Quaternion.Euler(-90f,0f,0f);

            var renderers = train.transform.GetComponents<Renderer>();
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.red * 1f);
                }
            }

            AudioSource song = train.GetComponent<AudioSource>();
            song.volume = 0.3f;
            song.outputAudioMixerGroup = AudioUtils.GetEffectsMixer();
            song.clip = (AudioClip)prefabs["OldSpice" + UnityEngine.Random.Range(1, 4).ToString()];
            train.AddComponent<AudioDistortionFilter>().distortionLevel = 0.025f;
            song.Play();

            train.AddComponent<TrainAI>();

            GameObject terry = train.transform.GetChild(0).gameObject;
            terry.AddComponent<LookAtCamera>();

            GameObject indicate = train.transform.GetChild(1).gameObject;
            IndicatorThinker thinker = indicate.AddComponent<IndicatorThinker>();
            thinker.baseScale = 0.002f;
            thinker.disappearingDistance = 40f;

            Shader matShader = Shader.Find("Unlit/Unlit Transparent Color");

            foreach (var renderer in terry.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    mat.shader = matShader;
                }
            }
        }
        private static void JoeBiden(EventEntry entry)
        {
            GameObject obj = Instantiate(
                (GameObject)prefabs["JoeBiden"],
                ENT_Player.playerObject.transform.position,
                Quaternion.identity
            );
            entry.relatedObjects.Add(obj);

            obj.transform.localScale *= 5f;

            obj.GetComponentInChildren<AudioSource>().volume = 0.25f;
            obj.GetComponentInChildren<AudioSource>().outputAudioMixerGroup = AudioUtils.GetEffectsMixer();

            VideoPlayer player = obj.GetComponentInChildren<VideoPlayer>();
            player.Play();

            Renderer r = obj.GetComponent<Renderer>();
            r.material.shader = chaosBundle.LoadAsset<Shader>("VideoOverlay");

            obj.AddComponent<VideoOverlayThinker>();

            Destroy(obj, 4f);
        }
        private static void RedCarpet(EventEntry entry)
        {
            Transform player = ENT_Player.GetPlayer().transform;
            int size = 1;

            GameObject original = EntityHolder.GetEntityObject("denizen_roach_explosive");

            if (original == null)
            {
                Debug.LogError("[ChaosMod - RedCarpet] Unable to find explosive roach (denizen_roach_explosive)");
                return;
            }

            for (float x = -size; x <= size; x++)
            {
                for (float z = -size; z <= size; z++)
                {
                    GameObject obj = Instantiate(original, player.position + player.forward + new Vector3(x/size, -0.5f,z/size),Quaternion.identity,CL_EventManager.currentLevel.transform);
                    entry.relatedObjects.Add(obj);
                }
            }
        }
        private static void HappyBirthday(EventEntry entry)
        {
            PlayAudio((AudioClip)prefabs["BDay"], 0.08f, 0.5f, AudioUtils.GetEffectsMixer());
            GameObject go = new GameObject();
            go.AddComponent<GiftRain>().entry = entry;
            entry.relatedObjects.Add(go);
            go.name = "HappyBirthday";
        }
        private static void BBQChickenAlert(EventEntry entry)
        {
            PlayAudio((AudioClip)prefabs["BBQChicken"], 0.35f, 0.6f, AudioUtils.GetEffectsMixer());
            GameObject go = new GameObject();
            go.AddComponent<BBQChickenThinker>().entry = entry;
            entry.relatedObjects.Add(go);
            go.name = "BBQChickenAlert";
        }
        private static void DoubleRandomEvent(EventEntry entry)
        {
            RandomEvent(true);
            RandomEvent(true);
        }
        private static void SpawnShrek(EventEntry entry)
        {
            GameObject obj = Instantiate((GameObject)prefabs["Shrek"], Camera.main.transform.position + (Vector3.down * 15), Quaternion.identity);
            entry.relatedObjects.Add(obj);

            Shader matShader = Shader.Find("Unlit/Unlit Transparent Color");

            if (matShader == null)
            {
                Debug.LogError("[ChaosMod - SpawnShrek] Could not find game shader!");
                return;
            }

            foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    mat.shader = matShader;
                }
            }

            AudioSource song = obj.GetComponent<AudioSource>();
            song.volume = 0.6f;
            obj.GetComponentInChildren<AudioSource>().outputAudioMixerGroup = AudioUtils.GetEffectsMixer();
            song.gameObject.GetComponent<AudioDistortionFilter>().distortionLevel = 0.35f;

            obj.AddComponent<ShrekAI>();
        }
        private static void PropMagnet(EventEntry entry)
        {
            List<CL_Prop> props = FindObjectsByType<CL_Prop>(FindObjectsSortMode.None).ToList();
            foreach (var prop in props)
            {
                Rigidbody rb = prop.transform.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce((ENT_Player.GetPlayer().transform.position - prop.transform.position) * 150 * rb.mass);
                }
            }
        }
        private static void ButterFingers(EventEntry entry)
        {
            PlayAudio((AudioClip)prefabs["ButterfingerSlip"], 0f, 0.5f, AudioUtils.GetEffectsMixer());
            ENT_Player.GetPlayer().DropHang();
            ENT_Player.GetInventory().DropItemFromHand(Camera.main.transform.position + Camera.main.transform.forward, 0);
            ENT_Player.GetInventory().DropItemFromHand(Camera.main.transform.position + Camera.main.transform.forward, 1);
        }
        private static void Drunk(EventEntry entry)
        {
            GameObject go = new GameObject();
            go.AddComponent<DrunkTimer>();
            entry.relatedObjects.Add(go);
            go.name = "Drunk";


            Dictionary<string, float> buffs = new Dictionary<string, float>();
            buffs["intoxication"] = 15f;
            BuffContainer container = GetBlankBuffContainer(buffs);
            ENT_Player.GetPlayer().AddNewBuff(container);
            GameObject babysitter = new GameObject();
            babysitter.AddComponent<BuffBabySitter>().StartBabySitting(container, 15f);
            entry.relatedObjects.Add(babysitter);
            babysitter.name = "Buff Babysitter: Drunk";
        }
        private static void LowGravity(EventEntry entry)
        {
            Dictionary<string,float> buffs = new Dictionary<string,float>();
            buffs["addGravity"] = -0.5f;
            BuffContainer container = GetBlankBuffContainer(buffs);
            ENT_Player.GetPlayer().AddNewBuff(container);
            GameObject babysitter = new GameObject();
            babysitter.AddComponent<BuffBabySitter>().StartBabySitting(container, 15f);
            entry.relatedObjects.Add(babysitter);
            babysitter.name = "Buff Babysitter: Low Gravity";
        }
        private static void TurretCircle(EventEntry entry)
        {
            Vector3 center = ENT_Player.GetPlayer().transform.position;
            GameObject original = EntityHolder.GetEntityObject("denizen_turret_basic");

            SpawnObjectCircle(original, center, entry, 5f, 3f, true);
        }
        private static void PropLoot(EventEntry entry)
        {
            GameObject level = CL_EventManager.currentLevel.gameObject;
            CL_Prop[] props = level.GetComponentsInChildren<CL_Prop>();

            foreach (CL_Prop prop in props)
            {
                if (prop.GetComponent<Item_Object>() != null)
                    continue;
                GameObject item = Instantiate(GetRandomItem(),prop.transform.position,Quaternion.identity);
                Destroy(prop.gameObject);
            }
        }
    }
    public struct Event
    {
        public string name;
        public float time;
        public Action<EventEntry> action;
        public int cooldown;
        public Event SetEntry(string name, float time, Action<EventEntry> action, int cooldown = 0)
        {
            this.name = name;
            this.time = time;
            this.action = action;
            this.cooldown = cooldown;
            return this;
        }
    }
    public class HouseAI : MonoBehaviour
    {
        private float timeLeft = 30f;
        void Update()
        {
            Vector3 toCamera = Camera.main.transform.position - transform.position;
            toCamera.Normalize();

            transform.rotation = Quaternion.LookRotation(toCamera,Vector3.up) * Quaternion.Euler(90f, 0f, 0f);

            if (Main.hardMode)
                toCamera *= 3;
            transform.position += toCamera * 3 * Mathf.Clamp(Vector3.Distance(transform.position,Camera.main.transform.position)/20,1f,10f) * Time.deltaTime;

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
                Destroy(gameObject);
        }
    }
    public class VideoOverlayThinker : MonoBehaviour
    {
        void LateUpdate()
        {
            if (Camera.main == null) return;

            transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 8);

            Vector3 toCamera = Camera.main.transform.position - transform.position;

            transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
        }
    }
    public class FeastModeThinker : MonoBehaviour
    {
        private float timeLeft = 20f;
        private float nextTick = 20f;
        private static Transform player;
        private readonly static List<GameObject> spawns = new List<GameObject>();
        public EventEntry entry = null;
        void Awake()
        {
            if (player == null)
                player = ENT_Player.GetPlayer().gameObject.transform;
            spawns.Add(EntityHolder.GetEntityObject("item_beans"));
            spawns.Add(EntityHolder.GetEntityObject("item_food_bar"));
            spawns.Add(EntityHolder.GetEntityObject("item_food_cookie"));
        }
        void Update()
        {
            if (nextTick >= timeLeft)
            {
                DropFood();
                nextTick -= 0.75f;
            }

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                Destroy(gameObject);
            }
        }
        void DropFood()
        {
            GameObject item = spawns[UnityEngine.Random.Range(0,spawns.Count)];
            GameObject copy = Instantiate(item);
            if (entry != null)
                entry.relatedObjects.Add(copy);
            copy.transform.position = player.position + new Vector3(UnityEngine.Random.Range(-2f, 2f), -1, UnityEngine.Random.Range(-2f, 2f));
            copy.transform.parent = CL_EventManager.currentLevel.transform;
        }
    }
    public class RoachRain : MonoBehaviour
    {
        private float timeLeft = 20f;
        private float nextTick = 20f;
        private static Transform player;
        private static GameObject roach = null;
        public EventEntry entry = null;
        void Awake()
        {
            if (player == null)
                player = ENT_Player.GetPlayer().gameObject.transform;
            if (roach == null)
            {
                for (int i = 0; i < CL_AssetManager.GetFullCombinedAssetDatabase().entityPrefabs.Count; i++)
                {
                    if (CL_AssetManager.GetFullCombinedAssetDatabase().entityPrefabs[i].name.ToLower() == "denizen_roach_platinum")
                    {
                        roach = CL_AssetManager.GetFullCombinedAssetDatabase().entityPrefabs[i];
                        break;
                    }
                }
            }
        }
        void Update()
        {
            if (nextTick >= timeLeft)
            {
                GameObject copy = Instantiate(roach);
                if (entry != null)
                    entry.relatedObjects.Add(copy);
                copy.transform.position = player.position + new Vector3(UnityEngine.Random.Range(-5f, 5f), 8, UnityEngine.Random.Range(-5f, 5f));
                copy.transform.parent = CL_EventManager.currentLevel.transform;
                nextTick -= 0.15f;
            }

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
    public class PirateAI : MonoBehaviour
    {
        public static Transform player;
        private bool pDead = false;
        private AudioSource song = null;
        void Awake()
        {
            if (player == null)
                player = ENT_Player.GetPlayer().transform;
            song = gameObject.GetComponent<AudioSource>();
        }
        void Update()
        {
            if (Main.hardMode)
                transform.position -= Vector3.up * 50 * Time.deltaTime;
            else
                transform.position -= Vector3.up * 30 * Time.deltaTime;

            float diff = player.position.y - transform.position.y;

            if (Vector3.Distance(player.position, transform.position) < 2.5f && !pDead)
            {
                pDead = true;
                CL_GameManager.DeathType pirateDeath = new CL_GameManager.DeathType();
                pirateDeath.deathText = "DEAD MEN TELL NO TALES";
                CL_GameManager.gMan.deathTypes[0] = pirateDeath;
                EventManager.PlayAudio((AudioClip)EventManager.prefabs["ShipCollide"], 0.5f, 1f, AudioUtils.GetEffectsMixer());
                Damageable.DamageInfo info = Damageable.DamageInfo.CreateDamageInfo(1f, "Ghost Ship", new List<string>(), null);
                ENT_Player.GetPlayer().Kill(info.type, info);
            }
            if (diff < -50)
                transform.position = new Vector3(player.position.x, transform.position.y, player.position.z);
            if (diff > 400 && song != null && song.volume > 0)
                song.volume -= Time.deltaTime / 4;
            if (diff > 800)
                Destroy(gameObject);
        }
    }
    public class TrainAI : MonoBehaviour
    {
        private static Transform player = null;
        private AudioSource song = null;
        private Vector3 dir = Vector3.up;
        private bool pDead = false;
        private bool passed = false;
        void Awake()
        {
            if (player == null)
                player = ENT_Player.GetPlayer().transform;
            song = transform.GetComponent<AudioSource>();
            dir = (player.position + (Vector3.up / 2) - transform.position).normalized;
        }
        void Update()
        {
            if (Main.hardMode)
                transform.position += dir * 75 * Time.deltaTime;
            else
                transform.position += dir * 50 * Time.deltaTime;

            transform.rotation = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(-90f, 0f, 0f);

            float dist = Vector3.Distance(transform.position, player.position + (Vector3.up/2));

            if (!pDead && !passed)
                transform.GetComponent<AudioDistortionFilter>().distortionLevel = Mathf.Clamp(dist/1000,0f,0.2f) + 0.75f;

            if (Vector3.Distance(transform.position + (Vector3.up/2), player.position) < 1.5f && !pDead && !ENT_Player.GetPlayer().IsDead())
            {
                pDead = true;
                CL_GameManager.DeathType spiceDeath = new CL_GameManager.DeathType();
                spiceDeath.deathText = "TOO MUCH OLD SPICE";
                CL_GameManager.gMan.deathTypes[0] = spiceDeath;
                EventManager.PlayAudio((AudioClip)EventManager.prefabs["TrainHit"], 0.75f, 0.9f);
                song.volume = 0.75f;
                ENT_Player.GetPlayer().Kill();
            }

            if (dist > 40f && !passed)
            {
                Vector3 toPlayer = player.position + (Vector3.up / 2) - transform.position;
                toPlayer.y = toPlayer.y/2;
                dir = toPlayer.normalized;
            }
            else
                passed = true;
            if (dist > 40f && passed && song != null && song.volume > 0)
                transform.GetComponent<AudioDistortionFilter>().distortionLevel -= Time.deltaTime / 5;
            if (dist > 600)
                Destroy(gameObject);
        }
    }
    public class LookAtCamera : MonoBehaviour
    {
        void Update()
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
        }
    }
    public class GiftRain : MonoBehaviour
    {
        private float timeLeft = 10f;
        private float nextTick = 10f;
        private static Transform player;
        public EventEntry entry = null;
        private List<GameObject> giftList = new List<GameObject>();
        void Awake()
        {
            if (player == null)
                player = ENT_Player.GetPlayer().gameObject.transform;
            giftList = EntityHolder.GetEntityObjectList("present");
            if (giftList.Count == 0)
                Debug.LogError("[ChaosMod - GiftRain] Unable to find any presents");
        }
        void Update()
        {
            if (nextTick >= timeLeft)
            {
                nextTick -= 0.35f;
                if (giftList.Count > 0)
                {
                    GameObject copy = Instantiate(giftList[UnityEngine.Random.Range(0, giftList.Count)]);
                    if (entry != null)
                        entry.relatedObjects.Add(copy);
                    copy.transform.position = player.position + new Vector3(UnityEngine.Random.Range(-5f, 5f), 8, UnityEngine.Random.Range(-5f, 5f));
                    copy.transform.parent = CL_EventManager.currentLevel.transform;
                }
            }

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
    public class BBQChickenThinker : MonoBehaviour
    {
        private float waitTime = 3f;
        private float timeLeft = 9f;
        private float nextTick = 9f;
        private static Transform player;
        private List<GameObject> platforms = new List<GameObject>();
        private static Shader matShader = null;
        public EventEntry entry = null;
        void Awake()
        {
            if (player == null)
                player = ENT_Player.GetPlayer().gameObject.transform;

            if (matShader == null)
                matShader = Shader.Find("Unlit/Unlit Transparent Color");
        }
        void Update()
        {
            if (waitTime > 0f)
            {
                waitTime -= Time.deltaTime;
                return;
            }

            if (nextTick >= timeLeft)
            {
                Vector2 circle = UnityEngine.Random.insideUnitCircle.normalized * 3;
                GameObject copy = Instantiate((GameObject)EventManager.prefabs["BBQChickenPlatform"]);
                if (entry != null)
                    entry.relatedObjects.Add(copy);
                copy.transform.position = player.position + new Vector3(circle.x, UnityEngine.Random.Range(-2f, 6f), circle.y);
                copy.AddComponent<LookAtCamera>();
                platforms.Add(copy);
                foreach (var renderer in copy.GetComponentsInChildren<Renderer>())
                {
                    foreach (var mat in renderer.materials)
                    {
                        mat.shader = matShader;
                    }
                }
                nextTick -= 0.2f;
            }

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                foreach (GameObject platform in platforms)
                {
                    Destroy(platform);
                }
                Destroy(gameObject);
            }
        }
    }
    public class ShrekAI : MonoBehaviour
    {
        private float timeLeft = 25f;
        private Vector3 moveDir = Vector3.zero;
        void Update()
        {
            Vector3 toCamera = Camera.main.transform.position - transform.position;
            toCamera.Normalize();

            transform.rotation = Quaternion.LookRotation(toCamera, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);

            if (Main.hardMode)
                moveDir = Vector3.Lerp(moveDir, toCamera, Time.deltaTime * 1.5f);
            else
                moveDir = Vector3.Lerp(moveDir, toCamera, Time.deltaTime / 1.5f);

            float multi = 12f;
            if (Main.hardMode)
                multi = 15f;
            transform.position += moveDir * multi * Time.deltaTime;

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
                Destroy(gameObject);
        }
    }
    public class DrunkTimer : MonoBehaviour
    {
        private float timeLeft = 15f;
        private float nextTick = 15f;
        private float tickDist = 5f;
        void Awake()
        {
            if (Main.hardMode)
                tickDist = 3f;
        }
        void Update()
        {
            if (nextTick >= timeLeft)
            {
                nextTick -= tickDist;
                ChaosUI.instance.FlashScreen(new Color(0,0,0,0.75f));
                ENT_Player.GetPlayer().SetPlayerRotation(new Vector3(UnityEngine.Random.Range(-90f, 90f), UnityEngine.Random.Range(0f, 360f), 0));
            }

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
    public class IndicatorThinker : MonoBehaviour
    {
        public float baseScale = 1f;
        public float disappearingDistance = 25f;
        private Vector3 pLast = Vector3.zero;
        void Start()
        {
            pLast = Camera.main.transform.position;
        }
        void Update()
        {
            pLast = Vector3.Lerp(pLast, Camera.main.transform.position,0.2f);
            transform.rotation = Quaternion.LookRotation(pLast - transform.position, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
            float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
            transform.localScale = Vector3.one * baseScale * Mathf.Clamp(1000 / dist, 0.5f, 2f);
            if (dist < disappearingDistance)
            {
                Destroy(gameObject);
            }
        }
    }
    public class PerkBabySitter : MonoBehaviour
    {
        private Perk baby = null;
        private float timeLeft;
        private bool awoken = false;
        public void StartBabySitting(Perk perk, float time = 60f)
        {
            baby = perk;
            timeLeft = time;
            awoken = true;
        }
        void Update()
        {
            if (awoken)
            {
                timeLeft -= Time.deltaTime;
                if (timeLeft <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
        void OnDestroy()
        {
            if (baby != null)
                ENT_Player.GetPlayer().RemovePerk(baby, false);
        }
    }
    public class BuffBabySitter : MonoBehaviour
    {
        private BuffContainer baby = null;
        private float timeLeft;
        private bool awoken = false;
        public void StartBabySitting(BuffContainer buff, float time = 60f)
        {
            baby = buff;
            timeLeft = time;
            awoken = true;
        }
        void Update()
        {
            if (awoken)
            {
                timeLeft -= Time.deltaTime;
                if (timeLeft <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
        void OnDestroy()
        {
            if (baby != null)
                ENT_Player.GetPlayer().RemoveBuff(baby);
        }
    }
    public static class EntityHolder
    {
        public static List<GameObject> GetEntityObjectList(string containing)
        {
            List<GameObject> list = new List<GameObject>();

            foreach (var item in CL_AssetManager.GetFullCombinedAssetDatabase().entityPrefabs)
            {
                if (item.name.ToLower().Contains(containing.ToLower()))
                    list.Add(item);
            }

            return list;
        }
        public static GameObject GetEntityObject(string name)
        {
            foreach (GameObject item in CL_AssetManager.GetFullCombinedAssetDatabase().entityPrefabs)
            {
                if (item.name.ToLower() == name.ToLower())
                    return item;
            }

            return null;
        }
    }

    public static class AudioUtils
    {
        private static AudioMixerGroup Announcer;
        private static AudioMixerGroup Effects;
        public static AudioMixerGroup GetAnnouncerMixer()
        {
            if (Announcer == null)
            {
                foreach (AudioMixerGroup group in GetAllMixerGroups())
                {
                    if (group.name == "Announcer")
                    {
                        Announcer = group;
                        break;
                    }
                }
            }
            return Announcer;
        }
        public static AudioMixerGroup GetEffectsMixer()
        {
            if (Effects == null)
            {
                foreach (AudioMixerGroup group in GetAllMixerGroups())
                {
                    if (group.name == "Effects")
                    {
                        Effects = group;
                        break;
                    }
                }
            }
            return Effects;
        }
        private static AudioMixerGroup[] GetAllMixerGroups()
        {
            return Resources.FindObjectsOfTypeAll<AudioMixerGroup>().OrderBy(g => g.audioMixer.name).ThenBy(g => g.name).ToArray();
        }
    }
}
