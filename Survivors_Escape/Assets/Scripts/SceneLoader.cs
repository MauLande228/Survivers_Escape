using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public enum Scene
    {
        MultiplayerDevScene, // Main scene
        LobbyScene,
        CharacterSelectScene,
        LoadingScene
    }

    private static Scene _targetScene;

    public static void Load(Scene scene)
    {
        SceneLoader._targetScene = scene;
        SceneManager.LoadScene(SceneLoader._targetScene.ToString());
    }

    public static void LoadNetwork(Scene scene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }
}
