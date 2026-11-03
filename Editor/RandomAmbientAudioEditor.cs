using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JanSharp
{
    public static class RandomAmbientAudioOnBuild
    {
        [OrderedInitializeOnLoad]
        private static void OnAssemblyLoad()
        {
            // Using Cumulative to have it validate and log errors for all of them
            // rather than aborting at the first invalid one.
            OnBuildUtil.RegisterTypeCumulative<RandomAmbientAudio>(OnBuild);
        }

        private static bool OnBuild(IEnumerable<RandomAmbientAudio> audios)
        {
            bool valid = true;
            foreach (RandomAmbientAudio audio in audios)
                valid &= OnBuild(audio);
            return valid;
        }

        private static bool OnBuild(RandomAmbientAudio audio)
        {
            bool valid = true;
            if ((audio.AudioClips?.Length ?? 0) != (audio.AudioClipWeights?.Length ?? 0))
            {
                Debug.LogError($"[RandomAmbientAudio] The Audio Clips and Audio Clip Weights for "
                    + $"Random Ambient Audio must have a matching amount of elements.", audio);
                valid = false;
            }
            if (audio.AudioClips?.Any(c => c == null) ?? false)
            {
                Debug.LogError($"[RandomAmbientAudio] Audio Clips for Random Ambient Audio must not contain null.", audio);
                valid = false;
            }
            return valid;
        }
    }
}
