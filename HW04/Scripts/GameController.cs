using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController
{
    // A finite state machine. GAME_STAT represents the state.
    public enum GAME_STAT { Non, Menu, Game, Tutorial, Pause, GameOver }
    // Update the finite state machine by GAME_SIG, which means "Game Signal".
    public enum GAME_SIG { Non, Menu, Game, Tutorial, Pause, Restart, GameOver }

    private static GAME_STAT now_state = GAME_STAT.Menu;

    public static GAME_STAT GetGameSTAT() { return now_state; }

    public static void SendSIG(GAME_SIG game_sig) {
        if (game_sig != GAME_SIG.Non) {
            UpdateState(game_sig);
        }
    }

    private static void UpdateState(GAME_SIG game_sig) {
        switch (now_state) {
            case GAME_STAT.Menu:
                if (game_sig == GAME_SIG.Game) now_state = GAME_STAT.Game;
                else if (game_sig == GAME_SIG.Tutorial) now_state = GAME_STAT.Tutorial;
                break;

            case GAME_STAT.Game:
                if (game_sig == GAME_SIG.Menu) now_state = GAME_STAT.Menu;
                else if (game_sig == GAME_SIG.Pause) now_state = GAME_STAT.Pause;
                else if (game_sig == GAME_SIG.GameOver) now_state = GAME_STAT.GameOver;
                break;

            case GAME_STAT.Tutorial:
                if (game_sig == GAME_SIG.Game) now_state = GAME_STAT.Game;
                else if (game_sig == GAME_SIG.Menu) now_state = GAME_STAT.Menu;
                break;

            case GAME_STAT.Pause:
                if (game_sig == GAME_SIG.Game) now_state = GAME_STAT.Game;
                break;

            case GAME_STAT.GameOver:
                if (game_sig == GAME_SIG.Game) now_state = GAME_STAT.Game;
                else if (game_sig == GAME_SIG.Menu) now_state = GAME_STAT.Menu;
                break;

            default:
                Debug.Log("Error!! Invalid game state: " + now_state);
                break;
        }
    }

    public static void QuitApp() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
