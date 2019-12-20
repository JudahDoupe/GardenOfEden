using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{

    private List<CapturePoint> _capturePoints;
    private bool _isGameInProgress;

    private void Start()
    {
        _isGameInProgress = true;
        _capturePoints = FindObjectsOfType<CapturePoint>().ToList();
    }

    private void Update()
    {
        if(_capturePoints.All(x => x.IsCaptured) && _isGameInProgress)
        {
            StartCoroutine(EndGame());
        }
    }

    private IEnumerator EndGame()
    {
        _isGameInProgress = false;

        yield return new WaitForSeconds(5);

        QuitGame();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
