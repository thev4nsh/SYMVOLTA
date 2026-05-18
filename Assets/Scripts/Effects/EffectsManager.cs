using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using SYMVOLTA.Core;

namespace SYMVOLTA.Effects
{
    public class EffectsManager : Singleton<EffectsManager>
    {
        [Header("Particle Pools")]
        [SerializeField] private int defaultPoolSize = 20;
        [SerializeField] private int maxPoolSize = 100;

        private Dictionary<GameObject, ObjectPool<GameObject>> _particlePools = new Dictionary<GameObject, ObjectPool<GameObject>>();

        public void Initialize()
        {
            Debug.Log("[EffectsManager] Initialized. Ready to spawn pooled particles.");
        }

        /// <summary>
        /// Plays a particle system from the pool at a specific position and rotation.
        /// Automatically returns to pool when finished.
        /// </summary>
        public void PlayParticle(GameObject particlePrefab, Vector3 position, Quaternion rotation)
        {
            if (particlePrefab == null) return;

            ObjectPool<GameObject> pool = GetOrCreatePool(particlePrefab);
            GameObject particleObj = pool.Get();

            particleObj.transform.position = position;
            particleObj.transform.rotation = rotation;
            particleObj.SetActive(true);

            ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Clear();
                ps.Play();
                float lifetime = ps.main.duration + ps.main.startLifetime.constantMax + 0.1f;
                StartCoroutine(ReturnToPoolAfterDuration(particleObj, lifetime));
            }
            else
            {
                // Fallback if no particle system is found (auto return after 3 seconds)
                StartCoroutine(ReturnToPoolAfterDuration(particleObj, 3f));
            }
        }

        public void PlayParticle(GameObject particlePrefab, Vector3 position)
        {
            PlayParticle(particlePrefab, position, Quaternion.identity);
        }

        private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
        {
            if (!_particlePools.ContainsKey(prefab))
            {
                ObjectPool<GameObject> newPool = new ObjectPool<GameObject>(
                    createFunc: () => CreateParticleInstance(prefab),
                    actionOnGet: (obj) => obj.SetActive(true),
                    actionOnRelease: (obj) => obj.SetActive(false),
                    actionOnDestroy: (obj) => Destroy(obj),
                    collectionCheck: false,
                    defaultCapacity: defaultPoolSize,
                    maxSize: maxPoolSize
                );

                _particlePools.Add(prefab, newPool);
            }

            return _particlePools[prefab];
        }

        private GameObject CreateParticleInstance(GameObject prefab)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);

            // Add ReturnToPool component to handle automatic returning if we miss the coroutine
            PooledParticle pooled = obj.AddComponent<PooledParticle>();
            pooled.Setup(prefab, this);

            return obj;
        }

        public void ReturnToPool(GameObject prefab, GameObject obj)
        {
            if (obj == null || !obj.activeSelf) return;

            if (_particlePools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool.Release(obj);
            }
            else
            {
                Destroy(obj);
            }
        }

        private System.Collections.IEnumerator ReturnToPoolAfterDuration(GameObject obj, float duration)
        {
            yield return new WaitForSeconds(duration);

            PooledParticle pooled = obj.GetComponent<PooledParticle>();
            if (pooled != null)
            {
                ReturnToPool(pooled.OriginalPrefab, obj);
            }
            else
            {
                Destroy(obj);
            }
        }
    }

    /// <summary>
    /// Attached to pooled particles to remember their original prefab for returning to the correct pool.
    /// </summary>
    public class PooledParticle : MonoBehaviour
    {
        public GameObject OriginalPrefab { get; private set; }
        private EffectsManager _manager;

        public void Setup(GameObject prefab, EffectsManager manager)
        {
            OriginalPrefab = prefab;
            _manager = manager;
        }

        private void OnParticleSystemStopped()
        {
            if (_manager != null)
            {
                _manager.ReturnToPool(OriginalPrefab, gameObject);
            }
        }
    }
}
