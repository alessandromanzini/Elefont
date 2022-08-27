using System;
namespace ElefontNETStandard.Models
{
    class QueryParameterModel
    {
        public readonly int OrderId;
        public string Parameter { get; set; }
        public string Type { get; set; }

        public QueryParameterModel(int orderId, string value)
        {
            try
            {
                var split = value?.Split(":");

                OrderId = orderId;
                Parameter = split[0];
                Type = split[1];
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException($"{ex.Source}: Query parameters are missing information. Perhaps you forgot to specify parameter value or type.");
            }
        }
    }
}

