using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloridaCounties.DataAccess {
    //                                              Model       Provider
    //                                                |             |
    //                                                |             |
    //                                                v             v
    public class DateTimeConverter : ValueConverter<DateTime, DateTime>{
        public DateTimeConverter() : base(
            convertToProviderExpression: modelDate => modelDate.ToUniversalTime(),
            convertFromProviderExpression: providerDate => providerDate.ToLocalTime()) { 
        }
    }
}
