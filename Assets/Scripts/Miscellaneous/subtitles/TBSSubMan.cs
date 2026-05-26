using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
namespace GrayCreature.YuriArchive.subtitlesYaoiYuriLiveReaction
{
    //atp this is just yuri script with semi decent ammount of modification kms
    // that uncommented text above is outdated Haha L
    public class TBSSubMan : MonoBehaviour 
    {
        #region SingletonSetup
        private void Awake() => Instance = this;
        public static TBSSubMan Instance;
        #endregion
        public void summonLeSubtitle(subtitlingIt subtitle, int id, subsScriptableObject subscriptobj, AudioSource audiSourc, Vector3 SubPos)
        {
            if (subsObjectIdSon[id] != null) Destroy(subsObjectIdSon[id]);
            subsObjectIdSon[id] = Instantiate<GameObject>(subtitlePrefab, subtitleCanvas.transform.position, Quaternion.identity, subtitleCanvas.transform);
            subtitlesScriptReal component = subsObjectIdSon[id].GetComponent<subtitlesScriptReal>();
            component.cameraTransf = cameraTransorm;
            component.producerAud = audiSourc;
            component.subtitlys = subtitle;
            component.is3d = component.subtitlys.isit3d;
            component.audiObject = subscriptobj;
            component.infinite = audiSourc.loop;
            component.aspectRatio = 1f;
            component.updateSubPostion();
            
        }
        public void endSubtitle(int id, bool endall = false)
        {
            if (endall) 
            {
                for (int i = 0; i < subsObjectIdSon.Length; i++) Destroy(subsObjectIdSon[i]);
                return;
            }
            if (subsObjectIdSon[id] != null) Destroy(subsObjectIdSon[id]);
        }
        public Transform cameraTransorm;
        public GameObject subtitlePrefab;
        public Canvas subtitleCanvas;
        public static int SubtitleTotalId = 512;
        public GameObject[] subsObjectIdSon = new GameObject[SubtitleTotalId];
    }
}
[Serializable]
public class subtitlingIt
{
    // hi yuri if u reading this hi
	public bool enabled = true;
	public string headText = "Placeholder";
    public bool isit3d = true;
    [Tooltip("yeah")] public bool IgnoreTimescale;
    public Vector3 twoDeePosition;
	[Min(0f)] public float duration;
	public ColorMode colorMode = ColorMode.Default;
    //public SubtitleImageStyles SubtitleStyle = SubtitleImageStyles.Default;
    public Color textColor = new Color(1f, 1f, 1f, 1f);
	[Header("rainbow and gradient color stuff")]
    public Gradient textGradient = new Gradient();
    public bool gradientAnimate = false;
    public GradientAnimationMode gradientAnimationMode = GradientAnimationMode.Rotate;
    public float gradientSpeed = 1f;
    public float rainbowSpeed = 1f;
	
	[Header("funny other options")]
	public bool undertaleStyleTextAppearing = true;
	public float TextAppearSpeed = 1f;
	public bool shakey;
	public float shakeyspeed = 1f;
    public float shakeyradius = 0.3f;
    public int fonts;
	public bool unreadable = false,upsideDown = false,textReverse = false;
	[HideInInspector] public ColorMode _FixedMode = ColorMode.Default;
    [HideInInspector] public ColorMode _GradientMode = ColorMode.Gradient;
    [HideInInspector] public ColorMode _RainbowMode = ColorMode.Rainbow;
	public enum ColorMode // joink
    {
        Default,
        Gradient,
        Rainbow
    }
    /*public enum SubtitleImageStyles
    {
        Default,
        BasicShow,
        SwordyHouse,
        HaiIdioticSchoolhouse,
        RobloxChatBubble_Light,
        RobloxChatBubble_Dark
    }*/
    public enum GradientAnimationMode
    {
        Rotate,
        ColorChanging,
        OrderLerp,
        LeftToRight,
        RightToLeft
    }
}
