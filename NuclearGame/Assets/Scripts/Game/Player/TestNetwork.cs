using PurrNet;
using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    public class TestNetwork : NetworkIdentity
    {
        [SerializeField] private NetworkIdentity _networkIdentity;
        void Start()
        {
        
        }

        void Update()
        {
        
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void OnSpawned()
        {
            base.OnSpawned();

            Instantiate(_networkIdentity, Vector3.zero, quaternion.identity);
        }
    }
}
