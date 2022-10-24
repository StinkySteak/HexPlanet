using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;

public class WorldSpaceUI : MonoBehaviour
{
    public CanvasScaler CanvasScaler;
    public RectTransform MainCanvas;
    public OnTravelUI OnTravelUIPrefab;
    public TMP_Text NoFuelText;
    public float NoFuelAlphaLeanSpeed;
    public static WorldSpaceUI Instance;

    public RectTransform Canvas;

    public List<ResourceText> ResourceTexts = new();

    public Vector2 Offset = Vector2.up * 2;

    public ResourceSprite[] Resources;

    public float UIDuration = 3;

    [System.Serializable]
    public class ResourceSprite
    {
        public string id;
        public Sprite sprite;
    }

    public ResourceSprite GetSprite(string id)
    {
        foreach (var res in Resources)
        {
            if (res.id == id)
                return res;
        }

        return null;
    }

    public class ResourceText
    {
        public OnTravelUI UI;
        public Vector2 InitialPos;
        public float Lifetime;
        public int Turn;
        public List<string> ExistedResources = new();
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        var alpha = NoFuelText.color.a - Time.deltaTime * NoFuelAlphaLeanSpeed;
        NoFuelText.color = new Color(1, 1, 1, alpha);

        for (int i = 0; i < ResourceTexts.Count; i++)
        {
            var res = ResourceTexts[i];

            var a = res.Lifetime / UIDuration;

            res.Lifetime -= Time.deltaTime;


            foreach (var listing in res.UI.SpawnedListing)
            {
                listing.Text.color = new Color(1, 1, 1, a);
                listing.Image.color = new Color(1, 1, 1, a);
            }

            if (res.Lifetime <= 0)
            {
                Destroy(res.UI.gameObject);
                ResourceTexts.RemoveAt(i);
            }
        }
    }
    public bool TryGet(int _turn, out ResourceText resource)
    {
        foreach (var res in ResourceTexts)
        {
            if (res.Turn == _turn)
            {
                resource = res;
                return true;
            }
        }

        resource = null;
        return false;
    }
    /// <summary>
    /// Used for multiple same resource id gained at different computation time (still same turn)
    /// </summary>
    /// <param name="_turn"></param>
    /// <param name="resource"></param>
    /// <returns></returns>
    public bool TryGetResource(string id)
    {
        foreach (var res in ResourceTexts)
        {
            foreach (var existedRes in res.ExistedResources)
            {
                if (existedRes == id)
                    return true;
            }
        }

        return false;
    }

    public ResourceText GetResource(string id)
    {
        foreach (var res in ResourceTexts)
        {
            foreach (var existedRes in res.ExistedResources)
            {
                if (existedRes == id)
                    return res;
            }
        }

        return null;
    }

    public void OnResourceChanged(Vector2 _to, string _name, int _value, int _turn)
    {
        var resId = _name.ToLower();

        if (TryGet(_turn, out var res)) // if there is multiple resource changes in 1 turn
        {
            if (TryGetResource(resId)) // if there is same but multiple times of resource gained
            {
                print("IsExist");

                var existedListing = res.UI.Get(resId);

                var newAmt = _value + existedListing.CurrentAmount;
                var newText = $"{GetPrefix(newAmt)}{newAmt} {_name}";
                existedListing.Text.text = newText;
                existedListing.SetNewValue(_value);
                return;
            }

            res.UI.Spawn($"{GetPrefix(_value)}{_value} {_name}", resId, _value);
            //  GetResource(resId).ExistedResources.Add(resId);
            return;
        }

        var newUI = Instantiate(OnTravelUIPrefab, _to + Offset, Quaternion.identity);
        newUI.transform.SetParent(transform);
        newUI.Spawn($"{GetPrefix(_value)}{_value} {_name}", resId, _value);

        var textData = new ResourceText() { InitialPos = _to + Offset, UI = newUI, Lifetime = UIDuration, Turn = _turn };
        // textData.ExistedResources.Add(resId);

        ResourceTexts.Add(textData);

        static string GetPrefix(int value)
        {
            return value > 0 ? "+" : string.Empty;
        }
    }

    public void OnNoFuel(string _text)
    {
        NoFuelText.transform.position = (Vector2)Player.Instance.transform.position + Offset;
        NoFuelText.text = _text;
        NoFuelText.color = Color.white;
    }
}
