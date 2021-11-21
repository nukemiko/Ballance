using UnityEngine;
using Ballance2.Game;

namespace Ballance2
{
    public class TimeCheck : MonoBehaviour
    {
        public GameObject currentBody = null;
        public float currentTime = 0;

        public TiggerTester startTigger = null;
        public TiggerTester endTigger = null;

        public bool isTesting = false;
        public bool isTestFinish = false;
        public float textDisplayY = 130;

        void Start()
        {
            startTigger.onTriggerEnter += (self, other) => {
                currentBody = other;
                currentTime = 0;
                startGuiTick = 50;
                isTesting = true;
                isTestFinish = false;
            };
            endTigger.onTriggerEnter += (self, other) => {
                if(currentBody == other) {
                    isTesting = false;
                    isTestFinish = true;
                    showGuiTick = 100;
                }
            };
        }
        private void FixedUpdate() 
        {
            if(isTesting) {
                currentTime += Time.fixedDeltaTime;
            }
        }

        private int showGuiTick = 60;
        private int startGuiTick = 60;
        private Rect rect = new Rect(40, 130, 100, 20);

        private void OnGUI() {
            if(isTestFinish) {
                if(showGuiTick > 0) {
                    showGuiTick --;
                    rect.y = textDisplayY;
                    GUI.Label(rect, "Time: " + currentTime + " (s) object: " + (currentBody != null ? currentBody.name : "null"));
                } else {
                    isTestFinish = false;
                }
            }
            if(startGuiTick > 0) {
                startGuiTick--;
                GUI.Label(rect, "TimeCheckStart: " + (currentBody != null ? currentBody.name : "null"));
            }
        }
    }
}
