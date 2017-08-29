using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XPatchLib.Json.UnitTest
{
    [PrimaryKey("AddressId")]
    public class AddressInfo : ICloneable
    {
        public Guid AddressId { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string Phone { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        /// <summary>创建作为当前实例副本的新对象。</summary>
        /// <returns>作为此实例副本的新对象。</returns>
        public object Clone()
        {
            AddressInfo result = new AddressInfo();
            result.AddressId = AddressId;
            result.City = City;
            result.Country = Country;
            result.Phone = Phone;
            result.State = State;
            result.Zip = Zip;
            return result;
        }

        /// <summary>确定指定的 <see cref="T:System.Object" /> 是否等于当前的 <see cref="T:System.Object" />。</summary>
        /// <returns>如果指定的 <see cref="T:System.Object" /> 等于当前的 <see cref="T:System.Object" />，则为 true；否则为 false。</returns>
        /// <param name="obj">与当前的 <see cref="T:System.Object" /> 进行比较的 <see cref="T:System.Object" />。</param>
        public override bool Equals(object obj)
        {
            AddressInfo c = obj as AddressInfo;
            if (c == null) return false;
            return Equals(c.AddressId, AddressId)
                   && string.Equals(c.City, City, StringComparison.Ordinal)
                   && string.Equals(c.Country, Country, StringComparison.Ordinal)
                   && string.Equals(c.Phone, Phone, StringComparison.Ordinal)
                   && string.Equals(c.State, State, StringComparison.Ordinal)
                   && string.Equals(c.Zip, Zip, StringComparison.Ordinal);
        }

        /// <summary>用作特定类型的哈希函数。</summary>
        /// <returns>当前 <see cref="T:System.Object" /> 的哈希代码。</returns>
        public override int GetHashCode()
        {
            var result = 0;
            if (AddressId != null)
                result ^= AddressId.GetHashCode();
            if (City != null)
                result ^= City.GetHashCode();
            if (State != null)
                result ^= State.GetHashCode();
            if (Zip != null)
                result ^= Zip.GetHashCode();
            if (Country != null)
                result ^= Country.GetHashCode();
            if (Phone != null)
                result ^= Phone.GetHashCode();
            return result;
        }
    }
}
