using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Architecture.Core
{
    public class SceneLoader : MonoBehaviour
    {
        private static readonly int Start = Animator.StringToHash("Start");

        [SerializeField] private Animator _transition;
        [SerializeField] private float _transitionTime = 1f;

        // Make this public so it can be called from other scripts
        public void LoadScene(int buildIndex)
        {
            gameObject.SetActive(true);
            StartCoroutine(LoadSceneWithTransition(buildIndex));
        }

        // Overload for loading by name
        public void LoadScene(string sceneName)
        {
            gameObject.SetActive(true);
            StartCoroutine(LoadSceneWithTransition(sceneName));
        }

        private IEnumerator LoadSceneWithTransition(int buildIndex)
        {
            // Play transition animation first (fade out)
            PlayTransition(Start);

            // Wait for transition animation to finish before loading
            yield return new WaitForSeconds(_transitionTime);

            // Load the scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex);
            if (asyncLoad != null)
            {
                asyncLoad.allowSceneActivation = true;

                // Wait until the scene is fully loaded
                while (asyncLoad is { isDone: false })
                {
                    yield return null;
                }
            }
        }

        private IEnumerator LoadSceneWithTransition(string sceneName)
        {
            // Play transition animation first (fade out)
            PlayTransition(Start);

            // Wait for transition animation to finish before loading
            yield return new WaitForSeconds(_transitionTime);

            // Load the scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            if (asyncLoad != null)
            {
                asyncLoad.allowSceneActivation = true;

                // Wait until the scene is fully loaded
                while (asyncLoad is { isDone: false })
                {
                    yield return null;
                }
            }
        }

        private void PlayTransition(int triggerParameter)
        {
            if (_transition != null)
            {
                _transition.SetTrigger(triggerParameter);
            }
            else
            {
                Debug.LogWarning("Transition animator not assigned to SceneLoader!");
            }
        }
    }
}