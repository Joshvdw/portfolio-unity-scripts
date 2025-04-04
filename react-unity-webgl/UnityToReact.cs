using System.Runtime.InteropServices;
using UnityEngine;

public class UnityToReact : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void UnityToFrontend(string msg);

    public void sndMsg(string msg)
    {
        Debug.Log("msg sent from Unity: " + msg);
#if UNITY_WEBGL == true && UNITY_EDITOR == false
            UnityToFrontend (msg);
#endif
    }
}
