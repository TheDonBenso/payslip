using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Payslip.Model;

namespace Payslip.DataAccess
{
    public sealed class CsvEmployeeRepository : IEmployeeRepository
    {
        public IEnumerable<T> ReadRecordsFromStream<T>(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                csvReader.Configuration.RegisterClassMap<EmployeeClassMapping>();
              //  csvReader.Configuration.PrepareHeaderForMatch = (header, 0, header) => header.Replace("_", string.Empty).ToLowerInvariant();
                return csvReader.GetRecords<T>().ToArray();
            }
        }
        public byte[] WriteRecordsToBytes<T>(IEnumerable<T> records)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(records);
                streamWriter.Flush();
                return memoryStream.ToArray();
            }
        }
    }
}