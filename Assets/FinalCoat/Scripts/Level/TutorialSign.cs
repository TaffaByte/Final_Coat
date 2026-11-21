using UnityEngine;

namespace FinalCoat{
    public class TutorialSign : MonoBehaviour{
        [TextArea] public string text;
        void OnDrawGizmos(){
            Gizmos.color = new Color(1f, 0.8f, 0.1f);
            Gizmos.DrawLine(transform.position + Vector3.down * 4f, transform.position + Vector3.up * 4f);
        }
    }
}
