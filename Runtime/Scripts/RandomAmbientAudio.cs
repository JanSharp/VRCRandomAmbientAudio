using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonDependency(typeof(RandomAmbientAudioManager))]
    public class RandomAmbientAudio : UdonSharpBehaviour
    {
        [HideInInspector][SerializeField][SingletonReference] private LockstepAPI lockstep;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] audioClips;
        private int audioClipsCount;
        [SerializeField] private uint[] audioClipWeights;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public AudioClip[] AudioClips => audioClips;
        public uint[] AudioClipWeights => audioClipWeights;
#endif
        [Min(0f)]
        [SerializeField] private float minDelay;
        [Min(0f)]
        [SerializeField] private float maxDelay;
        private uint totalWeight;
        /// <summary>
        /// <para>Inclusive.</para>
        /// </summary>
        private uint minDelayTicks;
        /// <summary>
        /// <para>Exclusive.</para>
        /// </summary>
        private uint maxDelayTicks;
        [System.NonSerialized] public int index;

        public void Setup(int index)
        {
            this.index = index;
            audioClipsCount = audioClips.Length;

            minDelayTicks = (uint)Mathf.RoundToInt(minDelay * LockstepAPI.TickRate);
            maxDelayTicks = (uint)Mathf.RoundToInt(maxDelay * LockstepAPI.TickRate);
            if (minDelayTicks == 0u)
                minDelayTicks = 1u; // A delay of 0 ticks is invalid for lockstep delayed events.
            if (maxDelayTicks < minDelayTicks)
                maxDelayTicks = minDelayTicks;
            maxDelayTicks += 1u; // Make it exclusive.

            foreach (uint weight in audioClipWeights)
                totalWeight += weight;
        }

        public uint GetTickDelayUntilNextPlay(SerializableRNG rng)
        {
            return rng.rng.Range(minDelayTicks, maxDelayTicks);
        }

        public void Play(SerializableRNG rng)
        {
            uint chosenValue = rng.rng.Range(0u, totalWeight); // 0 based.
            uint currentWeight = 0u; // 1 based.

            // Do not actually play audio from the past.
            // But do run the random number generator, else the game state would be desynced.
            if (lockstep.IsCatchingUp)
                return;

            for (int i = 0; i < audioClipsCount; i++)
            {
                currentWeight += audioClipWeights[i];
                if (currentWeight > chosenValue) // > not >= because currentWeight is 1 based chosenValue is 0 based.
                {
                    audioSource.PlayOneShot(audioClips[i]);
                    break;
                }
            }
        }
    }
}
