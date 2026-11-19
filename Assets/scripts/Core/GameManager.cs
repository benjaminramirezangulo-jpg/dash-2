using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        // Preserve the original scene as a reference; both Editor and shipped player
        // start the same campaign code without depending on generated scene assets.
        if (FindAnyObjectByType<VelocityGame>() == null)
        {
            new GameObject("Velocity Campaign").AddComponent<VelocityGame>();
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                if (root.GetComponent<VelocityGame>() == null) root.SetActive(false);
        }
    }
    [SerializeField] private Health playerHealth;
    [SerializeField] private GameObject gameOverPanel;

    private bool isGameOver;

    private void Start()
    {
        gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        playerHealth.Died += HandlePlayerDied;
    }

    private void OnDisable()
    {
        playerHealth.Died -= HandlePlayerDied;
    }

    private void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void HandlePlayerDied()
    {
        isGameOver = true;
        playerHealth.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);
    }
}
