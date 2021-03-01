// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class BbvaReportGenService : IReportGenService
    {
        public byte[] GenerateBafinCncReport(IEnumerable<CostsAndChargesCalculation> calculations)
        {
            var str = @"private string file = @""%PDF-1.3
1 0 obj
<< /Type /Catalog
/Outlines 2 0 R
/Pages 3 0 R >>
endobj
2 0 obj
<< /Type /Outlines /Count 0 >>
endobj
3 0 obj
<< /Type /Pages
/Kids [6 0 R
]
/Count 1
/Resources <<
/ProcSet 4 0 R
/Font << 
/F1 8 0 R
>>
>>
/MediaBox [0.000 0.000 612.000 792.000]
 >>
endobj
4 0 obj
[/PDF /Text ]
endobj
5 0 obj
<<
/Creator (DOMPDF)
/CreationDate (D:20210209101107+00'00')
/ModDate (D:20210209101107+00'00')
/Title (Ex Ante)
>>
endobj
6 0 obj
<< /Type /Page
/Parent 3 0 R
/Contents 7 0 R
>>
endobj
7 0 obj
<<
/Length 1347 >>
stream

0.000 0.000 0.000 rg
BT 34.016 734.579 Td /F1 12.0 Tf  [(Ex Ante feature is in development)] TJ ET

endstream
endobj
8 0 obj
<< /Type /Font
/Subtype /Type1
/Name /F1
/BaseFont /Times-Roman
/Encoding /WinAnsiEncoding
>>
endobj
xref
0 9
0000000000 65535 f 
0000000008 00000 n 
0000000073 00000 n 
0000000119 00000 n 
0000000273 00000 n 
0000000302 00000 n 
0000000435 00000 n 
0000000498 00000 n 
0000001897 00000 n 
trailer
<<
/Size 9
/Root 1 0 R
/Info 5 0 R
>>
startxref
2006
%%EOF
";
            var bytes = System.Text.Encoding.Default.GetBytes(str);
            return bytes;
        }
    }
}