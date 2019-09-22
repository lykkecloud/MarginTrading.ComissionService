// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.TestClient
{
    public class MtCoreHttpErrorResponse
    {
        public string ErrorMessage { get; set; }

        public override string ToString() => this.ErrorMessage;
    }
}