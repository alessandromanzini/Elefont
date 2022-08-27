using System;
namespace Elefont
{
    public interface IQueryResponse
    {
        bool IsDBNull(int index);

        int GetInt32(int index);
        int? GetNullableInt32(int index);

        double GetDouble(int index);
        double? GetNullableDouble(int index);

        string GetString(int index);

        bool GetBoolean(int index);
        bool? GetNullableBoolean(int index);

        DateTime GetDateTime(int index);
        DateTime? GetNullableDateTime(int index);
    }
}

