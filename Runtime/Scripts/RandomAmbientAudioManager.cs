using UdonSharp;
using UnityEngine;

namespace JanSharp
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [SingletonScript("ef424a299c66329dc924adfc99b9cec1")] // Runtime/Prefabs/Managers/RandomAmbientAudioManager.prefab
    public class RandomAmbientAudioManager : LockstepGameState
    {
        public override string GameStateInternalName => "jansharp.random-ambient-audio";
        public override string GameStateDisplayName => "Random Ambient Audio";
        public override bool GameStateSupportsImportExport => false;
        public override uint GameStateDataVersion => 0u;
        public override uint GameStateLowestSupportedDataVersion => 0u;
        public override LockstepGameStateOptionsUI ExportUI => null;
        public override LockstepGameStateOptionsUI ImportUI => null;

        [HideInInspector][SerializeField][FindInScene] private RandomAmbientAudio[] allAudio; // Game state safe since it is unchanging.

        #region GameState
        private SerializableRNG rng;
        #endregion

        private void Start()
        {
            for (int i = 0; i < allAudio.Length; i++)
                allAudio[i].Setup(i);
        }

        [LockstepEvent(LockstepEventType.OnInit)]
        public void OnInit()
        {
            rng = lockstep.NewSerializableRNG();
            foreach (RandomAmbientAudio audio in allAudio)
                SendDelayedPlayEventForAudio(audio);
        }

        private void SendDelayedPlayEventForAudio(RandomAmbientAudio audio)
        {
            uint delay = audio.GetTickDelayUntilNextPlay(rng);
            lockstep.WriteSmallUInt((uint)audio.index);
            lockstep.SendEventDelayedTicks(playRandomAmbientAudioIAId, delay);
        }

        [HideInInspector][SerializeField] private uint playRandomAmbientAudioIAId;
        [LockstepInputAction(nameof(playRandomAmbientAudioIAId))]
        public void OnPlayRandomAmbientAudioIA()
        {
            uint index = lockstep.ReadSmallUInt();
            RandomAmbientAudio audio = allAudio[index];
            audio.Play(rng);
            SendDelayedPlayEventForAudio(audio);
        }

        public override void SerializeGameState(bool isExport, LockstepGameStateOptionsData exportOptions)
        {
            lockstep.WriteCustomClass(rng);
        }

        public override string DeserializeGameState(bool isImport, uint importedDataVersion, LockstepGameStateOptionsData importOptions)
        {
            rng = (SerializableRNG)lockstep.ReadCustomClass(nameof(SerializableRNG));
            return null;
        }
    }
}
