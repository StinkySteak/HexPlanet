using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LevelManager;
using System.Text;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectHexa
{
    public class GUI : MonoBehaviour
    {
        public static GUI Instance;

        public GameObject ActionPanel;

        public ButtonListing ButtonListingPrefab;
        public Transform ButtonListingParent;

        readonly List<ButtonListing> SpawnedButtonListing = new();

        [Space]

        public TMP_Text ResourceText;

        [Space]
        public TMP_Text WoodText, FuelText;

        [Space]

        public Image StartOverlay;

        public RectTransform WinPanel;
        public RectTransform LostPanel;
        public TMP_Text LostReasonText;

        public Vector2 TweenedPos;

        public int MaxDesiredFuel = 6;
        int DesiredFuel = 1;

        private void Start()
        {
            OnResourceChanged();
            LeanTween.value(StartOverlay.color.a, 0, 1).setEaseInQuad().setOnUpdate((float value) => StartOverlay.color = new Color(0, 0, 0, value));
        }
        public void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        public void ReturnToMenu()
        {
            SceneManager.LoadScene("Menu");
        }

        public void OnWin()
        {
            LeanTween.move(WinPanel, TweenedPos, 1f).setEaseInQuad();
        }
        public void OnLost(string reason)
        {
            LostReasonText.text = reason;
            LeanTween.move(LostPanel, TweenedPos, 1f).setEaseInQuad();
        }

        public void SetFuel(int _value)
        {
            if (DesiredFuel + _value <= 0 || DesiredFuel + _value > MaxDesiredFuel)
                return;

            DesiredFuel += _value;

            WoodText.text = (DesiredFuel * LevelManager.Instance.WoodPerFuel).ToString(); 
            FuelText.text = DesiredFuel.ToString();
        }

        public void AddFuel()
        {
            if (LevelManager.Instance.CurrentPlayerTurn == PlayerTurn.Action)
                return;

            if (LevelManager.Instance.Wood < DesiredFuel * LevelManager.Instance.WoodPerFuel)
                return;

            LevelManager.Instance.OnWoodToFuel();
            LevelManager.Instance.AddFuel(DesiredFuel);
            LevelManager.Instance.AddWood(-DesiredFuel * LevelManager.Instance.WoodPerFuel);
        }

        private void Awake()
        {
            Instance = this;
        }

        public void EnableChoice(bool value)
        {
            ActionPanel.SetActive(value);
        }
        public void OnResourceChanged()
        {
            StringBuilder sb = new();

            sb.AppendLine($"Fuel: {LevelManager.Instance.Fuel}\nFood: {LevelManager.Instance.Food}\nWood: {LevelManager.Instance.Wood}");
            ResourceText.text = sb.ToString();
        }

        public void OnAction(ActionData actionData)
        {
            foreach (var listing in SpawnedButtonListing)
                Destroy(listing.gameObject);

            SpawnedButtonListing.Clear();

            foreach (var choice in actionData.Choices)
            {
                var go = Instantiate(ButtonListingPrefab, ButtonListingParent);

                go.Title.text = choice.Name;
                go.Description.text = choice.Description;

                go.AppendResource("Fuel", choice.Fuel);
                go.AppendResource("Food", choice.Food);
                go.AppendResource("Wood", choice.Wood);

                go.Button.onClick.AddListener(() => LevelManager.Instance.OnChoice(choice));

                SpawnedButtonListing.Add(go);
            }
        }
    }
}