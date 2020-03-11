using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UIAction
{
    public enum UI_ACTION { Non, Menu, Game, Tutorial, Pause, Exit }

    public static void DoAction(UI_ACTION UI_action) {
        switch (UI_action) {
            case UI_ACTION.Menu:
                GameController.SendSIG(GameController.GAME_SIG.Menu);
                break;

            case UI_ACTION.Game:
                GameController.SendSIG(GameController.GAME_SIG.Game);
                break;

            case UI_ACTION.Tutorial:
                GameController.SendSIG(GameController.GAME_SIG.Tutorial);
                break;

            case UI_ACTION.Pause:
                GameController.SendSIG(GameController.GAME_SIG.Pause);
                break;

            case UI_ACTION.Exit:
                GameController.QuitApp();
                break;

            default:
                Debug.Log("Error!! Invalid UI action: " + UI_action);
                break;
        }
    }
}
