using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _createLobbyButton;
    [SerializeField] private Button _quickJoinButton;
    [SerializeField] private Button joinCodeButton;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private LobbyCreateUI lobbyCreateUI;

    private void Awake()
    {
        /*_mainMenuButton.onClick.AddListener(() => {
            SurvivorsEscapeLobby.Instance.LeaveLobby();
            SceneLoader.Load(SceneLoader.Scene.MainMenuScene);
        });*/
        _createLobbyButton.onClick.AddListener(() => {
            lobbyCreateUI.Show();
            //SurvivorsEscapeLobby.Instance.CreateLobby("LobbyName", false);
        });
        _quickJoinButton.onClick.AddListener(() => {
            SurvivorsEscapeLobby.Instance.QuickJoin();
        });
        joinCodeButton.onClick.AddListener(() => {
            SurvivorsEscapeLobby.Instance.JoinWithCode(joinCodeInputField.text);
        });

        //lobbyTemplate.gameObject.SetActive(false);


    }

    // TESTING
    /*[SerializeField] private Button _createGameButton;
    [SerializeField] private Button _joinGameButton;

    private void Awake()
    {
        _createGameButton.onClick.AddListener(() =>
        {
            SurvivorsEscapeMultiplayer.Instance.StartHost();
            SceneLoader.LoadNetwork(SceneLoader.Scene.CharacterSelectScene);
        });

        _joinGameButton.onClick.AddListener(() =>
        {
            SurvivorsEscapeMultiplayer.Instance.StartClient();
        });
    }*/
}
