using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameController : MonoBehaviour
    {
        public void OnReplayClick()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}