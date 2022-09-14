using MessagePack.Formatters;
using PretiaArCloud.Networking;
using UnityEngine;

namespace PretiaArCloud.Samples.ShooterSample
{
    public class Initializer
    {
        [RuntimeInitializeOnLoadMethod]
        private static void InitFormatters()
        {
            FormatterResolver.Add(typeof(uint?[]), new ArrayFormatter<uint?>());
        }
    }
}