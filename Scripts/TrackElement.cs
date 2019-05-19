using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace GravityRunner {

    public class TrackElement : MonoBehaviour, ICfg {
        
        #region Encoding
        public void Decode(string data) => new CfgDecoder(data).DecodeTagsFor(this);
        
        public bool Decode(string tg, string data) {
            switch (tg)  {
                case "pos": transform.localPosition = data.ToVector3(); break;
                default: return false;
            }

            return true;
        }
        
        public CfgEncoder Encode() => new CfgEncoder()
            .Add("pos", transform.localPosition);
        #endregion
    }

    [Serializable]
    public class TrackElementPool : AbstractCfg, IPEGI, IGotDisplayName
    {
        private GameController mgmt => GameController.instance;
        private GameConfiguration cfg => mgmt.configuration;

        [SerializeField] private TrackElement prefab;
        [SerializeField] private float scale = 1f;
        [SerializeField] public string tagToUse = "";
        [SerializeField] private bool canSpawnAtTop;
        [SerializeField] private float spawnChance;

        [NonSerialized] private List<TrackElement> active = new List<TrackElement>();
        [NonSerialized] private List<TrackElement> pooled = new List<TrackElement>();

        protected float lastSpawnTryPosition;

        public void Clear() {

            foreach (var e in active)
                e.gameObject.DestroyWhatever();

            foreach (var e in pooled)
                e.gameObject.DestroyWhatever();

            pooled.Clear();

            active.Clear();

            lastSpawnTryPosition = 0;
        }

        public void Clear(TrackElement element) {

            if (active.Contains(element)) {
                element.gameObject.SetActive(false);
                active.Remove(element);
                pooled.Add(element);
            }
            else
            {
                #if UNITY_EDITOR
                Debug.LogWarning("{0} does not contain {1}".F(tagToUse, element.gameObject.name));
                #endif
            }

        }

        TrackElement GetOrCreateInstance()
        {
            TrackElement inst;

            if (pooled.Count > 0)
            {
                inst = pooled[0];
                pooled.RemoveAt(0);
            }
            else
            {
                inst = UnityEngine.Object.Instantiate(prefab, GameController.instance.transform);
                inst.gameObject.tag = tagToUse;

                inst.transform.localScale = cfg.blocksSize * scale;
            }

            active.Add(inst);

            inst.gameObject.SetActive(true);

            return inst;
        }
        
        public void FixedModelUpdate(GameController controller)
        {
            var cfg = controller.configuration;

            for (int i = active.Count - 1; i >= 0; i--) {

                var a = active[i];
                var tf = a.transform;

                tf.localPosition += Vector3.back * Time.deltaTime * controller.player.speed;

                if (tf.localPosition.z < cfg.despawnZPosition) {
                    a.gameObject.SetActive(false);
                    active.RemoveAt(i);
                    pooled.Add(a);
                }

            }

            bool timeToSpawn = lastSpawnTryPosition <= mgmt.player.distanceTravaled;

            if (timeToSpawn) {

                float gap = cfg.spawnGap * (0.5f + UnityEngine.Random.Range(0f, 1f));

                lastSpawnTryPosition += gap;

                if (UnityEngine.Random.Range(0f, 1f) < spawnChance) {

                    var inst = GetOrCreateInstance();
                    var tf = inst.transform;
                    
                    tf.localPosition = Vector3.forward * (cfg.spawnZPosition + gap);

                    if (canSpawnAtTop && UnityEngine.Random.Range(0f,1f) > 0.5f) 
                        tf.localPosition += Vector3.up * cfg.spawnYPositionIfTop;
                    
                }
            }
        }
        
        #region Inspector
        public string NameForDisplayPEGI => "{0} [{1}]".F(prefab ? prefab.name : "Null", tagToUse);

        public bool Inspect()
        {
            var changed = false;
            "Prefab".edit(ref prefab).nl(ref changed);

            pegi.editTag(ref tagToUse).nl(ref changed);
            
            "{0} active /{1} pooled".F(active.Count, pooled.Count).nl();

            "Can Spawn At Top".toggleIcon(ref canSpawnAtTop).nl(ref changed);

            "Spawn Chance".edit01(ref spawnChance).nl(ref changed);

            "Scale".edit(ref scale).nl(ref changed);

            return changed;
        }
        #endregion

        #region Encoding
        public override CfgEncoder Encode()
        {
            var cody = new CfgEncoder().Add("tmr", lastSpawnTryPosition);

            foreach (var a in active) cody.Add("te", a);

            return cody;
        }

        public override bool Decode(string tg, string data) {

            switch (tg) {

                case "tmr": lastSpawnTryPosition = data.ToFloat(); break;
                case "te": GetOrCreateInstance().Decode(data); break;
                default:  Debug.LogError("Saved Progress unrecognized tag: {0}".F(tg));  return false;

            }

            return true;
        }
        #endregion
    }


}