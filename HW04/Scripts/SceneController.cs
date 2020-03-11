using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class SceneController
{
    public enum SCENE_ID { Non, Menu, Game }

    private static SCENE_ID now_scene = SCENE_ID.Menu;

    public static SCENE_ID NowScene() {
        return now_scene;
    }

    public static void GameScene() {
        now_scene = SCENE_ID.Game;
        SceneManager.LoadScene("Flight_Game");
    }

    public static void MenuScene() {
        now_scene = SCENE_ID.Menu;
        SceneManager.LoadScene("Start_Menu");
    }
}
