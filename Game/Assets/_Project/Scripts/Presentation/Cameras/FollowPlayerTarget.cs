using UFQ.Presentation.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace UFQ.Presentation.Cameras
{
    /// <summary>
    /// Los planos de cada lugar viven en su escena, pero Mateo vive en la escena
    /// persistente «Juego»: no se puede enlazar en el editor. Al activarse, el plano
    /// busca a Mateo y lo usa como objetivo (LookAt).
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public sealed class FollowPlayerTarget : MonoBehaviour
    {
        [SerializeField] private bool alsoFollow;

        private void OnEnable() => Bind();

        private void Start() => Bind();

        private void Bind()
        {
            var vcam = GetComponent<CinemachineCamera>();
            if (vcam.LookAt != null) return;

            var player = FindFirstObjectByType<PlayerMover>();
            if (player == null) return;

            vcam.LookAt = player.transform;
            if (alsoFollow) vcam.Follow = player.transform;
        }
    }
}
