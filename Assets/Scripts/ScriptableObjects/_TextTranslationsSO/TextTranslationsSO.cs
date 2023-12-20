using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu]
public class TextTranslationsSO : ScriptableObject, INetworkSerializable
{
    public string englishTextTranslation;
    public string russianTextTranslation;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref englishTextTranslation);
        serializer.SerializeValue(ref russianTextTranslation);
    }
}
