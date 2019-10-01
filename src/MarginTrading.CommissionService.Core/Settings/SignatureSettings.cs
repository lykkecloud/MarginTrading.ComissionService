// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace MarginTrading.CommissionService.Core.Settings
{
    [UsedImplicitly]
    public class SignatureSettings
    {
        public CryptoKeySource KeySource { get; set; }
        public string PublicKeyPath { get; set; }
        public string PrivateKeyPath { get; set; }
        public bool ShouldGenerateKey { get; set; }
        public bool ShouldValidateSignature { get; set; }
    }
}