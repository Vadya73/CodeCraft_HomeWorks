using Modules.UI;
using UnityEngine;

namespace Game
{
    public sealed class EnemyScorePresenter : MonoBehaviour
    {
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private ScoreView _scoreView;

        private int _score;

        private void Awake()
        {
            _scoreView.SetValue(_score);
        }

        private void OnEnable()
        {
            _enemySpawner.EnemyDestroyed += OnEnemyDestroyed;
        }

        private void OnDisable()
        {
            _enemySpawner.EnemyDestroyed -= OnEnemyDestroyed;
        }

        private void OnEnemyDestroyed(Enemy _)
        {
            _score++;
            _scoreView.SetValue(_score);
        }
    }
}
