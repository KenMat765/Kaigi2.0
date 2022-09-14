using MessagePack;
using PretiaArCloud.Networking;
using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    [RequireComponent(typeof(Renderer))]
    public class ShooterColor : MonoBehaviour
    {
        [SerializeField]
        private Renderer _renderer;

        public void ChangeColor(Material material)
        {
            var materials = _renderer.sharedMaterials;
            materials[1] = material;
            _renderer.sharedMaterials = materials;
        }
    }
}