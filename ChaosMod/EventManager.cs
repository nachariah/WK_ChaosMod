using BepInEx;
using ChaosMod.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Video;

namespace ChaosMod.Events
{
    public class EventManager : MonoBehaviour
    {
        private static List<Event> Events = new List<Event>();
        public static Dictionary<string, UnityEngine.Object> prefabs = new Dictionary<string, UnityEngine.Object>();
        public static AssetBundle chaosBundle = null;
        public static void FillList()
        {
            Events.Clear();
            //0-10
            /*
            Events.Add(new Event().SetEntry("Perk Overdose",0f, PerkOverdose)); //10 of a random perk
            Events.Add(new Event().SetEntry("Bloodbug Infestation", 0f, BloodbugHorde)); //10 Bloodbugs
            Events.Add(new Event().SetEntry("House M.D.", 30f, SpawnHouseMD)); //House M.D.
            Events.Add(new Event().SetEntry("Random Perk", 0f, RandomPerk));
            Events.Add(new Event().SetEntry("Random Item", 0f, RandomItem));
            Events.Add(new Event().SetEntry("You are playing in IRON KNUCKLE mode. No perks for you!", 0f, IronKnuckle)); //Remove all Perks
            Events.Add(new Event().SetEntry("Jumpscare!", 0f, Jumpscare));
            Events.Add(new Event().SetEntry("It's Turbo Time!", 0f, TurboTime)); //Prepare for launch
            Events.Add(new Event().SetEntry("FEAST MODE ACTIVATED", 20f, FeastMode)); //Prepare for lunch
            Events.Add(new Event().SetEntry("Roach Rain", 20f, SkyDiamonds)); //Raining Plat. Roaches
            //11-19
            Events.Add(new Event().SetEntry("Yarr Harr", 0f, PirateShip)); //Incoming Pirateship
            Events.Add(new Event().SetEntry("Yahoo!", 0f, PlayerLaunch));
            Events.Add(new Event().SetEntry("Will you be my buddy?", 0f, SpawnBuddies));
            Events.Add(new Event().SetEntry("Moving Day", 0f, SpawnFurniture));
            */
            Events.Add(new Event().SetEntry("Old Spice Train", 0f, OldSpiceTrain));
            Events.Add(new Event().SetEntry("Advertisement", 4f, JoeBiden));
            Events.Add(new Event().SetEntry("The Red Carpet", 0f, RedCarpet));
            //Prop Magnet
            //Shrek
            //20-29
            //Blizzard
            //Christmas
        }
        public static void RandomEvent()
        {
            if (chaosBundle == null)
                LoadBundle();
            if (Events.Count == 0)
                FillList();
            Event randEvent = Events[UnityEngine.Random.Range(0, Events.Count)];
            ChaosUI.instance.AddEntry(randEvent.name, randEvent.time);
            if (randEvent.action != null )
                randEvent.action();
        }
        public static void LoadBundle()
        {
            string dllPath = Assembly.GetExecutingAssembly().Location;
            string folder = Path.GetDirectoryName(dllPath);
            string path = Path.Combine(folder, "chaosassets");

            if (!File.Exists(path))
            {
                Debug.LogError($"[ChaosMod] AssetBundle not found: {path}");
                return;
            }
            else
                chaosBundle = AssetBundle.LoadFromFile(path);

            if (chaosBundle == null)
            {
                Debug.LogError("[ChaosMod] Failed to load ChaosMod AssetBundle - Make sure it's in the same folder as the .dll file and it's named 'chaosassets'");
                return;
            }
            else
                Debug.Log($"[ChaosMod] AssetBundle Loaded!");

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
        }
        public static void PlayAudio(AudioClip clip, float distortion = 0f, float volume = 1f)
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

            source.Play();

            Destroy(go, clip.length);
        }
        //Event Methods
        private static void PerkOverdose()
        {
            Perk randomPerk = CL_AssetManager.GetFullCombinedAssetDatabase().perkAssets[UnityEngine.Random.Range(0, CL_AssetManager.GetFullCombinedAssetDatabase().perkAssets.Count)];

            for (int i = 0; i < 10; i++)
            {
                ENT_Player.GetPlayer().AddPerk(randomPerk);
            }
        }
        private static void BloodbugHorde()
        {
            for (int i = 0; i < 10; i++)
            {
                CL_GameManager.gMan.SpawnEntity(new[] { "denizen_bloodbug" });
            }
        }
        private static void SpawnHouseMD()
        {
            GameObject obj = Instantiate((GameObject)prefabs["House"], Camera.main.transform.position + Camera.main.transform.forward * 10, Quaternion.identity);

            Shader matShader = Shader.Find("Unlit/Unlit Transparent Color");

            if (matShader == null)
            {
                Debug.LogError("[ChaosMod] Could not find game shader!");
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
            song.gameObject.AddComponent<AudioDistortionFilter>().distortionLevel = 0.1f;

            obj.AddComponent<HouseAI>();
        }
        private static void RandomPerk()
        {
            ENT_Player.GetPlayer().AddPerk(CL_AssetManager.GetFullCombinedAssetDatabase().perkAssets[UnityEngine.Random.Range(0, CL_AssetManager.GetFullCombinedAssetDatabase().perkAssets.Count)]);
        }
        private static void RandomItem()
        {
            Instantiate(CL_AssetManager.GetFullCombinedAssetDatabase().itemPrefabs[UnityEngine.Random.Range(0, CL_AssetManager.GetFullCombinedAssetDatabase().itemPrefabs.Count)], ENT_Player.playerObject.transform.position, Quaternion.identity);
        }
        private static void IronKnuckle()
        {
            ENT_Player.GetPlayer().RemoveAllPerks();
        }
        private static void Jumpscare()
        {
            GameObject obj = Instantiate(
                (GameObject)prefabs["Jumpscare"],
                ENT_Player.playerObject.transform.position,
                Quaternion.identity
            );

            obj.transform.localScale *= 6f;

            VideoPlayer player = obj.GetComponentInChildren<VideoPlayer>();
            player.Play();

            Renderer r = obj.GetComponent<Renderer>();
            r.material.shader = chaosBundle.LoadAsset<Shader>("VideoOverlay");

            obj.AddComponent<VideoOverlayThinker>();

            Destroy(obj, 0.25f);
        }
        private static void TurboTime()
        {
            PlayAudio((AudioClip)prefabs["TurboTime"], 0.1f);
            ENT_Player.playerObject.AddForce(Vector3.up * 30);
        }
        private static void FeastMode()
        {
            PlayAudio((AudioClip)prefabs["FeastMode"], 0.05f);
            GameObject go = new GameObject();
            go.AddComponent<FeastModeThinker>();
        }
        private static void SkyDiamonds()
        {
            PlayAudio((AudioClip)prefabs["SkyDiamonds"], 0.05f);
            GameObject go = new GameObject();
            go.AddComponent<RoachRain>();
        }
        private static void PirateShip()
        {
            GameObject ship = Instantiate((GameObject)prefabs["PirateShip"]);
            ship.transform.position = ENT_Player.GetPlayer().transform.position + (Vector3.up * 400);
            ship.AddComponent<PirateAI>();

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
        private static void PlayerLaunch()
        {
            PlayAudio((AudioClip)prefabs["Yahoo"], 0.2f);
            ENT_Player.playerObject.AddForce((ENT_Player.GetPlayer().transform.forward * 7) + (Vector3.up * 2));
        }
        private static void SpawnBuddies()
        {
            Vector3 center = ENT_Player.GetPlayer().transform.position;

            if (EntityHolder.buddy == null)
                EntityHolder.SetVariables();

            float amount = 6;
            if (Main.hardMode)
                amount = 15;

            for (int i = 0; i < amount; i++)
            {
                GameObject copy = Instantiate(EntityHolder.buddy, CL_EventManager.currentLevel.transform);

                float y = 2f;

                if (UnityEngine.Random.value > 0.5f)
                    y = -y;

                copy.transform.position = center + new Vector3(UnityEngine.Random.Range(-3f,3f),y, UnityEngine.Random.Range(-3f, 3f));
            }
        }
        private static void SpawnFurniture()
        {
            if (EntityHolder.propList.Count == 0)
                EntityHolder.SetVariables();

            float amount = 15;
            if (Main.hardMode)
                amount = 30;

            for (int i = 0; i < amount; i++)
            {
                GameObject copy = Instantiate(EntityHolder.propList[UnityEngine.Random.Range(0, EntityHolder.propList.Count - 1)],CL_EventManager.currentLevel.transform);

                float x = UnityEngine.Random.Range(1f, 3f);
                float z = UnityEngine.Random.Range(1f, 3f);

                if (UnityEngine.Random.value > 0.5f)
                    x = -x;

                if (UnityEngine.Random.value > 0.5f)
                    z = -z;

                copy.transform.position = ENT_Player.GetPlayer().transform.position + new Vector3(x,0,z);
            }
        }
        private static void OldSpiceTrain()
        {
            GameObject train = Instantiate((GameObject)prefabs["Train"]);

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
            song.clip = (AudioClip)prefabs["OldSpice" + UnityEngine.Random.Range(1, 4).ToString()];
            train.AddComponent<AudioDistortionFilter>().distortionLevel = 0.85f;
            song.Play();

            train.AddComponent<TrainAI>();

            GameObject terry = train.transform.GetChild(0).gameObject;
            terry.AddComponent<TerryAI>();

            Shader matShader = Shader.Find("Unlit/Unlit Transparent Color");

            foreach (var renderer in terry.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    mat.shader = matShader;
                }
            }
        }
        private static void JoeBiden()
        {
            GameObject obj = Instantiate(
                (GameObject)prefabs["JoeBiden"],
                ENT_Player.playerObject.transform.position,
                Quaternion.identity
            );

            obj.transform.localScale *= 5f;

            VideoPlayer player = obj.GetComponentInChildren<VideoPlayer>();
            player.Play();

            Renderer r = obj.GetComponent<Renderer>();
            r.material.shader = chaosBundle.LoadAsset<Shader>("VideoOverlay");

            obj.AddComponent<VideoOverlayThinker>();

            Destroy(obj, 4f);
        }
        private static void RedCarpet()
        {
            if (EntityHolder.explosiveRoach == null)
                EntityHolder.SetVariables();
            Transform player = ENT_Player.GetPlayer().transform;
            for (float x = -4f; x < 4f; x++)
            {
                for (float z = -4f; z < 4f; z++)
                {
                    Instantiate(EntityHolder.explosiveRoach,player.position + player.forward + new Vector3(x/4,-0.5f,z/4),Quaternion.identity,CL_EventManager.currentLevel.transform);
                }
            }
        }
    }
    public struct Event
    {
        public string name;
        public float time;
        public Action action;

        public Event SetEntry(string name, float time, Action action)
        {
            this.name = name;
            this.time = time;
            this.action = action;
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
        private static GameObject beans = null;
        private static GameObject bar = null;
        void Awake()
        {
            if (player == null)
                player = ENT_Player.GetPlayer().gameObject.transform;
            if (beans == null || bar == null)
            {
                for (int i = 0; i < CL_AssetManager.GetFullCombinedAssetDatabase().itemPrefabs.Count; i++)
                {
                    if (CL_AssetManager.GetFullCombinedAssetDatabase().itemPrefabs[i].name.ToLower() == "item_beans")
                    {
                        beans = CL_AssetManager.GetFullCombinedAssetDatabase().itemPrefabs[i];
                    } else if (CL_AssetManager.GetFullCombinedAssetDatabase().itemPrefabs[i].name.ToLower() == "item_food_bar")
                    {
                        bar = CL_AssetManager.GetFullCombinedAssetDatabase().itemPrefabs[i];
                    }
                }
            }
        }
        void Update()
        {
            if (nextTick >= timeLeft)
            {
                DropFood();
                nextTick -= 0.35f;
            }

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                Destroy(gameObject);
            }
        }
        void DropFood()
        {
            GameObject item = beans;
            if (UnityEngine.Random.value > 0.75f)
            {
                item = bar;
            }
            GameObject copy = Instantiate(item);
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
                ENT_Player.GetPlayer().Kill();
                EventManager.PlayAudio((AudioClip)EventManager.prefabs["ShipCollide"], 0.85f);
            }
            if (diff < -50)
                transform.position = new Vector3(player.position.x, transform.position.y, player.position.z);
            if (diff > 400 && song != null && song.volume > 0)
                song.volume -= Time.deltaTime/4;
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
    public class TerryAI : MonoBehaviour
    {
        void Update()
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
        }
    }
    public static class EntityHolder
    {
        public static GameObject buddy = null;
        public static GameObject explosiveRoach = null;
        public static List<GameObject> propList = new List<GameObject>();
        public static void SetVariables()
        {
            foreach (var item in CL_AssetManager.GetFullCombinedAssetDatabase().entityPrefabs)
            {
                if (item.name.ToLower() == "denizen_drone_buddy")
                    buddy = item;
                if(item.name.ToLower() == "denizen_roach_explosive")
                    explosiveRoach = item;
                if (item.name.ToLower().Contains("prop_"))
                    propList.Add(item);
            }
        }
    }
}
