using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexGeneration.Models
{
    public class IndexFileDetails
    {
        public string StartDate { get; set; }
        public string IndexFileNumber { get; set; }
        public string IndexInnerFileNumberCCX4000 { get; set; }
        public string IndexInnerFileNumberCCX3000 { get; set; }
        public string IndexInnerFileNumberCCX2000 { get; set; }
        public string IndexInnerFileNumberCCX1000 { get; set; }
        public string IndexInnerPDFNumberCCX4000 { get; set; }
        public string IndexInnerPDFNumberCCX3000 { get; set; }
        public string IndexInnerPDFNumberCCX2000 { get; set; }
        public string IndexInnerPDFNumberCCX1000 { get; set; }
        public string AuditFileNumber { get; set; }
    }
}
