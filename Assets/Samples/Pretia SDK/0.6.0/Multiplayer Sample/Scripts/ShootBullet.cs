using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    public class ShootBullet : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody _bulletPrefab;

        [SerializeField]
        private Transform _spawnPoint;

        [SerializeField]
        private float _speed = 10f;

        [SerializeField]
        private float _despawnAfterSeconds = 3f;

        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;

        private void FixedUpdate()
        {
            _spawnPosition = _spawnPoint.position;
            _spawnRotation = _spawnPoint.rotation;
        }

        public void Shoot()
        {
            var bullet = Instantiate(_bulletPrefab, _spawnPosition, _spawnRotation);
            bullet.AddForce(bullet.transform.forward * _speed, ForceMode.Impulse);
            Destroy(bullet.gameObject, _despawnAfterSeconds);
        }
    }
}