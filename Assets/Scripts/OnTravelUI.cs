using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OnTravelUI : MonoBehaviour
{
    public OnTravelUIListing Listing;
    public float VerticalSpeed = 0.1f;
    float CurrentVerticalSpeed = 0f;

    readonly public List<OnTravelUIListing> SpawnedListing = new();

    private void Start()
    {
        CurrentVerticalSpeed = VerticalSpeed;
    }

    public void Update()
    {
        CurrentVerticalSpeed -= Time.deltaTime;

        if (CurrentVerticalSpeed < 0f)
            return;

        transform.position += Time.deltaTime * CurrentVerticalSpeed * Vector3.up;
    }
    public OnTravelUIListing Get(string resId)
    {
        foreach (var listing in SpawnedListing)
        {
            if (listing.ResourceId == resId)
                return listing;
        }

        return null;
    }
    public void Spawn(string _text, string _resourceId, int _amount)
    {
        var ui = Instantiate(Listing, transform);

        ui.Text.text = _text;
        ui.Image.sprite = WorldSpaceUI.Instance.GetSprite(_resourceId).sprite;
        ui.ResourceId = _resourceId;
        ui.SetNewValue(_amount);

        SpawnedListing.Add(ui);
    }
    
}
