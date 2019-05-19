using System;
using System.Collections;
using System.Collections.Generic;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace GravityRunner {

    public class PlayerController : MonoBehaviour, ICfg, IPEGI
    {
        private GameController mgmt => GameController.instance;
        private GameConfiguration cfg => mgmt.configuration;

        [NonSerialized] public float speed;
        [NonSerialized] private float yPosition;
        [NonSerialized] private float yVelocity;
        [NonSerialized] private int score;

        public int Score
        {
            get { return score; }
            set {
                score = value;
                mgmt.scoreTextController.targetValue = score;
            }
        }


        public void Clear() {
            Score = 0;
            speed = 0;
            yPosition = 0;
            yVelocity = 0;
        }

        void OnTriggerEnter(Collider other)
        {

            var tEl = other.gameObject.GetComponent<TrackElement>();

            if (!tEl) {

                #if UNITY_EDITOR
                Debug.LogWarning("Player Colliding with {0}".F(other.gameObject.name));
                unprocessedCollision.Add(other.gameObject.name + ": unrecognized Game Object");
                #endif

            } else {

                var otherTag = tEl.gameObject.tag;

                if (mgmt.dangerElements.tagToUse.Equals(otherTag)) {

                    if (Application.isEditor && cfg.debugDisableDeath) {

                        mgmt.dangerElements.Clear(tEl);
                        Debug.Log("PLayer collided with obstacle");
                    }
                    else 
                        GameController.instance.FinishGame();

                } else if (mgmt.collectibleElements.tagToUse.Equals(otherTag)) {

                    Score += (int)(1f * speed);
                    
                    mgmt.collectibleElements.Clear(tEl);

                } else if (mgmt.powerUpElemenets.tagToUse.Equals(otherTag)) {

                    mgmt.configuration.ApplySlowdown(ref speed);

                    mgmt.powerUpElemenets.Clear(tEl);

                }  else  {
                #if UNITY_EDITOR
                Debug.LogWarning("Unprocessed Track Element: {0}".F(otherTag));
                unprocessedCollision.Add(other.gameObject.name + ": unrecognized track element");
                #endif
                }
            }
        }

        public void ManagedUpdate(GameController controller) {

            var cfg = controller.configuration;

            cfg.UpdateSpeed(ref speed);

            cfg.UpdateHeight(ref yPosition, ref yVelocity, Input.GetKey(KeyCode.Space));

            var tf = transform;

            var pos = tf.position;
            pos.y = yPosition;
            tf.position = pos;

        }

        #region Inspector

        private List<string> unprocessedCollision = new List<string>();

        public bool Inspect()
        {
            var changed = false;

            "Score".edit(50, ref score).nl(ref changed);
            "Speed".edit(50, ref speed).nl(ref changed);

            if (Application.isPlaying)
                "Unprocessed collisions (Debug)".edit_List(ref unprocessedCollision).nl(ref changed);

            return changed;
        }

        #endregion

        #region Encoding
        public CfgEncoder Encode() => new CfgEncoder()
         
            .Add("sp", speed)
            .Add("yPos", yPosition)
            .Add("yVel", yVelocity)
            .Add("sc", score);
        
        public void Decode(string data) => new CfgDecoder(data).DecodeTagsFor(this);

        public bool Decode(string tg, string data) {
            switch (tg) {
               
                case "sp": speed = data.ToFloat(); break;
                case "yPos": yPosition = data.ToFloat(); break;
                case "yVel": yVelocity = data.ToFloat(); break;
                case "sc": Score = data.ToInt();
                    mgmt.scoreTextController.targetValue = score; break;
                default: return false;
            }

            return true;
        }
        #endregion
    }
}