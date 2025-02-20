using Bronya.Entities;

using Buratino.Entities.Abstractions;

using System.Diagnostics.CodeAnalysis;

namespace Bronya.Dtos
{
    public class QueryCommand : IEquatable<QueryCommand>
    {
        public string Command { get; set; }
        public DateTime UpdateDate { get; set; }
        public Account Account { get; internal set; }

        public bool Equals(QueryCommand other) => Command == other.Command && Account == other.Account && UpdateDate == other.UpdateDate;

        public override int GetHashCode() => Account.GetHashCode();

        public override bool Equals(object obj)
        {
            return Equals(obj as QueryCommand);
        }

        public static bool operator ==(QueryCommand a, QueryCommand b) => a?.Equals(b) ?? (a is null && b is null);

        public static bool operator !=(QueryCommand a, QueryCommand b) => !a?.Equals(b) ?? (a is null ^ b is null);
    }
}